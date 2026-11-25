// 화면 구석의 치트 패널(IMGUI 임시 UI, 모든 씬 공용). 키 없이 마우스로 쓴다: 왼쪽 가장자리 "치트" 버튼으로 열고 닫는다(` 키도 됨).
// 맨 위에는 늘 쓰는 줄(일시 정지·배속·디버그 표시), 아래는 탭. 동작은 TimeCheats 등 치트 동작 모음에 맡기고, 다른 단계는 AddTab으로 탭을 붙인다.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.DebugTools
{
    public sealed class CheatPanel
    {
        private const int WindowId = 0x5EED;

        private readonly List<(string name, Action<CheatTargets> draw)> _tabs = new List<(string, Action<CheatTargets>)>();
        private Rect _window = new Rect(48f, 150f, 360f, 430f);
        private int _tab;
        private Vector2 _scroll;
        private CheatTargets _targets;

        public CheatPanel()
        {
            AddTab("시간·날짜", DrawTimeTab);
        }

        public bool Visible { get; set; }
        /// <summary>배속을 월드 시간(timeScale)에도 걸지 (촬영용: 낚시·이동 등도 빨라짐).</summary>
        public bool WorldSpeedToo { get; private set; }
        public Func<bool> IsOverlayVisible { get; set; }
        public Action ToggleOverlay { get; set; }

        public void AddTab(string name, Action<CheatTargets> draw) => _tabs.Add((name, draw));

        public void Draw(CheatTargets targets, float scale)
        {
            _targets = targets;
            var matrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            // 패널이 닫혀 있을 때도 보이는 작은 여닫기 버튼 (왼쪽 가장자리, 키 없이 열기)
            float screenH = Screen.height / scale;
            if (GUI.Button(new Rect(0f, screenH * 0.5f - 14f, 44f, 28f), Visible ? "◀" : "치트")) Visible = !Visible;
            if (Visible) _window = GUI.Window(WindowId, _window, DrawWindow, "치트 (임시 UI · 제목줄을 끌어 옮김)");

            GUI.matrix = matrix;
        }

        private void DrawWindow(int id)
        {
            DrawQuickRow(_targets);

            GUILayout.BeginHorizontal();
            for (int i = 0; i < _tabs.Count; i++)
                if (GUILayout.Toggle(_tab == i, _tabs[i].name, GUI.skin.button)) _tab = i;
            GUILayout.EndHorizontal();

            _scroll = GUILayout.BeginScrollView(_scroll);
            if (_tab >= 0 && _tab < _tabs.Count) _tabs[_tab].draw(_targets);
            GUILayout.EndScrollView();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        // 늘 쓰는 줄: 일시 정지, 배속, 월드 배속, 디버그 표시
        private void DrawQuickRow(CheatTargets t)
        {
            var clock = t.Clock;
            GUILayout.BeginHorizontal();
            GUI.enabled = clock != null;
            if (GUILayout.Button(TimeCheats.IsPaused(clock) ? "▶ 다시 흐름" : "❚❚ 일시 정지", GUILayout.Width(120f))) TimeCheats.TogglePause(clock);
            GUI.enabled = true;
            bool overlay = IsOverlayVisible != null && IsOverlayVisible();
            if (GUILayout.Toggle(overlay, "디버그 표시", GUI.skin.button) != overlay) ToggleOverlay?.Invoke();
            GUILayout.EndHorizontal();

            if (clock == null) return;
            GUILayout.BeginHorizontal();
            foreach (float speed in TimeCheats.Speeds)
            {
                bool on = Mathf.Approximately(clock.SpeedMultiplier, speed);
                if (GUILayout.Toggle(on, "×" + speed, GUI.skin.button) && !on) TimeCheats.SetSpeed(clock, speed, WorldSpeedToo);
            }
            GUILayout.EndHorizontal();
            bool world = GUILayout.Toggle(WorldSpeedToo, " 월드(낚시·이동·애니메이션)도 같은 배속");
            if (world != WorldSpeedToo)
            {
                WorldSpeedToo = world;
                TimeCheats.SetSpeed(clock, clock.SpeedMultiplier, WorldSpeedToo);
            }
        }

        private void DrawTimeTab(CheatTargets t)
        {
            var clock = t.Clock;
            var days = t.Days;
            if (clock == null)
            {
                GUILayout.Label("게임 시계가 없는 씬입니다.");
                return;
            }

            GUILayout.Label("시각 이동 (같은 날)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("16:00")) TimeCheats.JumpTo(clock, 16);
            if (GUILayout.Button("19:00")) TimeCheats.JumpTo(clock, 19);
            if (GUILayout.Button("20:00")) TimeCheats.JumpTo(clock, 20);
            if (GUILayout.Button("23:00")) TimeCheats.JumpTo(clock, 23);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("23:59 (24:00 직전)")) TimeCheats.JumpTo(clock, 23, 59);
            if (GUILayout.Button("+1시간")) TimeCheats.AddHour(clock);
            GUILayout.EndHorizontal();

            GUILayout.Label("날짜로 이동 (그날 처음부터)");
            var calendar = clock.Calendar;
            for (int row = 0; row * 4 < calendar.DayCount; row++)
            {
                GUILayout.BeginHorizontal();
                for (int day = row * 4 + 1; day <= Mathf.Min(calendar.DayCount, row * 4 + 4); day++)
                {
                    int left = Mathf.Max(0, calendar.ExamDay - day);
                    string label = $"{day}일 {(left == 0 ? "D-Day" : "D-" + left)}";
                    if (GUILayout.Toggle(clock.Day == day, label, GUI.skin.button) && clock.Day != day) TimeCheats.GoToDay(days, day);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("오늘: " + calendar.Get(clock.Day).Label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("다음 날로")) TimeCheats.NextDay(days);
            if (days != null && days.Kind == DayKind.Story && GUILayout.Button("스토리 날 끝내기")) days.FinishStoryDay();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("스스로 하루 끝내기 (보정 없음)")) days?.EndDayVoluntarily();

            GUILayout.Label("다음 날 시작 보정 시험 (원인: 치트)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+60분")) TimeCheats.RequestShift(days, 60);
            if (GUILayout.Button("+30분")) TimeCheats.RequestShift(days, 30);
            if (GUILayout.Button("−30분")) TimeCheats.RequestShift(days, -30);
            if (GUILayout.Button("비우기")) TimeCheats.ClearShift(days);
            GUILayout.EndHorizontal();
        }
    }
}
#endif
