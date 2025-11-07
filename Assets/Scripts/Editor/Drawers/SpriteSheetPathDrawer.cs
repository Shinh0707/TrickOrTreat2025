#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Attributes;
using System.IO;

namespace HalloweenEditor.Drawers
{
    /// <summary>
    /// SpriteSheetPathAttribute が付与された
    /// string型フィールドのGUIを描画する.
    /// </summary>
    [CustomPropertyDrawer(typeof(SpriteSheetPathAttribute))]
    public class SpriteSheetPathDrawer : PropertyDrawer
    {
        // エラーメッセージ用の定数（GCを避けるため static readonly）
        private static readonly GUIContent ErrorLabel =
            new GUIContent("エラー", "この属性は string 型のフィールドにのみ使用できます.");

        /// <summary>
        /// プロパティのGUIを描画する.
        /// </summary>
        /// <param name="position">描画領域を示す矩形</param>
        /// <param name="property">シリアライズされたプロパティ</param>
        /// <param name="label">プロパティのラベル</param>
        public override void OnGUI(Rect position,
                                 SerializedProperty property,
                                 GUIContent label)
        {
            // プロパティがstring型でない場合はエラーを表示して終了する
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label, ErrorLabel);
                return;
            }

            // --- 描画処理 ---
            string currentPath = property.stringValue;
            Sprite currentSprite = null;
            // 現在のパス（文字列）を取得する
            if (!string.IsNullOrEmpty(currentPath))
            {
                // メモリ負荷を考慮し, 必要なアセットのみロードする
                currentSprite = Resources.Load<Sprite>(currentPath);
            }

            EditorGUI.BeginProperty(position, label, property);

            // Sprite用のObjectFieldを描画する
            // ユーザーはスプライトシート（Texture2D）または
            // 個別のSpriteをドラッグ＆ドロップできる.
            Sprite newSprite = EditorGUI.ObjectField(
                position,
                label,
                currentSprite,
                typeof(Sprite),
                false) as Sprite; // allowSceneObjects を false にする

            // ObjectFieldの値が変更されたかチェックする
            if (newSprite != currentSprite)
            {
                if (!newSprite)
                {
                    property.stringValue = "";
                }
                else
                {
                    string fullPath = AssetDatabase.GetAssetPath(newSprite);
                    if (TryGetSpriteSheetPath(fullPath, out currentPath))
                    {
                        Debug.Log($"Full: {fullPath} / Resource: {currentPath}");
                        // プロパティの文字列値を更新する
                        property.stringValue = currentPath;
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        bool TryGetSpriteSheetPath(string path, out string spriteSheetPath)
        {
            var importer = AssetImporter.GetAtPath(path);
            // インポートした物がResources以下のパック済Spriteか判別
            var pos = path.IndexOf("Resources");
            TextureImporter texImporter = importer as TextureImporter;
            if (texImporter == null ||
               texImporter.spriteImportMode != SpriteImportMode.Multiple ||
               pos == -1)
            {
                spriteSheetPath = "";
                return false;
            }
            string resourcePath = path.Substring(pos + 10);
            // Sprite名の取得
            string fileName = Path.GetFileNameWithoutExtension(resourcePath);
            string filePath = Path.GetDirectoryName(resourcePath);
            spriteSheetPath = (string.IsNullOrEmpty(filePath) ? "" : filePath + "/") + fileName;
            return true;
        }
    }
}
#endif