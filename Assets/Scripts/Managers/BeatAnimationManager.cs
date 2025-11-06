using System.Collections.Generic;
using UnityEngine;

namespace Halloween.Managers
{
    public class BeatAnimationManager : MonoBehaviour
    {
        [SerializeField] protected Animation _anim;
        [SerializeField] AnimationClip _defaultAnimation;
        [SerializeField] AnimationClip[] _beatAnimations;
        private Dictionary<string, AnimationClip> _clips = new();
        private string currentClipName = "";
        private float _targetDuration = 1.0f;
        protected float targetDuration => _targetDuration;
        protected void AddClip(AnimationClip clip, string name)
        {
            clip.legacy = true;
            clip.wrapMode = WrapMode.Once;
            _clips[name] = clip;
            _anim.AddClip(clip, name);
            _anim[name].blendMode = AnimationBlendMode.Blend;
            _anim[name].weight = 1.0f;
            _anim[name].wrapMode = WrapMode.Once;
        }
        protected void PlayClip(string name)
        {
            if (!string.IsNullOrEmpty(currentClipName))
            {
                _clips[currentClipName].SampleAnimation(_anim.gameObject, _clips[currentClipName].length);
            }
            _anim.Rewind(name);
            _anim.clip = _clips[name];
            _anim[name].speed = _clips[name].length / _targetDuration;
            _anim.Play(name, PlayMode.StopAll);
            currentClipName = name;
        }
        protected virtual void Start()
        {
            _anim.animatePhysics = false;
            _anim.playAutomatically = false;
            _anim.wrapMode = WrapMode.Once;
            _anim.updateMode = AnimationUpdateMode.Fixed;
            _anim.clip = _defaultAnimation;
            AddClip(_defaultAnimation, "-1");
            for (int i = 0; i < _beatAnimations.Length; i++)
            {
                AddClip(_beatAnimations[i], $"{i}");
            }
        }
        public void SetDuration(float targetDuration)
        {
            if (targetDuration > 0)
            {
                _targetDuration = targetDuration;
                foreach (AnimationState clip in _anim)
                {
                    clip.speed = clip.length / targetDuration;
                }
            }
        }
        public virtual void Play(int index)
        {
            PlayClip($"{index}");
        }
    }
}