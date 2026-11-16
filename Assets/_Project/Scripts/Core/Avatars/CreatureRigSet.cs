// 한 크리처 프리팹에 들어 있는 여러 리그(뼈 구조)와 그 리그용 클립 목록. 클립을 재생할 때 그 리그의 메시만 켠다.
// 리그마다 뼈 이름이 달라 클립은 자기 리그의 뼈만 움직이므로, 안 쓰는 리그의 메시는 꺼서 그리기·스키닝 비용을 한 벌로 유지한다.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    public sealed class CreatureRigSet : MonoBehaviour
    {
        [Serializable]
        public sealed class Group
        {
            public string Name;
            public SkinnedMeshRenderer Renderer;
            public AnimationClip[] Clips;
        }

        [SerializeField] private Group[] _groups = new Group[0];

        public int GroupCount => _groups.Length;

        public Group GroupOf(AnimationClip clip)
        {
            if (clip == null) return null;
            foreach (var group in _groups)
                if (group.Clips != null && Array.IndexOf(group.Clips, clip) >= 0) return group;
            return null;
        }

        public bool Contains(AnimationClip clip) => GroupOf(clip) != null;

        /// <summary>클립이 속한 리그의 메시만 켜고 나머지는 끈다. 어느 리그에도 없는 클립이면 false.</summary>
        public bool Activate(AnimationClip clip)
        {
            var target = GroupOf(clip);
            if (target == null) return false;
            foreach (var group in _groups)
                if (group.Renderer != null) group.Renderer.enabled = group == target;
            return true;
        }
    }
}
