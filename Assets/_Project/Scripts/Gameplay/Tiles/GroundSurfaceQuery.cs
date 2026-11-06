// 씬의 모든 구운 바닥에 "이 위치의 바닥 종류"를 묻는 곳. 바닥이 여러 개여도 하나만 부르면 된다 (물리·충돌체 불필요).
// 예: 플레이어 걸음마다 GroundSurfaceQuery.TryGet(발 위치, out var surface) → surface에 맞는 파티클.
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public static class GroundSurfaceQuery
    {
        private static readonly List<GroundSurfaceSource> Sources = new List<GroundSurfaceSource>();

        public static int SourceCount => Sources.Count;

        internal static void Register(GroundSurfaceSource source)
        {
            if (!Sources.Contains(source)) Sources.Add(source);
        }

        internal static void Unregister(GroundSurfaceSource source) => Sources.Remove(source);

        /// <summary>위치 아래 바닥 종류. 어느 바닥에도 없거나 칠하지 않은 칸이면 false</summary>
        public static bool TryGet(Vector3 worldPosition, out GroundSurface surface)
        {
            for (int i = Sources.Count - 1; i >= 0; i--)
            {
                var source = Sources[i];
                if (source == null) { Sources.RemoveAt(i); continue; }
                if (source.TryGetSurface(worldPosition, out surface)) return true;
            }
            surface = null;
            return false;
        }
    }
}
