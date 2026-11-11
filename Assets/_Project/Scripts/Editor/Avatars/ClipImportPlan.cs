// 동작 클립 임포트 계획 (에디터 전용 데이터): 어떤 애니메이션 파일을 몇 프레임씩 잘라 어떤 이름의 Humanoid 클립으로 만들지, 반복 여부, 뒤집은 클립, Animation Event.
// 메뉴 VoxelMagic/아바타/동작 클립 준비가 이 표대로 임포트한다. 새 동작은 표에 한 줄 추가하면 된다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.EditorTools
{
    [CreateAssetMenu(menuName = "VoxelMagic/Avatar/Clip Import Plan", fileName = "ClipImportPlan")]
    public sealed class ClipImportPlan : ScriptableObject
    {
        /// <summary>클립 안의 한 순간에 보낼 Animation Event (CharacterAnimationEvents.OnActionEvent의 문자열 값).</summary>
        [Serializable]
        public sealed class EventMark
        {
            [Tooltip("이벤트 이름 (CastRelease = 찌를 놓는 순간, WaterApply = 물이 들어가는 순간)")]
            [SerializeField] private string _id;
            [Tooltip("클립 안 위치 (0 = 처음, 1 = 끝)")]
            [SerializeField, Range(0f, 1f)] private float _normalizedTime;

            public EventMark() { }

            public EventMark(string id, float normalizedTime)
            {
                _id = id;
                _normalizedTime = normalizedTime;
            }

            public string Id => _id;
            public float NormalizedTime => _normalizedTime;
        }

        [Serializable]
        public sealed class Segment
        {
            [SerializeField] private string _name;
            [SerializeField, Min(0)] private int _firstFrame;
            [Tooltip("-1이면 파일 끝까지")]
            [SerializeField] private int _lastFrame = -1;
            [SerializeField] private bool _loop;
            [Tooltip("거꾸로 재생하는 클립(<이름>_Reverse.anim)도 만든다")]
            [SerializeField] private bool _makeReversed;
            [SerializeField] private List<EventMark> _events = new List<EventMark>();

            public Segment() { }

            public Segment(string name, int firstFrame, int lastFrame, bool loop, bool makeReversed)
            {
                _name = name;
                _firstFrame = firstFrame;
                _lastFrame = lastFrame;
                _loop = loop;
                _makeReversed = makeReversed;
            }

            public string Name => _name;
            public int FirstFrame => _firstFrame;
            public int LastFrame => _lastFrame;
            public bool Loop => _loop;
            public bool MakeReversed => _makeReversed;
            public IReadOnlyList<EventMark> Events => _events;
        }

        [Serializable]
        public sealed class Source
        {
            [Tooltip("Mixamo 등 애니메이션 FBX")]
            [SerializeField] private GameObject _file;
            [SerializeField] private List<Segment> _segments = new List<Segment>();

            public Source() { }

            public Source(GameObject file, params Segment[] segments)
            {
                _file = file;
                _segments = new List<Segment>(segments);
            }

            public GameObject File => _file;
            public IReadOnlyList<Segment> Segments => _segments;
        }

        [SerializeField] private List<Source> _sources = new List<Source>();

        public IReadOnlyList<Source> Sources => _sources;

        public void SetSources(IEnumerable<Source> sources) => _sources = new List<Source>(sources);
    }
}
