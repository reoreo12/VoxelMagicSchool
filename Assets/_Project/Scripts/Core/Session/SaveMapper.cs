// PlayerProgress ↔ SaveData 변환. 저장은 ID만 꺼내고, 불러오기는 DefinitionCatalog로 에셋을 찾으며 모르는 ID는 건너뛰고 경고로 남긴다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    public static class SaveMapper
    {
        public static SaveData ToData(PlayerProgress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            var data = new SaveData { SavedAtUtc = DateTime.UtcNow.ToString("o") };

            var inventory = progress.Inventory;
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                var slot = inventory.GetSlot(i);
                data.Slots.Add(new SlotRecord { ItemId = slot.IsEmpty ? null : slot.Item.Id, Count = slot.IsEmpty ? 0 : slot.Count });
            }
            data.SelectedSlot = inventory.SelectedIndex;
            data.PracticalScore = progress.Score.Practical;
            data.WrittenScore = progress.Score.Written;
            data.JournalItemIds.AddRange(progress.Journal.DeliveredIds);
            foreach (var request in progress.Requests.CompletedRequests)
                data.CompletedRequestIds.Add(DefinitionCatalog.IdOf(request));
            data.ActiveRequestId = DefinitionCatalog.IdOf(progress.Requests.Active);
            foreach (var plot in progress.FarmPlots)
            {
                if (plot.Seed == null) continue;
                data.FarmPlots.Add(new FarmPlotRecord
                    { SeedId = plot.Seed.Id, OriginX = plot.Origin.x, OriginY = plot.Origin.y, Watered = plot.Watered });
            }
            return data;
        }

        /// <summary>저장 내용을 진행 상태에 덮어쓴다. 모르는 ID 목록을 돌려준다 (비어 있으면 모두 찾음).</summary>
        public static List<string> Apply(SaveData data, PlayerProgress progress, DefinitionCatalog catalog)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            var unknown = new List<string>();

            var slots = new ItemStack[progress.Inventory.SlotCount];
            for (int i = 0; i < slots.Length && i < data.Slots.Count; i++)
            {
                var record = data.Slots[i];
                if (record == null || string.IsNullOrEmpty(record.ItemId) || record.Count <= 0) continue;
                if (catalog.TryGetItem(record.ItemId, out var item)) slots[i] = new ItemStack(item, record.Count);
                else unknown.Add(record.ItemId);
            }
            progress.Inventory.Restore(slots, data.SelectedSlot);
            progress.Score.Restore(data.PracticalScore, data.WrittenScore);
            progress.Journal.Restore(data.JournalItemIds);

            var completed = new List<RequestDefinition>();
            foreach (var id in data.CompletedRequestIds)
            {
                if (catalog.TryGetRequest(id, out var request)) completed.Add(request);
                else unknown.Add(id);
            }
            RequestDefinition active = null;
            if (!string.IsNullOrEmpty(data.ActiveRequestId) && !catalog.TryGetRequest(data.ActiveRequestId, out active))
                unknown.Add(data.ActiveRequestId);
            progress.Requests.Restore(completed, active);

            var plots = new List<FarmPlotState>();
            foreach (var record in data.FarmPlots)
            {
                if (record == null) continue;
                if (catalog.TryGetItem(record.SeedId, out var item) && item is SeedDefinition seed)
                    plots.Add(new FarmPlotState(seed, new Vector2Int(record.OriginX, record.OriginY), record.Watered));
                else unknown.Add(record.SeedId);
            }
            progress.SetFarmPlots(plots);
            return unknown;
        }
    }
}
