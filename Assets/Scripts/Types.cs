namespace Halloween
{
    public static class Types
    {
        public enum ResultType
        {
            LATE = 0,
            SAFE = 1,
            SUCCESS = 2,
            FAIL = 3,
            DEATH = 4
        }
        public enum AlianType
        {
            KID, MONSTER, NONE
        }
        public enum State
        {
            READY, GAME, GAMEOVER
        }
        public enum CharacterState
        {
            NORMAL, SUCCESS, SHOCK, WORRY, RELIEF, NONE
        }
        public enum GameSceneState
        {
            HOME, GAME
        }

    }
}
