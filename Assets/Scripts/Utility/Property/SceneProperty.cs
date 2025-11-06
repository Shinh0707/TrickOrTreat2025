using UnityEngine;
using UnityEngine.SceneManagement;

namespace Halloween.Utility.Property
{
    /// <summary>
    /// Unityエディタのインスペクタからシーンを割り当て、
    /// ロードモードを指定するためのシリアライズ可能な構造体.
    /// </summary>
    [System.Serializable]
    public struct SceneProperty
    {
        /// <summary>
        /// ロードするシーンのパス（ビルド設定またはアセットパス）
        /// </summary>
        [SerializeField]
        private string _scenePath;

        /// <summary>
        /// シーンのロードモード（SingleまたはAdditive）
        /// </summary>
        [SerializeField]
        private LoadSceneMode _loadMode;

        /// <summary>
        /// 割り当てられたシーンのパスを取得する.
        /// </summary>
        public string ScenePath
        {
            get { return _scenePath; }
        }

        /// <summary>
        /// 割り当てられたロードモードを取得する.
        /// </summary>
        public LoadSceneMode LoadMode
        {
            get { return _loadMode; }
        }

        /// <summary>
        /// 設定されたシーンを指定されたモードで同期的にロードする.
        /// </summary>
        /// <remarks>
        /// シーンがビルド設定に含まれていない場合、
        /// エディタ外ではロードに失敗する可能性がある.
        /// </remarks>
        public void LoadScene()
        {
            if (string.IsNullOrEmpty(_scenePath))
            {
                Debug.LogError("Scene path is not set.");
                return;
            }
            SceneManager.LoadScene(_scenePath, _loadMode);
        }

        /// <summary>
        /// 設定されたシーンを指定されたモードで非同期的にロードする.
        /// </summary>
        /// <returns>
        /// ロード操作の進捗と完了を追跡するための AsyncOperation.
        /// </returns>
        /// <remarks>
        /// シーンがビルド設定に含まれていない場合、
        /// エディタ外ではロードに失敗する可能性がある.
        /// </remarks>
        public AsyncOperation LoadSceneAsync()
        {
            if (string.IsNullOrEmpty(_scenePath))
            {
                Debug.LogError("Scene path is not set.");
                return null;
            }
            return SceneManager.LoadSceneAsync(_scenePath, _loadMode);
        }

        public AsyncOperation UnloadSceneAsync()
        {
            if (string.IsNullOrEmpty(_scenePath))
            {
                Debug.LogError("Scene path is not set.");
                return null;
            }
            return SceneManager.UnloadSceneAsync(_scenePath);
        }
    }
}