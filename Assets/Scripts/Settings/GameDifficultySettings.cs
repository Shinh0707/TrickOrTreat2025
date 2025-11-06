using UnityEngine;

namespace Halloween.Settings
{
    /// <summary>
    /// ゲームの難易度設定を保持するScriptableObject
    /// </summary>
    [CreateAssetMenu(
        fileName = "GameDifficultySettings",
        menuName = "Settings/Game Difficulty Settings")]
    public class GameDifficultySettings : ScriptableObject
    {
        public AudioClip bgm;
        /// <summary>
        /// フェーズごとのエイリアンの数
        /// </summary>
        public int phaseAlians = 4;

        /// <summary>
        /// フェーズごとの速度（1.0〜3.0）
        /// </summary>
        [Range(1.0f, 3.0f)]
        public float phaseSpeed = 1.0f;

        /// <summary>
        /// Treatの出現率（0.0〜1.0）
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float treatRate;

        /// <summary>
        /// Trickの出現率（0.0〜1.0）
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float trickRate;
    }
}