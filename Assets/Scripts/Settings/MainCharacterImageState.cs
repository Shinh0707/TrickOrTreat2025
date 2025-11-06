using UnityEngine;
using UnityEngine.UI;

namespace Halloween.Settings
{
    [CreateAssetMenu(
        fileName = "MainCharacterImageState",
        menuName = "Settings/MainCharacterImageState")]
    public class MainCharacterImageState : ScriptableObject
    {
        [SerializeField] Sprite _normalImage;
        [SerializeField] Sprite _successImage;
        [SerializeField] Sprite _shockImage;
        [SerializeField] Sprite _worryImage;
        [SerializeField] Sprite _reliefImage;
        
        public void SetStateImage(Types.CharacterState state, Image target)
        {
            //Debug.Log($"Set State: {state}");
            switch (state)
            {
                case Types.CharacterState.NORMAL:
                    target.sprite = _normalImage;
                    break;
                case Types.CharacterState.SUCCESS:
                    target.sprite = _successImage;
                    break;
                case Types.CharacterState.SHOCK:
                    target.sprite = _shockImage;
                    break;
                case Types.CharacterState.WORRY:
                    target.sprite = _worryImage;
                    break;
                case Types.CharacterState.RELIEF:
                    target.sprite = _reliefImage;
                    break;
                default:
                    target.sprite = _normalImage;
                    break;
            }
        }
    }
}
