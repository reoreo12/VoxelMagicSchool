// 대사창에 나오는 화자(크리처 등) 데이터: 이름표 이름, 기본 초상화, 표정 키별 초상화(UI_SPEC 6-2). 표정 키가 없으면 기본 초상화.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Speaker", fileName = "Speaker_")]
    public sealed class SpeakerProfile : ScriptableObject
    {
        [Serializable]
        public struct Expression
        {
            [Tooltip("표정 키 (예: smile, angry). 대사 줄에서 이 키를 고른다")] public string Key;
            [Tooltip("72×72 초상화 (테마 Dialogue › Portrait Size)")] public Sprite Portrait;
        }

        [SerializeField, Tooltip("이름표에 보일 이름 (6자까지 12px 굵게, 더 길면 자동으로 작은 글씨)")] private string _displayName;
        [SerializeField, Tooltip("72×72 기본 초상화")] private Sprite _defaultPortrait;
        [SerializeField] private List<Expression> _expressions = new List<Expression>();

        public string DisplayName => _displayName;

        /// <summary>표정 키에 맞는 초상화. 비었거나 없는 키면 기본 초상화.</summary>
        public Sprite Portrait(string expression)
        {
            if (!string.IsNullOrEmpty(expression))
                foreach (var e in _expressions)
                    if (e.Key == expression && e.Portrait != null) return e.Portrait;
            return _defaultPortrait;
        }
    }
}
