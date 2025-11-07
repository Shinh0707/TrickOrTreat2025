#if UNITY_EDITOR
using UnityEngine;
using UnityEditor; // Editor機能（MenuItem, AssetDatabase）に必要
using System.IO; // ファイル操作（Directory, File）に必要
using System.Text; // StringBuilderに必要

namespace HalloweenEditor.EditorScripts
{
    /// <summary>
    /// プロジェクト内のLICENCEファイルを統合する機能を提供する.
    /// </summary>
    [ExecuteInEditMode]
    public class LICENCEComposer : MonoBehaviour
    {
        // 検索するファイル名
        private const string SearchPattern = "LICENCE";
        // 出力するファイル名
        private const string OutputFileName = "Data/LICENCE/LICENCE";
        // 区切り文字
        private const string Separator =
            "\n----------------------------------------\n";

        /// <summary>
        /// Assets/ 以下の全 "LICENCE" ファイルを走査し,
        /// 1つの "LICENCE" ファイルにまとめる.
        /// (メニュー: Assets/Compose LICENCE File)
        /// </summary>
        [MenuItem("Assets/Compose LICENCE File")]
        public static void ComposeLicenceFile()
        {
            // パフォーマンスのため, 文字列結合にはStringBuilderを使用する
            var builder = new StringBuilder();
            builder.AppendLine(
                "# All LICENCE"
            );
            builder.AppendLine();

            // Assets/ フォルダの絶対パスを取得する
            string assetsPath = Application.dataPath;

            // Assets/ 以下の全 "LICENCE" ファイルを検索する
            string[] licencePaths = Directory.GetFiles(
                assetsPath,
                SearchPattern,
                SearchOption.AllDirectories
            );

            // 処理速度のため, forループを使用する
            for (int i = 0; i < licencePaths.Length; i++)
            {
                string path = licencePaths[i];

                // 出力ファイル自身はスキップする
                if (Path.GetFullPath(path) == Path.GetFullPath(Path.Combine(assetsPath, OutputFileName)))
                {
                    continue;
                }

                // ファイルパスを "Assets/..." 形式に整形する
                //string relativePath = "Assets" + path.Substring(assetsPath.Length);
                // Windows環境を考慮し, パス区切り文字を統一する
                //relativePath = relativePath.Replace("\\", "/");

                // ファイル内容を読み込む
                string content = File.ReadAllText(path);

                //builder.AppendLine(Separator);
                //builder.AppendLine($"# Source: {relativePath}");
                builder.AppendLine(Separator);
                builder.AppendLine(content);
            }

            // Assets/ 直下に結合したファイルを出力する
            string outputPath = Path.Combine(assetsPath, OutputFileName);
            File.WriteAllText(outputPath, builder.ToString());

            // Unity Editorにファイルの変更を通知する
            AssetDatabase.Refresh();

            Debug.Log($"[LICENCEComposer] Succeeded. {licencePaths.Length} files composed.");
        }
    }
}
#endif