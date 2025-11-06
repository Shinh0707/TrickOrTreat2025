using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using Halloween.Settings;
using UnityEngine.InputSystem;

namespace Halloween.Managers
{
    /// <summary>
    /// シーンを跨いで保持したい変数などを持つ
    /// </summary>
    public class GameSceneManager : Utility.SingletonMonoBehaviour<GameSceneManager>
    {
        [SerializeField] private GameSettings _globalGameSettings;
        public static GameSettings GameSettings => Instance._globalGameSettings;
        [SerializeField] private GameData _gameData = new();
        public static GameData GameData => Instance._gameData;
        public static void AddResult(GameResult result)
        {
            GameData.results.Add(result);
        }
        public static bool TryGetLastResult(out GameResult result)
        {
            return GameData.TryGetLastResult(out result);
        }
        private static InputAction CheckAndGetAction(InputActionProperty inputActionProperty)
        {
            if (!inputActionProperty.reference)
            {
                if (!inputActionProperty.action.enabled)
                {
                    inputActionProperty.action.Enable();
                }
                return inputActionProperty.action;
            }
            if (!inputActionProperty.reference.action.enabled)
            {
                inputActionProperty.reference.action.Enable();
            }
            return inputActionProperty.reference.action;
        }
        private static bool Vector2Inside(Vector2 target, float width, float height) => (target.x >= 0f) && (target.x <= width) && (target.y >= 0f) && (target.y <= height);
        public static bool InputTriggered()
        {
            if (CheckAndGetAction(GameSettings.inputButtonAction).IsPressed()) return true;
            if (!CheckAndGetAction(GameSettings.inputPointerAction).IsPressed()) return false;
            return Vector2Inside(
                CheckAndGetAction(GameSettings.inputPointerActionPosition).ReadValue<Vector2>(),
                Screen.width, Screen.height
            );
        }
        public static bool PlayedGame => GameData.results.Count > 0;
        [SerializeField] private Types.GameSceneState _currentState = Types.GameSceneState.HOME;
        public static Types.GameSceneState CurrentState => Instance._currentState;
        private UnityEvent<Types.GameSceneState> _onSceneChanged = new();
        public static UnityEvent<Types.GameSceneState> onSceneChanged => Instance._onSceneChanged;
        private void _OnSceneChanged(Types.GameSceneState state)
        {
            _onSceneChanged?.Invoke(state);
        }
        public static void SceneChanged(Types.GameSceneState state)
        {
            Instance._currentState = state;
            Instance._OnSceneChanged(state);
        }
        private IEnumerator _UnloadSceneAsync(Utility.Property.SceneProperty scene, IEnumerator onUnloaded = null)
        {
            var unload = scene.UnloadSceneAsync();
            yield return unload;
            yield return null;
            yield return onUnloaded;
        }
        private void _UnloadScene(Utility.Property.SceneProperty scene, IEnumerator onUnloaded = null)
        {
            StartCoroutine(_UnloadSceneAsync(scene, onUnloaded));
        }
        public static void UnloadScene(Utility.Property.SceneProperty scene, IEnumerator onUnloaded = null)
        {
            Instance._UnloadScene(scene, onUnloaded);
        }

        public static IEnumerator UnloadSceneAsync(Utility.Property.SceneProperty scene, IEnumerator onUnloaded = null)
        {
            yield return Instance._UnloadSceneAsync(scene, onUnloaded);
        }
    }
}