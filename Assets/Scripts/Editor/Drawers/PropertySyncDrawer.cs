using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic; // List<T> はEditorスクリプト内での一時利用のため許容
using System; // Type, Array

namespace HalloweenEditor.Drawers
{
    /// <summary>
    /// PropertySync コンポーネントのカスタムInspector.
    /// </summary>
    [CustomEditor(typeof(Halloween.Tools.PropertySync))]
    public class PropertySyncEditor : UnityEditor.Editor
    {
        private SerializedProperty _referencedObjectProp;
        private SerializedProperty _targetObjectPropertiesProp;

        // ドロップダウン表示用のメンバー情報キャッシュ
        private static Dictionary<Type, MemberInfo[]> _memberCache =
            new Dictionary<Type, MemberInfo[]>();
        
        // 80文字制限対策
        private const BindingFlags BINDING_FLAGS = BindingFlags.Public |
            BindingFlags.NonPublic | BindingFlags.Instance;

        private void OnEnable()
        {
            _referencedObjectProp = serializedObject.FindProperty("_referencedObjectProperty");
            _targetObjectPropertiesProp = serializedObject.FindProperty("_targetObjectProperties");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Reference Property", EditorStyles.boldLabel);
            
            // 参照元プロパティを描画（型フィルタなし）
            DrawObjectProperty(_referencedObjectProp, null);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Target Properties", EditorStyles.boldLabel);

            // 参照元の型を取得
            string refTypeName = _referencedObjectProp
                .FindPropertyRelative("_propertyTypeName").stringValue;
            Type refType = null;
            if (!string.IsNullOrEmpty(refTypeName))
            {
                refType = Type.GetType(refTypeName);
            }

            // ターゲットプロパティ（配列）を描画
            EditorGUILayout.PropertyField(_targetObjectPropertiesProp, true); // Foldoutを表示

            if (_targetObjectPropertiesProp.isExpanded)
            {
                EditorGUI.indentLevel++;

                int arraySize = _targetObjectPropertiesProp.arraySize;
                for (int i = 0; i < arraySize; i++)
                {
                    SerializedProperty elementProp = 
                        _targetObjectPropertiesProp.GetArrayElementAtIndex(i);
                    
                    // ターゲットプロパティを描画（参照元の型でフィルタリング）
                    DrawObjectProperty(elementProp, refType);
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// ObjectProperty シリアライズドプロパティをInspector上に描画する.
        /// </summary>
        /// <param name="property">描画対象の ObjectProperty</param>
        /// <param name="filterType">
        /// フィルタリングする型. nullでない場合, この型に代入可能なプロパティのみ表示する.
        /// </param>
        private void DrawObjectProperty(SerializedProperty property, Type filterType)
        {
            SerializedProperty targetObjProp = 
                property.FindPropertyRelative("_targetObject");
            SerializedProperty pathProp = 
                property.FindPropertyRelative("_propertyPath");
            SerializedProperty typeNameProp = 
                property.FindPropertyRelative("_propertyTypeName");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(targetObjProp);

            UnityEngine.Object targetObj = targetObjProp.objectReferenceValue;

            if (targetObj != null)
            {
                MemberInfo[] members = GetBindableMembers(
                    targetObj.GetType(), filterType);

                int memberCount = members.Length;
                if (memberCount == 0)
                {
                    string msg = (filterType != null) ? 
                        $"No properties assignable to {filterType.Name} found." :
                        "No bindable properties found.";
                    EditorGUILayout.HelpBox(msg, MessageType.Info);
                    pathProp.stringValue = string.Empty;
                    typeNameProp.stringValue = string.Empty;
                    EditorGUILayout.EndVertical();
                    return;
                }

                // string[] の作成（パフォーマンスのため Array を使用）
                string[] memberNames = new string[memberCount];
                string[] displayNames = new string[memberCount];
                int currentIndex = -1;

                for (int i = 0; i < memberCount; i++)
                {
                    memberNames[i] = members[i].Name;
                    Type memberType = GetMemberType(members[i]);
                    // 80文字制限を考慮
                    displayNames[i] = 
                        $"{members[i].Name} ({memberType.Name})";
                    
                    if (memberNames[i] == pathProp.stringValue)
                    {
                        currentIndex = i;
                    }
                }

                // 現在選択されているパスがリストにない場合（型が変更されたなど）
                if (currentIndex == -1)
                {
                    pathProp.stringValue = string.Empty;
                    typeNameProp.stringValue = string.Empty;
                }

                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup(
                    "Property", currentIndex, displayNames);

                if (EditorGUI.EndChangeCheck())
                {
                    if (newIndex >= 0 && newIndex < memberCount)
                    {
                        pathProp.stringValue = memberNames[newIndex];
                        Type newType = GetMemberType(members[newIndex]);
                        typeNameProp.stringValue = newType.AssemblyQualifiedName;
                    }
                    else
                    {
                        pathProp.stringValue = string.Empty;
                        typeNameProp.stringValue = string.Empty;
                    }
                }
            }
            else // ターゲットオブジェクトが null の場合
            {
                pathProp.stringValue = string.Empty;
                typeNameProp.stringValue = string.Empty;
            }
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 指定された型のバインド可能なメンバー（プロパティ/フィールド）を取得する.
        /// </summary>
        private MemberInfo[] GetBindableMembers(Type targetType, Type filterType)
        {
            // Editorスクリプト内でのみ使用するため, キャッシュを活用する
            if (_memberCache.ContainsKey(targetType))
            {
                // return _memberCache[targetType]; // フィルタリングが必要なためキャッシュは使えない
                // TODO: フィルタリング結果もキャッシュすると高速化できるが, 複雑になる
            }
            
            // List<T> は一時的な利用に留め, ToArray() で規約に準拠する
            List<MemberInfo> members = new List<MemberInfo>();

            // 1. プロパティの収集
            PropertyInfo[] props = targetType.GetProperties(BINDING_FLAGS);
            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo prop = props[i];
                // 読み書き可能か, 非推奨でないか
                if (!prop.CanRead || !prop.CanWrite ||
                    prop.IsDefined(typeof(ObsoleteAttribute), true))
                {
                    continue;
                }

                // 型フィルタリング
                if (filterType != null && 
                    !filterType.IsAssignableFrom(prop.PropertyType))
                {
                    continue;
                }
                members.Add(prop);
            }

            // 2. フィールドの収集
            FieldInfo[] fields = targetType.GetFields(BINDING_FLAGS);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field.IsDefined(typeof(ObsoleteAttribute), true))
                {
                    continue;
                }

                // public または [SerializeField] がついているもののみ
                if (!field.IsPublic &&
                    !field.IsDefined(typeof(SerializeField), true))
                {
                    continue;
                }
                
                // 型フィルタリング
                if (filterType != null && 
                    !filterType.IsAssignableFrom(field.FieldType))
                {
                    continue;
                }
                members.Add(field);
            }

            // _memberCache[targetType] = members.ToArray(); // フィルタなしの場合のみキャッシュ可
            return members.ToArray();
        }

        /// <summary>
        /// MemberInfo（PropertyInfo または FieldInfo）からその型を取得する.
        /// </summary>
        private Type GetMemberType(MemberInfo member)
        {
            if (member is PropertyInfo pi)
            {
                return pi.PropertyType;
            }
            if (member is FieldInfo fi)
            {
                return fi.FieldType;
            }
            return null;
        }
    }
}