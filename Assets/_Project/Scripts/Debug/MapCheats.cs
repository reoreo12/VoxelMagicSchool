// 맵 이동 치트 ("이동" 탭): 버튼 하나로 정해 둔 씬의 도착 지점(SceneSpawnPoint ID)으로 간다. 문을 지날 때와 같은 페이드·자동 저장을 거친다.
// 같은 씬 안의 다른 지점도 씬을 다시 불러와 옮긴다. 굴 입구의 "밤에만" 같은 제한은 무시한다.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.DebugTools
{
    public static class MapCheats
    {
        private readonly struct Destination
        {
            public Destination(string label, string scene, string spawnId)
            {
                Label = label;
                Scene = scene;
                SpawnId = spawnId;
            }

            public string Label { get; }
            public string Scene { get; }
            public string SpawnId { get; }
        }

        // 도착 지점 ID는 각 씬의 SceneSpawnPoint와 같아야 한다 (복도 가운데 CheatCenter는 치트 전용).
        private static readonly Destination[] Destinations =
        {
            new Destination("크리처 굴 (들어온 포탈)", "Creature_Cave", "FromBackMountain"),
            new Destination("뒷산 (복도에서 나온 자리)", "BackMountain", "FromCorridor"),
            new Destination("뒷산 · 크리처 굴 입구 앞", "BackMountain", "FromCave"),
            new Destination("텃밭 (복도에서 나온 자리)", "FarmPond", "FromCorridor"),
            new Destination("복도 (가운데)", "Corridor_SideScroll", "CheatCenter"),
        };

        public static void Register(CheatPanel panel) => panel.AddTab("이동", DrawTab);

        private static void DrawTab(CheatTargets t)
        {
            GUILayout.Label($"지금 씬: {SceneManager.GetActiveScene().name}");
            GUI.enabled = !SceneTravel.IsTraveling;
            foreach (var d in Destinations)
                if (GUILayout.Button(d.Label)) TravelTo(d);
            GUI.enabled = true;
            if (SceneTravel.IsTraveling) GUILayout.Label("이동 중…");
        }

        private static void TravelTo(Destination d)
        {
            if (!Application.CanStreamedLevelBeLoaded(d.Scene))
            {
                Debug.LogWarning($"[Cheat] '{d.Scene}' 씬이 Build Settings에 없습니다.");
                return;
            }
            var director = Object.FindFirstObjectByType<SceneTransitionDirector>();
            if (director != null)
            {
                director.TravelTo(d.Scene, d.SpawnId);
                return;
            }
            // 이동 연출이 없는 씬: 저장 후 바로 불러온다 (도착 씬의 진행자가 페이드 인과 입력 풀기를 맡음).
            if (!SceneTravel.Begin(d.SpawnId)) return;
            GameSaveSystem.AutoSave();
            SceneManager.LoadScene(d.Scene);
        }
    }
}
#endif
