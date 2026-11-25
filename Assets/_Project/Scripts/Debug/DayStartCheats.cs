// 하루 시작 처리 치트·디버그 표시: 채집 자리 다시 뽑기와 켜진 자리·희귀도 표시, 작물 모두 다 자라게, 게시판 새로 뽑기와 오늘 의뢰 표시, 다음 낚시 희귀도 고정.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Text;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.DebugTools
{
    public static class DayStartCheats
    {
        private static bool s_showSpots = true;

        public static void Register(CheatPanel panel, DebugOverlay overlay)
        {
            panel.AddTab("재료·게시판", DrawTab);
            overlay.AddSection(AppendLines);
        }

        private static void DrawTab(CheatTargets t)
        {
            var progress = t.Context?.Progress ?? GameSession.Progress;
            if (progress?.Clock == null)
            {
                GUILayout.Label("진행 상태(시계)가 없는 씬입니다.");
                return;
            }
            int day = progress.Clock.Day;

            GUILayout.Label("채집");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("채집 자리 다시 뽑기") && progress.GatherStep != null) progress.GatherStep.Reroll(day);
            s_showSpots = GUILayout.Toggle(s_showSpots, " 켜진 자리 표시");
            GUILayout.EndHorizontal();

            GUILayout.Label("농작");
            if (GUILayout.Button("작물 모두 다 자라게")) progress.Farm?.GrowAll();

            GUILayout.Label("게시판");
            if (GUILayout.Button("게시판 새로 뽑기") && progress.BoardStep != null) progress.BoardStep.Refresh(day);

            GUILayout.Label("다음 낚시 한 번 희귀도 고정");
            var fishing = t.Context?.Fishing;
            GUI.enabled = fishing != null;
            GUILayout.BeginHorizontal();
            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
                if (GUILayout.Button(ItemTypeNames.Of(rarity))) fishing.ForcedNextRarity = rarity;
            if (GUILayout.Button("해제")) fishing.ForcedNextRarity = null;
            GUILayout.EndHorizontal();
            GUI.enabled = true;
            if (fishing?.ForcedNextRarity != null) GUILayout.Label("→ 다음 입질: " + ItemTypeNames.Of(fishing.ForcedNextRarity.Value));
        }

        private static void AppendLines(StringBuilder text, CheatTargets t)
        {
            var progress = t.Context?.Progress ?? GameSession.Progress;
            if (progress == null) return;

            if (progress.Board != null)
            {
                text.Append("\n게시판(").Append(progress.Board.Day).Append("일): ");
                if (progress.Board.Offers.Count == 0) text.Append("없음");
                foreach (var r in progress.Board.Offers)
                    text.Append('[').Append(DifficultyName(r.Difficulty)).Append("] ").Append(r.Title).Append("  ");
            }

            var day = progress.Gathering;
            if (s_showSpots && day != null)
            {
                text.Append("\n채집 자리(").Append(day.Day).Append("일):");
                foreach (GatherHabitatKind kind in Enum.GetValues(typeof(GatherHabitatKind)))
                {
                    text.Append("\n  ").Append(KindName(kind)).Append(' ');
                    for (int i = 0; i < day.SpotsPerKind; i++)
                    {
                        var m = day.Get(kind, i);
                        text.Append(m == null ? "· " : $"{i}:{m.DisplayName}({ItemTypeNames.Of(m.Rarity)}) ");
                    }
                }
            }

            if (progress.Farm != null && progress.Farm.Plots.Count > 0)
            {
                text.Append("\n작물: ");
                foreach (var plot in progress.Farm.Plots)
                    text.Append(plot.Seed.Produce != null ? plot.Seed.Produce.DisplayName : plot.Seed.DisplayName)
                        .Append(CropState(plot, progress.Clock)).Append(' ');
            }
        }

        private static string CropState(FarmPlot plot, GameClock clock)
        {
            if (plot.IsGrown) return "(다 자람)";
            if (!plot.IsWatered) return "(물 안 줌)";
            if (plot.Seed.GrowsNextDay) return "(내일 아침)";
            float left = plot.WateredAt + plot.Seed.GrowHours * 60f - (clock != null ? clock.AbsoluteMinute : 0f);
            int minutes = Mathf.Max(0, Mathf.CeilToInt(left));
            return $"({minutes / 60}:{minutes % 60:00} 남음)";
        }

        private static string DifficultyName(RequestDifficulty d) => d switch
        {
            RequestDifficulty.Hard => "어려움",
            RequestDifficulty.Normal => "보통",
            RequestDifficulty.Tutorial => "튜토리얼",
            _ => "쉬움"
        };

        private static string KindName(GatherHabitatKind kind) => kind switch
        {
            GatherHabitatKind.TreeShade => "그늘",
            GatherHabitatKind.Bush => "덤불",
            GatherHabitatKind.RockCrevice => "바위틈",
            _ => "평지"
        };
    }
}
#endif
