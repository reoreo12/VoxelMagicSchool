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
            foreach (var request in progress.Requests.DeliveredRequests)
                data.CompletedRequestIds.Add(DefinitionCatalog.IdOf(request));
            foreach (var accepted in progress.Requests.Active)
                data.AcceptedRequestIds.Add(DefinitionCatalog.IdOf(accepted.Request));
            foreach (var request in progress.Requests.AbandonedRequests)
                data.AbandonedRequestIds.Add(DefinitionCatalog.IdOf(request));
            data.NextRequestId = progress.Requests.NextId;
            if (progress.Farm != null) progress.CaptureFarm(progress.Farm);
            foreach (var plot in progress.FarmPlots)
            {
                if (plot.Seed == null) continue;
                data.FarmPlots.Add(new FarmPlotRecord
                    { SeedId = plot.Seed.Id, OriginX = plot.Origin.x, OriginY = plot.Origin.y, Watered = plot.Watered,
                      WateredAt = plot.WateredAt, Grown = plot.Grown, Facing = (int)plot.Facing });
            }
            var world = progress.CaptureWorld();
            if (world != null)
            {
                data.HasWorld = true;
                data.Day = world.Day;
                data.Minute = world.Minute;
                foreach (var pair in world.Shifts) data.NextDayShifts.Add(new ShiftRecord { Source = pair.Key, Minutes = pair.Value });
                data.BoardDay = world.BoardDay;
                foreach (var r in world.Offers) data.BoardOfferIds.Add(DefinitionCatalog.IdOf(r));
                foreach (var h in world.BoardHistory)
                    data.BoardHistory.Add(new BoardHistoryRecord { RequestId = DefinitionCatalog.IdOf(h.Request), LastDay = h.LastDay, Times = h.Times });
                data.GatherDay = world.GatherDay;
                foreach (var s in world.GatherSpots)
                    data.GatherSpots.Add(new GatherSpotRecord { Kind = (int)s.Kind, Spot = s.Spot, MaterialId = s.Material.Id });
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

            progress.Requests.Restore(Requests(data.AcceptedRequestIds, catalog, unknown),
                Requests(data.CompletedRequestIds, catalog, unknown), Requests(data.AbandonedRequestIds, catalog, unknown),
                data.NextRequestId);

            var plots = new List<FarmPlotState>();
            foreach (var record in data.FarmPlots)
            {
                if (record == null) continue;
                if (catalog.TryGetItem(record.SeedId, out var item) && item is SeedDefinition seed)
                    plots.Add(new FarmPlotState(seed, new Vector2Int(record.OriginX, record.OriginY), record.Watered, record.WateredAt, record.Grown,
                        (FarmFacing)Mathf.Clamp(record.Facing, 0, 3)));
                else unknown.Add(record.SeedId);
            }
            progress.SetFarmPlots(plots);

            if (data.HasWorld)
            {
                var world = new WorldSnapshot { Day = data.Day, Minute = data.Minute, BoardDay = data.BoardDay, GatherDay = data.GatherDay };
                foreach (var s in data.NextDayShifts)
                    if (s != null && !string.IsNullOrEmpty(s.Source)) world.Shifts.Add(new KeyValuePair<string, int>(s.Source, s.Minutes));
                world.Offers.AddRange(Requests(data.BoardOfferIds, catalog, unknown));
                foreach (var h in data.BoardHistory)
                {
                    if (h == null || string.IsNullOrEmpty(h.RequestId)) continue;
                    if (catalog.TryGetRequest(h.RequestId, out var request)) world.BoardHistory.Add(new BoardHistoryEntry(request, h.LastDay, h.Times));
                    else unknown.Add(h.RequestId);
                }
                foreach (var s in data.GatherSpots)
                {
                    if (s == null || string.IsNullOrEmpty(s.MaterialId)) continue;
                    if (catalog.TryGetItem(s.MaterialId, out var item) && item is GatherMaterialDefinition material)
                        world.GatherSpots.Add(new GatherSpotEntry((GatherHabitatKind)s.Kind, s.Spot, material));
                    else unknown.Add(s.MaterialId);
                }
                progress.SetWorld(world);
            }
            return unknown;
        }

        private static List<RequestDefinition> Requests(List<string> ids, DefinitionCatalog catalog, List<string> unknown)
        {
            var list = new List<RequestDefinition>();
            if (ids == null) return list;
            foreach (var id in ids)
            {
                if (string.IsNullOrEmpty(id)) continue;
                if (catalog.TryGetRequest(id, out var request)) list.Add(request);
                else unknown.Add(id);
            }
            return list;
        }
    }
}
