using UnityEngine;
using System.Reflection;
using System; // Array

namespace Halloween.Tools
{
    /// <summary>
    /// 参照元プロパティの値を, 複数のターゲットプロパティに手動で同期する.
    /// </summary>
    /// <remarks>
    /// 同期はインスペクタのコンポーネントメニュー（歯車アイコン）から
    /// 'Apply Property Sync' を選択した時に実行される.
    /// パフォーマンスのため, リフレクション情報は実行時にキャッシュする.
    /// </remarks>
    [ExecuteInEditMode] // OnEnable での初期化のために残す
    public class PropertySync : MonoBehaviour
    {
        [SerializeField]
        private ObjectProperty _referencedObjectProperty = new ObjectProperty();

        [SerializeField]
        private ObjectProperty[] _targetObjectProperties = new ObjectProperty[0];

        // 自動同期（LateUpdate）は削除

        // Unity ライフサイクルメソッド
        private void OnEnable()
        {
            // コンポーネントが有効になったときに一度初期化する
            InitializeProperties();
        }

        /// <summary>
        /// [ContextMenu] から呼び出され, 同期を手動で実行する.
        /// </summary>
        [ContextMenu("Apply Property Sync")]
        public void ApplySync()
        {
            // 実行前に最新の状態で初期化する
            InitializeProperties();
            
            if (!_referencedObjectProperty.IsValid())
            {
                Debug.LogWarning(
                    "Reference property is not valid. Cannot apply sync.", this);
                return;
            }
            
            object currentValue = _referencedObjectProperty.GetValue();

            // 値を取得してそのまま同期する
            SyncProperties(currentValue);

#if UNITY_EDITOR
            // Editor再生中以外での実行時
            if (!Application.isPlaying)
            {
                // 変更をシーンに保存するためにダーティフラグを立てる
                UnityEditor.EditorUtility.SetDirty(this);
                
                // ターゲットオブジェクトもダーティにする
                if (_targetObjectProperties != null)
                {
                    for (int i = 0; i < _targetObjectProperties.Length; i++)
                    {
                        _targetObjectProperties[i].MarkTargetDirty();
                    }
                }
            }
#endif
        }

        /// <summary>
        /// 全てのプロパティキャッシュを初期化（または再初期化）する.
        /// </summary>
        private void InitializeProperties()
        {
            _referencedObjectProperty.Initialize();

            if (_targetObjectProperties != null)
            {
                for (int i = 0; i < _targetObjectProperties.Length; i++)
                {
                    _targetObjectProperties[i].Initialize();
                }
            }
        }

        /// <summary>
        /// 参照元の値を, 全てのターゲットに同期（代入）する.
        /// </summary>
        /// <param name="newValue">代入する新しい値</param>
        private void SyncProperties(object newValue)
        {
            if (_targetObjectProperties == null)
            {
                return;
            }

            // 規約に基づき, パフォーマンスのためforループを使用する
            for (int i = 0; i < _targetObjectProperties.Length; i++)
            {
                // SetValue内で型チェックが行われる
                _targetObjectProperties[i].SetValue(newValue);
            }
        }
    }

    /// <summary>
    /// シリアライズ可能な, 特定オブジェクトの特定プロパティ（またはフィールド）への参照.
    /// </summary>
    [System.Serializable]
    public class ObjectProperty
    {
        [SerializeField]
        private UnityEngine.Object _targetObject;

        [SerializeField]
        private string _propertyPath;

        // System.Typeはシリアライズ不可のため, 型名を文字列で保持する
        [SerializeField]
        private string _propertyTypeName;

        // リフレクション情報を実行時にキャッシュする
        [System.NonSerialized]
        private MemberInfo _cachedInfo;

        [System.NonSerialized]
        private bool _cacheInitialized = false;

        // キャッシュされたPropertyInfoから型を取得する
        private Type _cachedPropertyType = null;

        /// <summary>
        /// リフレクション情報をキャッシュする.
        /// </summary>
        public void Initialize()
        {
            _cacheInitialized = true;
            _cachedInfo = null;
            _cachedPropertyType = null;

            if (_targetObject == null || string.IsNullOrEmpty(_propertyPath))
            {
                return;
            }

            Type targetType = _targetObject.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Instance;

            // まずプロパティを探す
            _cachedInfo = targetType.GetProperty(_propertyPath, flags);

            if (_cachedInfo != null)
            {
                _cachedPropertyType = ((PropertyInfo)_cachedInfo).PropertyType;
                return;
            }

            // 次にフィールドを探す
            _cachedInfo = targetType.GetField(_propertyPath, flags);

            if (_cachedInfo != null)
            {
                _cachedPropertyType = ((FieldInfo)_cachedInfo).FieldType;
            }
            // 見つからなければ _cachedInfo は null のまま
        }

        /// <summary>
        /// この参照が有効（キャッシュ済み）か確認する.
        /// </summary>
        public bool IsValid()
        {
            if (!_cacheInitialized)
            {
                Initialize();
            }
            return _cachedInfo != null && _targetObject != null;
        }

        /// <summary>
        /// キャッシュしたリフレクション情報を用いて, プロパティ（またはフィールド）の値を取得する.
        /// </summary>
        /// <returns>取得した値. 取得失敗時は null.</returns>
        public object GetValue()
        {
            if (!IsValid())
            {
                return null;
            }

            try
            {
                if (_cachedInfo is PropertyInfo pi)
                {
                    return pi.GetValue(_targetObject);
                }
                if (_cachedInfo is FieldInfo fi)
                {
                    return fi.GetValue(_targetObject);
                }
            }
            catch (Exception e)
            {
                // ターゲットが破棄された場合など
                Debug.LogError($"GetValue failed for {_targetObject.name}.{_propertyPath}: {e.Message}");
                _cachedInfo = null; // キャッシュを無効化
            }

            return null;
        }

        /// <summary>
        /// キャッシュしたリフレクション情報を用いて, プロパティ（またはフィールド）に値を設定する.
        /// </summary>
        /// <param name="value">設定する値</param>
        public void SetValue(object value)
        {
            if (!IsValid())
            {
                return;
            }

            try
            {
                // 型チェック
                if (value != null)
                {
                    Type valueType = value.GetType();
                    if (!_cachedPropertyType.IsAssignableFrom(valueType))
                    {
                        // 参照元の型とターゲットの型が異なる（互換性がない）
                        // Debug.LogWarning($"Type mismatch: Cannot assign {valueType} to {_cachedPropertyType} on {_targetObject.name}.{_propertyPath}");
                        return;
                    }
                }
                else if (_cachedPropertyType.IsValueType)
                {
                    // null を値型（struct）には設定できない
                    // Debug.LogWarning($"Cannot assign null to value type {_cachedPropertyType} on {_targetObject.name}.{_propertyPath}");
                    return;
                }

                if (_cachedInfo is PropertyInfo pi)
                {
                    pi.SetValue(_targetObject, value);
                }
                else if (_cachedInfo is FieldInfo fi)
                {
                    fi.SetValue(_targetObject, value);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SetValue failed for {_targetObject.name}.{_propertyPath}: {e.Message}");
                _cachedInfo = null; // キャッシュを無効化
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// （Editor専用）ターゲットオブジェクトに変更があったことを通知し, 保存対象にする.
        /// </summary>
        public void MarkTargetDirty()
        {
            if (_targetObject != null)
            {
                UnityEditor.EditorUtility.SetDirty(_targetObject);
            }
        }
#endif
    }
}