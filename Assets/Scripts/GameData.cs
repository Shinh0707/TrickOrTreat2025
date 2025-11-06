using System;
using System.Collections.Generic;

namespace Halloween
{

    [Serializable]
    public class GameData
    {
        public List<GameResult> results;
        public bool TryGetLastResult(out GameResult result)
        {
            if (results != null && results.Count > 0)
            {
                result = results[^1];
                return true;
            }
            result = new GameResult();
            return false;
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
    }
}