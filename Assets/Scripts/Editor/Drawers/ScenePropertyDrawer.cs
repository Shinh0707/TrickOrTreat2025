#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HalloweenEditor.Drawers
{
    /// <summary>
    /// SceneProperty 構造体をインスペクタで
    /// 扱いやすく表示するためのカスタムPropertyDrawer.
    /// </summary>
    /// <remarks>
    /// シーンアセットをドラッグ＆ドロップで設定できるようにし,
    /// 内部的にはシーンパス（string）を保持する.
    /// </remarks>
    [CustomPropertyDrawer(typeof(Halloween.Utility.Property.SceneProperty))]
    public class ScenePropertyDrawer : PropertyDrawer
    {
        // プロパティのフィールド名（SceneProperty 構造体と一致させる）
        private const string ScenePathPropName = "_scenePath";
        private const string LoadModePropName = "_loadMode";

        /// <summary>
        /// カスタムGUIを描画する.
        /// </summary>
        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // SerializedProperty を取得
            var scenePathProp = property.FindPropertyRelative(ScenePathPropName);
            var loadModeProp = property.FindPropertyRelative(LoadModePropName);

            // GUIのレイアウト計算
            // 1行をシーンフィールドとロードモードフィールドに分割する
            // 全体の幅
            float totalWidth = position.width;

            // LoadMode の幅（Enumポップアップに適切な幅）
            float modeWidth = 100f;

            // Scene アセットフィールドの幅
            float sceneWidth = totalWidth - modeWidth - 5f; // 5f はマージン

            var sceneRect = new Rect(
                position.x,
                position.y,
                sceneWidth,
                position.height);

            var modeRect = new Rect(
                position.x + sceneWidth + 5f,
                position.y,
                modeWidth,
                position.height);

            // --- シーンアセットフィールドの描画 ---

            // 1. 現在のパスから SceneAsset を取得する
            SceneAsset currentScene = null;
            if (!string.IsNullOrEmpty(scenePathProp.stringValue))
            {
                currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    scenePathProp.stringValue);
            }

            // 2. ObjectField を描画する
            EditorGUI.BeginChangeCheck();
            // ラベル（"Element 0" など）を表示し, SceneAsset 型の
            // オブジェクトフィールドを描画する
            var newScene = (SceneAsset)EditorGUI.ObjectField(
                sceneRect,
                label, // プロパティのラベル（"Element 0"など）を使用
                currentScene,
                typeof(SceneAsset),
                false); // シーン内のオブジェクトは許可しない

            // 3. 変更を検知した場合、パスを更新する
            if (EditorGUI.EndChangeCheck())
            {
                if (newScene != null)
                {
                    scenePathProp.stringValue = AssetDatabase.GetAssetPath(newScene);
                }
                else
                {
                    // シーンが null に設定された（クリアされた）場合
                    scenePathProp.stringValue = string.Empty;
                }
            }

            // --- ロードモードフィールドの描画 ---

            // LoadSceneMode の Enum ポップアップを描画する
            // ラベルは不要（シーンフィールドで表示済みのため）
            EditorGUI.PropertyField(modeRect, loadModeProp, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
#endif