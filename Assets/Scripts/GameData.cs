using System;
using System.Collections.Generic;

namespace Halloween
{
    [Flags]
    public enum GameDataEvaluate : int
    {
        NONE = 0,
        MAXCOMBO = 1,
        MAXSUCCESS = 2
    }

    [Serializable]
    public class GameData
    {
        private int _bestMaxComboIndex = -1;
        private int _bestSuccessIndex = -1;
        private int _bestUnmissesIndex = -1;
        private List<GameResult> _results = new();
        public bool HasResult => _results.Count > 0;
        public void AddResult(GameResult result)
        {
            if (_bestMaxComboIndex == -1)
            {
                _bestMaxComboIndex = 0;
            }
            else if (_results[_bestMaxComboIndex].maxCombo < result.maxCombo)
            {
                _bestMaxComboIndex = _results.Count;
            }
            if (_bestSuccessIndex == -1)
            {
                _bestSuccessIndex = 0;
            }
            else if (_results[_bestSuccessIndex].successCount < result.successCount)
            {
                _bestSuccessIndex = _results.Count;
            }
            if (_bestUnmissesIndex == -1)
            {
                _bestUnmissesIndex = 0;
            }
            else if (_results[_bestUnmissesIndex].totalUnmisses < result.totalUnmisses)
            {
                _bestUnmissesIndex = _results.Count;
            }
            _results.Add(result);
        }

        public bool TryGetLastResult(out GameResult result)
        {
            if (_results != null && _results.Count > 0)
            {
                result = _results[^1];
                return true;
            }
            result = new GameResult();
            return false;
        }

        public int GetBestMaxCombo()
        {
            if (_bestMaxComboIndex >= 0)
            {
                return _results[_bestMaxComboIndex].maxCombo;
            }
            return 0;
        }

        public int GetBestSuccess()
        {
            if (_bestSuccessIndex >= 0)
            {
                return _results[_bestSuccessIndex].successCount;
            }
            return 0;
        }

        public int GetBestUnMisses()
        {
            if (_bestUnmissesIndex >= 0)
            {
                return _results[_bestUnmissesIndex].totalUnmisses;
            }
            return 0;
        }

        public GameDataEvaluate LastEvaluate()
        {
            if (_results != null && _results.Count > 1)
            {
                return (
                    (((_results.Count - 1) == _bestMaxComboIndex) ? GameDataEvaluate.MAXCOMBO : GameDataEvaluate.NONE) |
                    (((_results.Count - 1) == _bestUnmissesIndex) ? GameDataEvaluate.MAXSUCCESS : GameDataEvaluate.NONE)
                );
            }
            return GameDataEvaluate.NONE;
        }
    }
    
    [Serializable]
    public struct GameResult
    {
        public int maxCombo;
        public int successCount;
        public int failureCount;
        public int lateCount;
        public int safeCount;
        public bool treatedDeath;
        public int totalAlians => successCount + failureCount + lateCount + safeCount;
        public int totalMisses => failureCount + lateCount;
        public int totalUnmisses => successCount + safeCount;
    }
}