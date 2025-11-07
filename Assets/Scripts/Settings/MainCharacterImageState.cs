using UnityEngine;
using UnityEngine.UI;

namespace Halloween.Settings
{
    [CreateAssetMenu(
        fileName = "MainCharacterImageState",
        menuName = "Settings/MainCharacterImageState")]
    public class MainCharacterImageState : ScriptableObject
    {
        [SerializeField] AudioClip _readyVoice;
        [SerializeField] Sprite _normalImage;
        [SerializeField] Sprite _successImage;
        [SerializeField] AudioClip _successVoice;
        [SerializeField] Sprite _shockImage;
        [SerializeField] AudioClip _shockVoice;
        [SerializeField] Sprite _worryImage;
        [SerializeField] AudioClip _worryVoice;
        [SerializeField] Sprite _reliefImage;
        [SerializeField] AudioClip _reliefVoice;

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

        private void PlayOneShot(AudioClip clip, AudioSource target)
        {
            if (clip)
            {
                target.PlayOneShot(clip);
            }
        }

        public void PlayStateVoice(Types.CharacterState state, AudioSource target)
        {
            switch (state)
            {
                case Types.CharacterState.SUCCESS:
                    PlayOneShot(_successVoice, target);
                    break;
                case Types.CharacterState.SHOCK:
                    PlayOneShot(_shockVoice, target);
                    break;
                case Types.CharacterState.WORRY:
                    PlayOneShot(_worryVoice, target);
                    break;
                case Types.CharacterState.RELIEF:
                    PlayOneShot(_reliefVoice, target);
                    break;
                default:
                    break;
            }
        }
        
        public void PlayReadyVoice(AudioSource target)
        {
            PlayOneShot(_readyVoice, target);
        }
    }
}
