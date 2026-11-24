// Game View 해상도를 버튼 하나로 바꾼다(UI_SPEC 2장 종횡비 프리뷰). 목록에 없으면 고정 해상도로 추가하고 고른다.
// Unity가 공개하지 않은 GameViewSizes를 리플렉션으로 쓰므로, 에디터 버전이 바뀌어 실패하면 false를 돌려주고 경고만 남긴다.
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class GameViewSizeSwitcher
    {
        private const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static bool TrySet(int width, int height, string label)
        {
            try
            {
                var assembly = typeof(Editor).Assembly;
                var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
                var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var sizes = singleton.GetProperty("instance", Any).GetValue(null);
                var groupType = sizesType.GetProperty("currentGroupType", Any).GetValue(sizes);
                var group = sizesType.GetMethod("GetGroup", Any).Invoke(sizes, new[] { groupType });
                var groupClass = group.GetType();

                int index = FindIndex(group, groupClass, width, height);
                if (index < 0)
                {
                    var sizeType = assembly.GetType("UnityEditor.GameViewSize");
                    var kindType = assembly.GetType("UnityEditor.GameViewSizeType");
                    var fixedKind = Enum.Parse(kindType, "FixedResolution");
                    var size = Activator.CreateInstance(sizeType, fixedKind, width, height, label);
                    groupClass.GetMethod("AddCustomSize", Any).Invoke(group, new[] { size });
                    index = FindIndex(group, groupClass, width, height);
                }

                var gameViewType = assembly.GetType("UnityEditor.GameView");
                var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
                var select = gameViewType.GetMethod("SizeSelectionCallback", Any);
                if (select != null) select.Invoke(gameView, new object[] { index, null });
                else gameViewType.GetProperty("selectedSizeIndex", Any).SetValue(gameView, index);
                gameView.Repaint();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UI 작업대] Game View 해상도를 바꾸지 못했습니다. Game View 위쪽 해상도 목록에서 직접 고르세요. ({e.GetBaseException().Message})");
                return false;
            }
        }

        private static int FindIndex(object group, Type groupClass, int width, int height)
        {
            int count = (int)groupClass.GetMethod("GetTotalCount", Any).Invoke(group, null);
            var get = groupClass.GetMethod("GetGameViewSize", Any);
            for (int i = 0; i < count; i++)
            {
                var size = get.Invoke(group, new object[] { i });
                var type = size.GetType();
                if ((int)type.GetProperty("width", Any).GetValue(size) == width &&
                    (int)type.GetProperty("height", Any).GetValue(size) == height &&
                    type.GetProperty("sizeType", Any).GetValue(size).ToString() == "FixedResolution")
                    return i;
            }
            return -1;
        }
    }
}
