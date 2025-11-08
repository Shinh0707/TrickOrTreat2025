using Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Halloween.Managers
{
    public class AlianManager : BeatAnimationManager
    {
        public enum DoorState
        {
            OPEN, CLOSE
        }
        [SerializeField] AnimationClip _appearAlianAnimation;
        [SerializeField] AnimationClip _deathAlianAnimation;
        [SerializeField] AnimationClip _noAlianAnimation;
        [SerializeField] Image _doorImage;
        [SerializeField] Sprite _imageOpen;
        [SerializeField] AudioClip _openSound;
        [SerializeField] Sprite _imageClose;
        [SerializeField] AudioClip _closeSound;
        [SerializeField] AudioSource _sound;
        [SerializeField] Image _alianImage;
        [SerializeField,SpriteSheetPath]
        private string _kidsSpriteSheet;
        [SerializeField,SpriteSheetPath]
        private string _monstersSpriteSheet;
        Sprite[] _kidSprites;
        Sprite[] _monsterSprites;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
            _kidSprites = Resources.LoadAll<Sprite>(_kidsSpriteSheet);
            _monsterSprites = Resources.LoadAll<Sprite>(_monstersSpriteSheet);
            AddClip(_appearAlianAnimation, $"30");
            AddClip(_deathAlianAnimation, $"31");
            AddClip(_noAlianAnimation, $"32");
        }

        public void SetDoorState(DoorState state)
        {
            _doorImage.sprite = (state == DoorState.OPEN) ? _imageOpen : _imageClose;
            _sound.PlayOneShot((state == DoorState.OPEN) ? _openSound : _closeSound);
        }

        public void SetSprite(Types.AlianType alianType)
        {
            switch (alianType)
            {
                case Types.AlianType.KID:
                    _alianImage.sprite = _kidSprites[Random.Range(0, _kidSprites.Length)];
                    break;
                case Types.AlianType.MONSTER:
                    _alianImage.sprite = _monsterSprites[Random.Range(0, _monsterSprites.Length)];
                    break;
                default:
                    _alianImage.sprite = null;
                    break;
            }
        }

        public void Play(Types.TreatResult result)
        {
            switch (result.resultType)
            {
                case Types.ResultType.SUCCESS:
                    SetSprite(result.hasDeath ? Types.AlianType.MONSTER : Types.AlianType.KID);
                    PlayClip($"30");
                    break;
                case Types.ResultType.FAIL:
                    SetSprite(Types.AlianType.MONSTER);
                    PlayClip($"30");
                    break;
                case Types.ResultType.DEATH:
                    SetSprite(Types.AlianType.NONE);
                    PlayClip($"31");
                    break;
                default:
                    SetSprite(Types.AlianType.NONE);
                    PlayClip($"32");
                    break;
            }
        }
    }
}