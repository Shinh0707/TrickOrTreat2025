using UnityEngine;

namespace Halloween.Utility
{
    /// <summary>
    /// MonoBehaviour を継承したシングルトンパターンの
    /// 抽象基底クラス.
    /// </summary>
    /// <remarks>
    /// これを継承するクラス T は, シーン内に T 型の
    /// コンポーネントが1つだけ存在することを保証しようとする.
    /// </remarks>
    /// <typeparam name="T">シングルトンにするクラス型</typeparam>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        // 唯一のインスタンスを保持する.
        // static 変数（メモリは確保されるが, 速度優先のため許容する）
        private static T _instance;

        /// <summary>
        /// シングルトンインスタンスにアクセスする.
        /// </summary>
        /// <remarks>
        /// このアクセスは高速（フィールド参照）だが,
        /// Awake() 以前にアクセスすると null が返る可能性がある.
        /// </remarks>
        public static T Instance
        {
            get
            {
                // 規約に基づき, 速度を優先し FindObjectOfType のような
                // 重い検索処理はここでは行わない.
                if (_instance == null)
                {
                    // 開発者がアクセス順序の誤りに気づけるよう警告する
                    Debug.LogWarning(typeof(T) +
                        " のインスタンスに Awake() 前か " +
                        "破棄後にアクセスしようとしました.");
                }
                return _instance;
            }
        }

        [Header("Singleton Settings")]
        [SerializeField]
        [Tooltip("true の場合, シーン遷移時にこのオブジェクトを破棄しない")]
        private bool _dontDestroyOnLoad = false;

        /// <summary>
        /// インスタンスの初期化と重複インスタンスの破棄を処理する.
        /// </summary>
        protected virtual void Awake()
        {
            // インスタンスがまだ存在しない場合
            if (_instance == null)
            {
                // this (自分自身) を T 型として static な _instance に格納する
                _instance = this as T;

                if (_dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
            }
            // インスタンスが既に存在し, それが自分でない場合
            else if (_instance != this)
            {
                // 重複して存在することを防ぐため,
                // 自分（後から作られた方）を破棄する
                Debug.LogWarning(typeof(T) + 
                    " のインスタンスが既に存在するため, " + 
                    this.gameObject.name + " を破棄しました.");
                
                // gameObject ではなく, このコンポーネント自体を破棄する
                // 方が, 他のコンポーネントへの影響が少ない場合がある.
                // ただし, 一般的には gameObject ごと破棄する.
                Destroy(this.gameObject);
            }
        }

        /// <summary>
        /// アプリケーション終了時またはオブジェクト破棄時に
        /// static インスタンス参照をクリアする.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 破棄されるのが自分自身（＝現在保持しているインスタンス）の場合
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}