using System;

namespace Emberfall.Core
{

    public static class GameEvents
    {
        public static event Action LevelCompleted;
        public static event Action PlayerDied;
        public static event Action<int, int> HeartsChanged;
        public static event Action<int> ScoreChanged;

        public static void RaiseLevelCompleted() => LevelCompleted?.Invoke();
        public static void RaisePlayerDied() => PlayerDied?.Invoke();
        public static void RaiseHeartsChanged(int current, int max) => HeartsChanged?.Invoke(current, max);
        public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
    }
}
