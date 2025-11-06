using UnityEngine;
using UnityEngine.InputSystem;

namespace Halloween.Settings
{
    [CreateAssetMenu(
    fileName = "GameSettings",
    menuName = "Settings/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        public InputActionProperty inputButtonAction;
        public InputActionProperty inputPointerAction;
        public InputActionProperty inputPointerActionPosition;
        public KeyCode mainKey = KeyCode.Space;
        public float preDelay = 1f;
        public int bpm = 120;
        public int beats = 4;
        public int allowableBeatStartIndex = 2;
        public int allowableBeatEndIndex = 3;
        public int life = 5;
        public int restoreLifeCombo = 5;
        public GameDifficultySettings[] gameDifficulties;
        public GameDifficultySettings endlessDifficulties;
    }
}