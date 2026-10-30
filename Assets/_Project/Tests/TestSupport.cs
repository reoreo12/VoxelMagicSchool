// 테스트용 도우미: private 직렬화 필드를 채워 ScriptableObject를 만들고, 고정 난수를 제공한다.
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VoxelMagic.Core;
using Object = UnityEngine.Object;

namespace VoxelMagic.Tests
{
    internal sealed class FixedRandom : IRandom
    {
        public FixedRandom(float value) { Value = value; }

        public float Value { get; set; }
        public float Range(float min, float max) => min + (max - min) * Value;
        public int Range(int minInclusive, int maxExclusive) => minInclusive;
    }

    internal static class TestAssets
    {
        private static readonly List<Object> Created = new List<Object>();

        public static T Create<T>(params (string field, object value)[] fields) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = typeof(T).Name;
            foreach (var (field, value) in fields) SetField(asset, field, value);
            Created.Add(asset);
            return asset;
        }

        public static void DestroyAll()
        {
            foreach (var asset in Created)
                if (asset != null) Object.DestroyImmediate(asset);
            Created.Clear();
        }

        public static ProduceDefinition Produce(string name, float minCm = 10f, float maxCm = 20f, int maxStack = 99)
            => Create<ProduceDefinition>(("_id", name), ("_displayName", name), ("_maxStack", maxStack),
                ("_sizeRangeCm", new FloatRange(minCm, maxCm)));

        public static SeedDefinition Seed(string name, ProduceDefinition produce, int width, int height)
            => Create<SeedDefinition>(("_id", name), ("_displayName", name), ("_produce", produce),
                ("_footprint", new Vector2Int(width, height)));

        public static ToolDefinition Wand()
            => Create<ToolDefinition>(("_id", "wand"), ("_displayName", "지팡이"), ("_maxStack", 1));

        public static FishDefinition Fish(string name)
            => Create<FishDefinition>(("_id", name), ("_displayName", name),
                ("_sizeRangeCm", new FloatRange(10f, 20f)), ("_dartChance", 0f));

        public static RequestDefinition Request(string title, int baseReward, ItemAmount[] needs, ItemAmount[] given)
            => Create<RequestDefinition>(("_title", title), ("_baseReward", baseReward),
                ("_requirements", new List<ItemAmount>(needs)), ("_givenItems", new List<ItemAmount>(given)));

        private static void SetField(object target, string fieldName, object value)
        {
            for (var type = target.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null) continue;
                field.SetValue(target, value);
                return;
            }
            throw new ArgumentException($"{target.GetType().Name}에 {fieldName} 필드가 없습니다.");
        }
    }
}
