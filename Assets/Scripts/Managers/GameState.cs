using Unity.VisualScripting;
using UnityEngine;

namespace Halloween
{
    public class GameState
    {
        GameResult _gameResult;
        Types.State _state;
        bool _beatTriggered;
        float _beatDuration;
        float _speed;
        int _beat;
        float _preDelayTime;
        float _beatTime;
        int _currentDifficultyIndex;
        int _localFinishedAlians;
        int _currentLife;
        int _combo;
        public bool Gameover => ((_state == Types.State.GAME) && (_currentLife <= 0)) || (_state == Types.State.GAMEOVER);
        public int Combo
        {
            get { return _combo; }
        }
        public int Life
        {
            get { return _currentLife; }
        }
        public int Beat
        {
            get { return _beat; }
        }
        public bool BeatTriggered
        {
            get { return _beatTriggered; }
        }
        public float BeatDuration
        {
            get { return _beatDuration; }
        }
        public float Speed
        {
            get { return _speed; }
            set
            {
                float v = Mathf.Max(value, 0f);
                if (_speed != v)
                {
                    _speed = v;
                    if (v != 0f)
                    {
                        _beatDuration = 60f / (_setting.bpm * v);
                    }
                }
            }
        }
        public Types.State State
        {
            get { return Gameover ? Types.State.GAMEOVER : _state; }
        }

        public GameResult Result
        {
            get { return _gameResult; }
        }
        Settings.GameSettings _setting;
        public bool FinishedAll => _currentDifficultyIndex >= _setting.gameDifficulties.Length;
        public Settings.GameDifficultySettings CurrentDifficulty => FinishedAll ? _setting.endlessDifficulties :_setting.gameDifficulties[_currentDifficultyIndex];
        public GameState(Settings.GameSettings setting)
        {
            _gameResult = new();
            _state = Types.State.READY;
            _setting = setting;
            _currentDifficultyIndex = 0;
            _localFinishedAlians = 0;
            _currentLife = setting.life;
            _combo = 0;
            _preDelayTime = 0f;
            _beatTime = 0f;
            _beat = 0;
            _speed = 1.0f;
            _beatDuration = 60f / _setting.bpm;
        }
        public bool AlianFinished(Types.ResultType resultType)
        {
            if (Gameover) return false;
            switch (resultType)
            {
                case Types.ResultType.SUCCESS:
                    _gameResult.successCount++;
                    break;
                case Types.ResultType.SAFE:
                    _gameResult.safeCount++;
                    break;
                case Types.ResultType.LATE:
                    _gameResult.lateCount++;
                    break;
                case Types.ResultType.FAIL:
                    _gameResult.failureCount++;
                    break;
                case Types.ResultType.DEATH:
                    _gameResult.treatedDeath = true;
                    break;
            }
            _localFinishedAlians++;
            if ((resultType == Types.ResultType.SAFE) || (resultType == Types.ResultType.SUCCESS))
            {
                _combo++;
                if (_gameResult.maxCombo < _combo)
                {
                    _gameResult.maxCombo = _combo;
                }
                if (_combo % _setting.restoreLifeCombo == 0 && _currentLife < _setting.life)
                {
                    ReduceLife(-1);
                }
            }
            else
            {
                _combo = 0;
                ReduceLife(resultType == Types.ResultType.DEATH ? _currentLife : 1);
            }
            if (CurrentDifficulty.phaseAlians <= _localFinishedAlians)
            {
                if (_currentDifficultyIndex < _setting.gameDifficulties.Length - 1)
                {
                    _currentDifficultyIndex++;
                }
                _localFinishedAlians = 0;
                return true;
            }
            return false;
        }

        void ReduceLife(int value)
        {
            _currentLife -= value;
        }

        public void ResetBeat()
        {
            _beatTime = 0f;
            _beat = 0;
            _beatTriggered = true;
        }

        public void Update(float deltaTime)
        {
            _beatTriggered = false;
            if (_speed <= 0f) return;
            _beatTime += deltaTime;
            if (_state == Types.State.READY)
            {
                _preDelayTime += deltaTime;
                if (_preDelayTime >= _setting.preDelay)
                {
                    _state = Types.State.GAME;
                    ResetBeat();
                }
            }
            if (_beatTime >= _beatDuration)
            {
                int addedBeats = (int)(_beatTime / _beatDuration);
                _beat = (_beat + addedBeats) % _setting.beats;
                _beatTime -= _beatDuration * addedBeats;
                _beatTriggered = true;
            }
        }
    }
}
