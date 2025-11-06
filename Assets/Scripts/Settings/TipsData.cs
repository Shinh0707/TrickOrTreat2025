using Halloween.Managers;
using Halloween.Utility.Property;
using UnityEngine;

namespace Halloween.Settings
{
    [CreateAssetMenu(
    fileName = "TipsData",
    menuName = "Settings/Tips Data")]
    public class TipsData : ScriptableObject
    {
        [SerializeField] private RichTextProperty[] _tips;
        private string[] _formattedTips = new string[0];
        private string Select(int index)
        {
            Debug.Log(_formattedTips.Length);
            if (_formattedTips.Length != _tips.Length)
            {
                _formattedTips = new string[_tips.Length];
                for (int i = 0; i < _tips.Length; i++)
                {
                    Debug.Log($"Format {_tips[i].text}");
                    _formattedTips[i] = Format(_tips[i].text);
                }
            }
            return _formattedTips[index];
        }

        public string RandomSelect() => Select(Random.Range(0, _tips.Length));

        private string Format(string str)
        {
            string res = str;
            res = ReplaceTag(res, "LIFE", GameSceneManager.GameSettings.life);
            res = ReplaceTag(res, "RELIFE", GameSceneManager.GameSettings.restoreLifeCombo);
            res = ReplaceTag(res, "BEATS", GameSceneManager.GameSettings.beats);
            res = ReplaceTag(res, "BEATSTART", GameSceneManager.GameSettings.allowableBeatStartIndex + 1);
            res = ReplaceTag(res, "BEATEND", GameSceneManager.GameSettings.allowableBeatEndIndex + 1);
            res = ReplaceTag(res, "BPM", GameSceneManager.GameSettings.bpm * 2);
            res = ReplaceTag(res, "STAGES", GameSceneManager.GameSettings.gameDifficulties.Length + 1);
            return res;
        }

        private string ReplaceTag(string target, string tag, object replace)
        {
            Debug.Log($"Replace {{{tag}}} to {replace} : {target}");
            if (target.Contains($"{{{tag}}}"))
            {
                return target.Replace($"{{{tag}}}", replace.ToString());
            }
            return target;
        }
    }
}