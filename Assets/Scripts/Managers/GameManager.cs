using System.Collections;
using UnityEngine;

namespace Halloween.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] GameObject _audioObject;
        [SerializeField] MainCharacterManager _characterManager;
        [SerializeField] FukidashiManager _fukidashiManager;
        [SerializeField] AlianManager _alianManager;
        [SerializeField] ComboManager _comboManager;
        [SerializeField] LifeManager _lifeManager;
        [SerializeField] AnnotationManager _annotationManager;
        [SerializeField] GameAudioManager _gameAudioManager;
        [SerializeField] AudioClip _endAudio;
        Settings.GameSettings _setting;
        bool _gameoverAnimationIsRunning;
        GameState _gameState;
        bool _trigger; // Treatフラグ
        bool _reset;

        void Start()
        {
            _setting = GameSceneManager.GameSettings;
            _gameState = new GameState(_setting);
            _trigger = false;
            _reset = false;
            _gameoverAnimationIsRunning = false;
            _fukidashiManager.SetUp(_gameState.CurrentDifficulty);
            if (_gameState.Speed != 1.0f)
            {
                _gameState.Speed = 1.0f;
                _characterManager.SetDuration(_gameState.BeatDuration);
                _fukidashiManager.SetDuration(_gameState.BeatDuration);
                _alianManager.SetDuration(_gameState.BeatDuration);
            }
            _lifeManager.SetLife(_gameState.Life);
            _gameState.ResetBeat();
        }

        void Restart(float speedMultiplier = 1.0f, bool restartBGM = false)
        {
            //Debug.Log($"Speed: {speedMultiplier}");
            _trigger = false;
            _reset = false;
            _gameAudioManager.SetPitch(speedMultiplier);
            _fukidashiManager.SetUp(_gameState.CurrentDifficulty);
            if (_gameState.Speed != speedMultiplier)
            {
                //Debug.Log($"Set Speed => {_gameState.Speed} => {speedMultiplier}");
                _gameState.Speed = speedMultiplier;
                _characterManager.SetDuration(_gameState.BeatDuration);
                _fukidashiManager.SetDuration(_gameState.BeatDuration);
                _alianManager.SetDuration(_gameState.BeatDuration);
            }
            if (restartBGM || !_gameAudioManager.isPlaying())
            {
                _gameAudioManager.StartCrossfadeAudio(_gameState.CurrentDifficulty.bgm, _gameState.BeatDuration);
            }
            _gameState.ResetBeat();
        }

        void PlayBeatGameAnimations(int index)
        {
            //Debug.Log($"Beat: {index} / Reset: {_reset}");
            if (index == 0)
            {
                if (_reset) _annotationManager.Play();
                Restart(_gameState.CurrentDifficulty.phaseSpeed, _reset);
            }
            _comboManager.SetCombo(_gameState.Combo);
            _lifeManager.SetLife(_gameState.Life);
            _fukidashiManager.Play(index);
            if (index == _setting.beats - 1)
            {
                Types.ResultType result = _fukidashiManager.GetResult(_trigger);
                _reset = _gameState.AlianFinished(result);
                _characterManager.Play(result);
                _alianManager.Play(result, _fukidashiManager.CheckAny(FukidashiManager.labels[2]));
            }
            else
            {
                _characterManager.Play(index);
                _alianManager.Play(index);
            }
        }

        void PlayBeatReadyAnimations()
        {
            _fukidashiManager.Play(-1);
            _characterManager.Play(-1);
            _alianManager.Play(-1);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (GameSceneManager.CurrentState != Types.GameSceneState.GAME) return;
            _gameState.Update(Time.fixedDeltaTime);
            if (_gameState.State == Types.State.GAME)
            {
                if ((_setting.allowableBeatStartIndex <= _gameState.Beat) && (_gameState.Beat <= _setting.allowableBeatEndIndex) && !_trigger)
                {
                    _trigger = GameSceneManager.InputTriggered();
                }
                if (_gameState.BeatTriggered) PlayBeatGameAnimations(_gameState.Beat);
            }
            else
            {
                _comboManager.SetCombo(0);
                _lifeManager.SetLife(0);
                if (_gameState.BeatTriggered) PlayBeatReadyAnimations();
                if ((_gameState.State == Types.State.GAMEOVER) && !_gameoverAnimationIsRunning)
                {
                    _gameoverAnimationIsRunning = true;
                    GameSceneManager.AddResult(_gameState.Result);
                    GameSceneManager.SceneChanged(Types.GameSceneState.HOME);
                    _gameAudioManager.SetLoop(false);
                    _gameAudioManager.SetPitch(1.0f);
                    _gameAudioManager.StartCrossfadeAudio(_endAudio, _endAudio.length * 0.25f, () =>
                    {
                        _gameAudioManager.StopAll();
                    });
                }
            }
        }
    }
}