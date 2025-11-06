using UnityEngine;
using TMPro;

namespace Halloween.Managers
{
    /// <summary>
    /// 吹き出しの表示テキストを管理する
    /// </summary>
    public class FukidashiManager : BeatAnimationManager
    {
        [SerializeField]
        private TextMeshProUGUI _first;
        [SerializeField]
        private TextMeshProUGUI _third;

        /// <summary>
        /// "Trick" (0), "Treat" (1), "Death" (2)
        /// </summary>
        public static readonly string[] labels = { "Trick", "Treat", "Death" };

        protected override void Start()
        {
            base.Start();
            _first.text = "";
            _third.text = "";
        }

        /// <summary>
        /// 難易度設定に基づき, 吹き出しのテキストを設定する
        /// </summary>
        /// <param name="gameDifficulty">難易度設定</param>
        public void SetUp(Settings.GameDifficultySettings gameDifficulty)
        {
            _first.text = GetRandomLabel(gameDifficulty);
            _third.text = GetRandomLabel(gameDifficulty);
        }

        /// <summary>
        /// 判定結果を取得する
        /// </summary>
        /// <param name="trigger">トリガーが引かれたか</param>
        /// <returns>判定結果</returns>
        public Types.ResultType GetResult(bool trigger)
        {
            if (CheckAny(labels[1]))
            {
                if (trigger) return Types.ResultType.SUCCESS;
                if (CheckAny(labels[0])) return Types.ResultType.FAIL;
                if (CheckAny(labels[2])) return Types.ResultType.DEATH;
                return Types.ResultType.LATE;
            }
            if (trigger)
            {
                if (CheckAny(labels[2])) return Types.ResultType.DEATH;
                return Types.ResultType.FAIL;
            }
            return Types.ResultType.SAFE;
        }
        
        /// <summary>
        /// 難易度設定に基づき, ランダムなラベルを返す
        /// </summary>
        /// <param name="gameDifficulty">難易度設定</param>
        /// <returns>選択されたラベル文字列</returns>
        private string GetRandomLabel(Settings.GameDifficultySettings gameDifficulty)
        {
            // 0.0f から 1.0f の間の乱数を生成する
            float randomValue = Random.Range(0f, 1.0f);

            // treatRate の範囲内か判定 (Treat)
            if (randomValue < gameDifficulty.treatRate)
            {
                return labels[1]; // "Treat"
            }

            // trickRate の範囲内か判定 (Trick)
            // treatRate の確率を加算して閾値とする
            float trickThreshold = gameDifficulty.treatRate + gameDifficulty.trickRate;
            
            if (randomValue < trickThreshold)
            {
                return labels[0]; // "Trick"
            }

            // 残りの範囲は deathRate (Death)
            // (1.0f - treatRate - trickRate) の確率に該当
            return labels[2]; // "Death"
        }

        /// <summary>
        /// いずれかの吹き出しに指定のラベルが含まれるか確認する
        /// </summary>
        /// <param name="label">確認するラベル</param>
        /// <returns>含まれる場合は true</returns>
        public bool CheckAny(string label)
        {
            return _first.text == label || _third.text == label;
        }
    }
}