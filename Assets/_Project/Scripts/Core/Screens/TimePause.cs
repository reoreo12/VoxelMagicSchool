// 시간 정지를 원인별로 등록·해제한다(UI_SPEC 4-A-2, 13장 / QUEST_TIMER 1-2). 모든 원인이 풀려야 다시 흐르고, 같은 원인은 두 번 등록해도 하나로 센다.
// 원인마다 단계(전체 정지 = timeScale 0, 게임 시계만)를 기억한다. 실제 적용은 Gameplay가 Changed를 받아 한다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public enum PauseLevel
    {
        None,
        /// <summary>게임 안의 시각만 멈춘다. 월드 애니메이션·크리처 동작은 계속된다 (크리처 대화, 수업, 스토리).</summary>
        GameClock,
        /// <summary>모든 게임 시간을 멈춘다(timeScale 0). UI 연출만 unscaled time으로 돈다 (책, 가방, 패널).</summary>
        Full
    }

    /// <summary>화면이 아닌 멈춤 원인 이름. 화면은 GameScreen 이름을 그대로 쓴다.</summary>
    public static class PauseCauses
    {
        public const string Story = "스토리";
        public const string Event = "이벤트";
        public const string Tutorial = "튜토리얼";
        public const string SceneTravel = "맵 이동";
        public const string Cheat = "치트";
        public const string GameOver = "마지막 날";

        public static string ForScreen(GameScreen screen) => "화면:" + screen;
    }

    public sealed class TimePause
    {
        private readonly Dictionary<string, PauseLevel> _causes = new Dictionary<string, PauseLevel>();

        /// <summary>멈춤 상태나 원인 목록이 바뀔 때마다.</summary>
        public event Action Changed;

        /// <summary>게임 시계가 멈춰 있는지 (전체 정지 포함).</summary>
        public bool IsClockPaused => _causes.Count > 0;

        /// <summary>전체 정지(timeScale 0)여야 하는지.</summary>
        public bool IsFullyPaused
        {
            get
            {
                foreach (var level in _causes.Values)
                    if (level == PauseLevel.Full) return true;
                return false;
            }
        }

        /// <summary>지금 등록된 원인과 단계 (디버그 표시용).</summary>
        public IReadOnlyDictionary<string, PauseLevel> Causes => _causes;

        public bool Has(string cause) => cause != null && _causes.ContainsKey(cause);

        /// <summary>원인을 등록한다. 이미 있으면 단계만 바꾼다.</summary>
        public void Acquire(PauseLevel level, string cause)
        {
            if (level == PauseLevel.None || string.IsNullOrEmpty(cause)) return;
            if (_causes.TryGetValue(cause, out var old) && old == level) return;
            _causes[cause] = level;
            Changed?.Invoke();
        }

        public void Release(string cause)
        {
            if (string.IsNullOrEmpty(cause) || !_causes.Remove(cause)) return;
            Changed?.Invoke();
        }

        /// <summary>조건에 따라 등록하거나 해제한다.</summary>
        public void Set(bool paused, PauseLevel level, string cause)
        {
            if (paused) Acquire(level, cause);
            else Release(cause);
        }

        public void Clear()
        {
            if (_causes.Count == 0) return;
            _causes.Clear();
            Changed?.Invoke();
        }
    }
}
