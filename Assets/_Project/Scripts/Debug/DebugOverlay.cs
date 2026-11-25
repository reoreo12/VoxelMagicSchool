// 개발용 디버그 표시(IMGUI 글자): 날짜·날 종류·D-n, 분 단위 게임 시각·시간대·경고, 배속, 흐름/멈춤과 멈춘 원인 목록, 다음 날 시작 보정.
// 의뢰·채집 줄은 해당 단계에서 AddSection으로 붙인다.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.DebugTools
{
    public sealed class DebugOverlay
    {
        private readonly StringBuilder _text = new StringBuilder();
        private readonly List<Action<StringBuilder, CheatTargets>> _sections = new List<Action<StringBuilder, CheatTargets>>();
        private GUIStyle _style;
        private GUIStyle _shadow;

        public bool Visible { get; set; } = true;

        /// <summary>다른 단계(의뢰·채집)가 줄을 덧붙인다.</summary>
        public void AddSection(Action<StringBuilder, CheatTargets> section) => _sections.Add(section);

        public void Draw(CheatTargets targets, float scale)
        {
            if (!Visible) return;
            Build(targets);
            int size = Mathf.RoundToInt(13 * scale);
            if (_style == null || _style.fontSize != size)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = size, wordWrap = false, normal = { textColor = new Color(1f, 1f, 0.85f) } };
                _shadow = new GUIStyle(_style) { normal = { textColor = new Color(0f, 0f, 0f, 0.85f) } };
            }
            var content = new GUIContent(_text.ToString());
            var area = _style.CalcSize(content);
            // 화면 오른쪽, 시간대 HUD 아래 (치트 패널은 왼쪽)
            var rect = new Rect(Screen.width - area.x - 12f * scale, Screen.height * 0.2f, area.x + 4f, area.y + 4f);
            GUI.Box(new Rect(rect.x - 4f, rect.y - 2f, rect.width + 8f, rect.height + 4f), GUIContent.none);
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), content, _shadow);
            GUI.Label(rect, content, _style);
        }

        private void Build(CheatTargets t)
        {
            _text.Clear();
            var clock = t.Clock;
            if (clock == null)
            {
                _text.Append("[디버그] 게임 시계 없음 (이 씬에 GameBootstrap이 없거나 Time·Calendar 설정이 비어 있음)");
                return;
            }
            var info = clock.Calendar.Get(clock.Day);
            int minute = Mathf.FloorToInt(clock.Minute);
            string dDay = clock.DaysUntilExam <= 0 ? "D-Day" : "D-" + clock.DaysUntilExam;
            _text.AppendFormat("{0}일째 · {1} {2} · {3}\n", clock.Day, KindName(info.Kind), info.Label, dDay);
            _text.AppendFormat("{0:00}:{1:00} {2}", minute / 60, minute % 60, clock.Period == TimeOfDay.Night ? "밤" : "방과후");
            if (clock.Warning != TimeWarning.None) _text.Append("  ⚠ ").Append(clock.Warning == TimeWarning.NightSoon ? "밤 임박" : "하루 끝 임박");
            _text.AppendFormat("\n배속 ×{0:0.#}{1}", clock.SpeedMultiplier, TimeScaleDriver.DebugWorldSpeed > 1.01f ? " (월드도)" : "");

            bool running = t.Driver != null && t.Driver.IsRunning;
            _text.Append(running ? " · 흐름" : " · 멈춤");
            var causes = new List<string>();
            foreach (var pair in clock.Holds.Causes) causes.Add(pair.Key);
            if (t.ScenePause != null)
                foreach (var pair in t.ScenePause.Causes) causes.Add(pair.Key + (pair.Value == PauseLevel.Full ? "(전체)" : ""));
            if (t.Driver == null) causes.Add("이 씬에 시계 구동기 없음");
            if (causes.Count > 0) _text.Append("\n멈춤 원인: ").Append(string.Join(", ", causes));

            var shift = t.Days?.NextDayShift;
            if (shift != null && shift.Requests.Count > 0)
            {
                _text.Append("\n다음 날 보정: ");
                foreach (var pair in shift.Requests) _text.AppendFormat("{0} {1:+0;-0} ", pair.Key, pair.Value);
                var config = clock.Config;
                int combined = shift.Combined(config.MinStartShiftMinutes, config.MaxStartShiftMinutes);
                int start = config.DayStartMinute + combined;
                _text.AppendFormat("→ {0:+0;-0}분 ({1:00}:{2:00} 시작)", combined, start / 60, start % 60);
            }
            if (t.Days != null && t.Days.IsFinished) _text.Append("\n마지막 날까지 끝남");

            foreach (var section in _sections) section(_text, t);
        }

        private static string KindName(DayKind kind) => kind switch
        {
            DayKind.Story => "[스토리]",
            DayKind.Event => "[이벤트·건너뜀]",
            _ => "[자유]"
        };
    }
}
#endif
