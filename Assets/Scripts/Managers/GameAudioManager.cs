using System.Collections;
using UnityEngine;
using UnityEngine.Events;
namespace Halloween.Managers
{
    public class GameAudioManager : MonoBehaviour
    {
        AudioSource[] _audioSources;
        int _audioSourceIndex;
        Coroutine _audioCrossfadeCoroutine = null;

        void Start()
        {
            _audioSourceIndex = 0;
            _audioSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                _audioSources[i] = gameObject.AddComponent<AudioSource>();
                _audioSources[i].bypassEffects = false;
                _audioSources[i].bypassListenerEffects = false;
                _audioSources[i].bypassReverbZones = false;
                _audioSources[i].playOnAwake = false;
                _audioSources[i].priority = 0;
                _audioSources[i].loop = true;
                _audioSources[i].volume = 1.0f;
                _audioSources[i].pitch = 1.0f;
                _audioSources[i].panStereo = 0.0f;
                _audioSources[i].spatialBlend = 0.0f;
                _audioSources[i].reverbZoneMix = 0.0f;
            }
        }

        public void SetPitch(float pitch)
        {
            foreach (var audioSource in _audioSources)
            {
                audioSource.pitch = pitch;
            }
        }

        public void SetLoop(bool loop)
        {
            foreach (var audioSource in _audioSources)
            {
                audioSource.loop = loop;
            }
        }

        public bool isPlaying()
        {
            foreach (var audioSource in _audioSources)
            {
                if (audioSource.isPlaying) return true;
            }
            return false;
        }
        
        public void StopAll()
        {
            foreach (var audioSource in _audioSources)
            {
                if (audioSource.isPlaying) audioSource.Stop();
            }
        }

        public void StartCrossfadeAudio(AudioClip targetBGM, float duration)
        {
            if (_audioCrossfadeCoroutine != null)
            {
                StopCoroutine(_audioCrossfadeCoroutine);
            }
            _audioCrossfadeCoroutine = StartCoroutine(CrossfadeAudio(targetBGM, duration));
        }

        public void StartCrossfadeAudio(AudioClip targetBGM, float duration, UnityAction onEndPlay)
        {
            if (_audioCrossfadeCoroutine != null)
            {
                StopCoroutine(_audioCrossfadeCoroutine);
            }
            var targetAudioSource = _audioSources[1 - _audioSourceIndex];
            _audioCrossfadeCoroutine = StartCoroutine(CrossfadeAudio(targetBGM, duration, WaitForEndPlayAudio(targetAudioSource, onEndPlay)));
        }

        public IEnumerator WaitForEndPlayAudio(AudioSource audioSource, UnityAction onEndPlay = null)
        {
            if (audioSource.pitch <= 0 || !audioSource.isPlaying) yield break;
            float estimateLength = (audioSource.clip.length - audioSource.time) / audioSource.pitch;
            float t = 0f;
            while (t < estimateLength)
            {
                if (!audioSource.isPlaying) yield break;
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
            }
            onEndPlay?.Invoke();
        }

        IEnumerator CrossfadeAudio(AudioClip targetBGM, float duration, IEnumerator onEndCrossfade = null)
        {
            int index = _audioSourceIndex;
            _audioSourceIndex = 1 - _audioSourceIndex;
            _audioSources[index].volume = 1.0f;
            if (!_audioSources[index].isPlaying) _audioSources[index].Play();
            _audioSources[1 - index].Stop();
            _audioSources[1 - index].clip = targetBGM;
            _audioSources[1 - index].volume = 0f;
            _audioSources[1 - index].Play();
            float t = 0f;
            while (t < duration)
            {
                _audioSources[1 - index].volume = Mathf.Min(1.0f, t / duration);
                _audioSources[index].volume = 1.0f - Mathf.Min(1.0f, t / duration);
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
            }
            _audioSources[1 - index].volume = 1.0f;
            _audioSources[index].volume = 0.0f;
            _audioSources[index].Stop();
            yield return onEndCrossfade;
        }
    }
}