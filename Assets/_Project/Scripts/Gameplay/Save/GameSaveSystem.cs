// 플레이어 진행 상태를 만들거나 저장 파일에서 불러오고(처음 한 번), 씬을 떠날 때·게임을 끌 때 저장한다.
// 지금은 슬롯 0 고정. 슬롯 선택·불러오기 화면은 Docs/HANDOFF_SAVE_LOAD.md 참고.
using System;
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public static class GameSaveSystem
    {
        public const int DefaultSlot = 0;

        private static ISaveStore _store;

        public static ISaveStore Store => _store ??= new JsonFileSaveStore();
        public static int ActiveSlot { get; private set; } = DefaultSlot;
        /// <summary>꺼 두면 씬 이동·종료 때 저장하지 않는다 (부트스트랩 설정).</summary>
        public static bool AutoSaveEnabled { get; set; } = true;

        /// <summary>이미 진행 중이면 그대로, 아니면 저장 파일에서 불러오고, 파일이 없으면 새로 만들어 onNewGame(시작 아이템 지급)을 부른다.</summary>
        public static PlayerProgress GetOrCreateProgress(GameBalanceConfig balance, IEnumerable<RequestDefinition> requests,
            DefinitionCatalog catalog, Action<PlayerProgress> onNewGame)
        {
            if (GameSession.HasProgress) return GameSession.Progress;

            var progress = PlayerProgress.CreateNew(balance, requests);
            if (!TryLoadInto(progress, catalog)) onNewGame?.Invoke(progress);
            GameSession.Begin(progress);
            return progress;
        }

        /// <summary>지금 진행 상태를 활성 슬롯에 저장한다. 저장했으면 true.</summary>
        public static bool Save()
        {
            if (!GameSession.HasProgress) return false;
            GameSession.Capture();
            try
            {
                var data = SaveMapper.ToData(GameSession.Progress);
                Store.Write(ActiveSlot, JsonUtility.ToJson(data, true));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[저장] 슬롯 {ActiveSlot} 저장 실패: {e.Message}");
                return false;
            }
        }

        public static void AutoSave()
        {
            if (AutoSaveEnabled) Save();
        }

        private static bool TryLoadInto(PlayerProgress progress, DefinitionCatalog catalog)
        {
            if (!Store.TryRead(ActiveSlot, out string json)) return false;
            if (catalog == null)
            {
                Debug.LogWarning("[저장] DefinitionCatalog가 연결되지 않아 저장 파일을 불러오지 않습니다 (메뉴 VoxelMagic/저장/정의 목록 채우기).");
                return false;
            }
            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return false;
                if (data.Version > SaveData.CurrentVersion)
                    Debug.LogWarning($"[저장] 더 새 형식의 저장 파일입니다 (파일 {data.Version}, 게임 {SaveData.CurrentVersion}). 아는 부분만 불러옵니다.");
                var unknown = SaveMapper.Apply(data, progress, catalog);
                if (unknown.Count > 0)
                    Debug.LogWarning($"[저장] 찾지 못한 ID {unknown.Count}개를 건너뛰었습니다: {string.Join(", ", unknown)}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[저장] 슬롯 {ActiveSlot}를 불러오지 못했습니다: {e.Message}");
                return false;
            }
        }

        private static void OnQuitting() => AutoSave();

        // 도메인 리로드를 끈 플레이 모드에서도 한 번만 구독되게 초기화 후 다시 건다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Application.quitting -= OnQuitting;
            Application.quitting += OnQuitting;
            _store = null;
            ActiveSlot = DefaultSlot;
            AutoSaveEnabled = true;
        }
    }
}
