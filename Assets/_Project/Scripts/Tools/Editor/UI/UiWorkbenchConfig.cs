// UI 작업대 설정: 해상도 프리뷰 목록(UI_SPEC 2장의 16:9·16:10·21:9·4:3)과 프리팹을 구워 넣을 폴더.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    [CreateAssetMenu(menuName = "VoxelMagic/UI/UI 작업대 설정", fileName = "UiWorkbenchConfig")]
    public sealed class UiWorkbenchConfig : ScriptableObject
    {
        [Serializable]
        public struct Resolution
        {
            [SerializeField] private string _aspect;
            [SerializeField] private int _width;
            [SerializeField] private int _height;

            public Resolution(string aspect, int width, int height)
            {
                _aspect = aspect;
                _width = width;
                _height = height;
            }

            public string Aspect => _aspect;
            public int Width => _width;
            public int Height => _height;
            public string Label => $"{_width}×{_height}";
        }

        [SerializeField] private List<Resolution> _resolutions = new List<Resolution>
        {
            new Resolution("16:9", 1280, 720),
            new Resolution("16:9", 1920, 1080),
            new Resolution("16:9", 2560, 1440),
            new Resolution("16:9", 3840, 2160),
            new Resolution("16:10", 1280, 800),
            new Resolution("16:10", 1920, 1200),
            new Resolution("21:9", 2560, 1080),
            new Resolution("21:9", 3440, 1440),
            new Resolution("4:3", 1024, 768),
            new Resolution("4:3", 1440, 1080),
        };
        [SerializeField] private string _bakeFolder = "Assets/_Project/Prefabs/UI";
        [SerializeField, TextArea, Tooltip("폰트 비교 문장 (대사 한 줄, 의뢰 제목 + 진행, 숫자·기호, UI_SPEC 1-1)")]
        private List<string> _fontSamples = new List<string>
        {
            "크리처: 오늘은 텃밭에 물을 꼭 줘야 해.",
            "약초 모으기  2/5",
            "D-3  필기 120점 · 실기 85점!",
        };

        public IReadOnlyList<Resolution> Resolutions => _resolutions;
        public string BakeFolder => _bakeFolder;
        public IReadOnlyList<string> FontSamples => _fontSamples;
    }
}
