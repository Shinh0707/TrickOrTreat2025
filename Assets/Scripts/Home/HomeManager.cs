using System;
using System.Collections;
using UnityEngine;

namespace Halloween.Managers
{
    public class HomeManager : MonoBehaviour
    {
        [SerializeField] GameObject _mainUI;
        [SerializeField] RectTransform[] _panels;
        [SerializeField] float _fallAcc = 300f;
        [SerializeField] float _raiseAcc = 1200f;
        [SerializeField] GameObject _transitionPanel;
        [SerializeField] AnimationClip _transitionAnimation;
        [SerializeField] Utility.Property.SceneProperty _gameScene;
        [SerializeField] HomeCharacterManager _homeCharacterManager;
        [SerializeField] TipsManager _tipsManager;
        [SerializeField] GameObject _tipsObject;
        Settings.GameSettings _setting;
        int _currentPanel;
        bool _isAnimate;
        bool _keepPressed;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            DontDestroyOnLoad(_mainUI);
            DontDestroyOnLoad(this);
            GameSceneManager.onSceneChanged.RemoveListener(OnSceneChanged);
            GameSceneManager.onSceneChanged.AddListener(OnSceneChanged);
            Restart();
        }

        void Restart()
        {
            _setting = GameSceneManager.GameSettings;
            _homeCharacterManager.Start();
            _currentPanel = 0;
            _keepPressed = false;
            _isAnimate = false;
        }

        IEnumerator FallPanel(RectTransform target)
        {
            Debug.Log($"Start Fall");
            _isAnimate = true;
            float speed = 0f;
            Vector2 pos = target.anchoredPosition;
            float h = target.rect.height;
            while (pos.y > -h)
            {
                yield return new WaitForFixedUpdate();
                float dt = Time.fixedDeltaTime;
                speed += _fallAcc * dt;
                pos.Set(pos.x, pos.y - speed * dt);
                target.anchoredPosition = pos;
            }
            target.gameObject.SetActive(false);
            _isAnimate = false;
            Debug.Log($"End Fall");
        }

        IEnumerator RaisePanel(RectTransform target)
        {
            Debug.Log($"Start Raise");
            _isAnimate = true;
            target.gameObject.SetActive(false);
            float speed = 0f;
            float h = target.rect.height;
            Vector2 pos = new(target.anchoredPosition.x, -h);
            target.anchoredPosition = pos;
            target.gameObject.SetActive(true);
            while (pos.y < 0)
            {
                yield return new WaitForFixedUpdate();
                float dt = Time.fixedDeltaTime;
                speed += _raiseAcc * dt;
                pos.Set(pos.x, pos.y + speed * dt);
                target.anchoredPosition = pos;
            }
            target.anchoredPosition = new(target.anchoredPosition.x, 0f);
            _isAnimate = false;
            Debug.Log($"End Raise");
        }
        IEnumerator RaisePanels(RectTransform[] targets, bool reverse)
        {
            Debug.Log($"Start Raise");
            _isAnimate = true;
            int n_targets = targets.Length;
            for (int i = 0; i < n_targets; i++)
            {
                var target = reverse ? targets[n_targets - i - 1] : targets[i]; 
                yield return RaisePanel(target);
            }
            _isAnimate = false;
            Debug.Log($"End Raise");
        }

        IEnumerator PlayTransition(GameObject target, bool reverse, IEnumerator onEndTransition = null)
        {
            Debug.Log("Start Transition");
            _isAnimate = true;
            float _totalLength = _transitionAnimation.length;
            float _currentTime = reverse ? _totalLength : 0f;
            target.SetActive(true);
            _transitionAnimation.SampleAnimation(target, reverse ? _totalLength : 0f);
            while (_currentTime >= 0 && _currentTime <= _totalLength)
            {
                yield return new WaitForFixedUpdate();
                float dt = Time.fixedDeltaTime;
                _currentTime = reverse ? (_currentTime - dt) : (_currentTime + dt);
                _transitionAnimation.SampleAnimation(target, Mathf.Clamp(_currentTime, 0f, _totalLength));
            }
            yield return onEndTransition;
            _isAnimate = false;
            Debug.Log("End Transition");
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_isAnimate || (GameSceneManager.CurrentState != Types.GameSceneState.HOME)) return;
            if (GameSceneManager.InputTriggered())
            {
                Debug.Log($"Pressed");
                if (_keepPressed) return;
                if (_currentPanel < _panels.Length)
                {
                    StartCoroutine(FallPanel(_panels[_currentPanel]));
                    _currentPanel++;
                    if (_currentPanel == _panels.Length)
                    {
                        _tipsObject.SetActive(true);
                        _tipsManager.SetTips();
                    }
                }
                else if (_currentPanel == _panels.Length)
                {
                    _currentPanel = _panels.Length + 1;
                    GameSceneManager.SceneChanged(Types.GameSceneState.GAME);
                    _gameScene.LoadScene();
                    _tipsObject.SetActive(false);
                    StartCoroutine(PlayTransition(_transitionPanel, false));
                }
            }
            else if (_keepPressed)
            {
                _keepPressed = false;
                Debug.Log($"Cancel Keep Pressed");
            }
        }

        void OnSceneChanged(Types.GameSceneState state)
        {
            if (state != Types.GameSceneState.HOME) return;
            if (_currentPanel != _panels.Length + 1) return;
            Restart();
            StartCoroutine(
                PlayTransition(_transitionPanel, true,
                    GameSceneManager.UnloadSceneAsync(_gameScene, RaisePanels(_panels, true))
                )
            );
        }
    }
}