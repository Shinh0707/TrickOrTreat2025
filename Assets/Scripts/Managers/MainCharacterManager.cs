using UnityEngine.UI;
using UnityEngine;
using Halloween.Utility.Property;
using System.Collections.Generic;
using TMPro;
using System.Collections;


namespace Halloween.Managers
{
    public class MainCharacterManager : BeatAnimationManager
    {
        [SerializeField] AnimationSerifsSetProperty _success;
        [SerializeField] AnimationSerifsSetProperty _late;
        [SerializeField] AnimationSerifsSetProperty _fail;
        [SerializeField] AnimationSerifsSetProperty _death;
        [SerializeField] AnimationSerifsSetProperty _safe;
        [SerializeField] Image _characterImage;
        [SerializeField] GameObject _serifContainer;
        [SerializeField] TextMeshProUGUI _serifTextMesh;
        [SerializeField] Settings.MainCharacterImageState _characterImageSetting;
        [SerializeField] AudioSource _voice;
        private Dictionary<Types.ResultType, AnimationSerifsSetProperty> _animMap = new();
        public void SetState(Types.CharacterState state)
        {
            //Debug.Log($"Set State: {state}");
            _characterImageSetting.SetStateImage(state, _characterImage);
            _characterImageSetting.PlayStateVoice(state, _voice);
        }
        protected override void Start()
        {
            base.Start();
            _animMap[Types.ResultType.SUCCESS] = _success;
            _animMap[Types.ResultType.LATE] = _late;
            _animMap[Types.ResultType.FAIL] = _fail;
            _animMap[Types.ResultType.DEATH] = _death;
            _animMap[Types.ResultType.SAFE] = _safe;
            foreach (var item in _animMap)
            {
                AddClip(item.Value.animation, $"3{(int)item.Key}");
            }
        }

        public void Play(Types.ResultType result)
        {
            PlayClip($"3{(int)result}");
            StartCoroutine(ShowSerif(targetDuration, _animMap[result].serifs));
        }
        public void PlayReady()
        {
            Debug.Log($"Ready");
            _characterImageSetting.PlayReadyVoice(_voice);
        }
        private IEnumerator ShowSerif(float duration, string[] serifs)
        {
            _serifContainer.SetActive(true);
            _serifTextMesh.text = serifs[Random.Range(0, serifs.Length)];
            yield return new WaitForSeconds(duration);
            _serifContainer.SetActive(false);
        }
    }
}
