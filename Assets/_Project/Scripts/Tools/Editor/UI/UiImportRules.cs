// UI 스프라이트 임포트 규칙 데이터(UI_SPEC.md 4장·각 폴더 README). 폴더·파일 이름별 9-slice 경계와 시트 자르기 값을 둔다.
// 공통 값(Point·압축 없음·Full Rect·PPU)도 여기서 조절한다. 실제 적용은 UiImportRuleApplier가 한다.
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    [CreateAssetMenu(menuName = "VoxelMagic/UI/임포트 규칙", fileName = "UiImportRules")]
    public sealed class UiImportRules : ScriptableObject
    {
        [SerializeField, Tooltip("규칙을 적용할 최상위 폴더")] private string _rootFolder = "Assets/_Project/Art/UI";
        [SerializeField, Tooltip("이 이름이 경로에 들어간 파일은 건너뛴다(미리보기 등)")] private List<string> _skipFolderNames = new() { "_preview" };
        [SerializeField] private int _pixelsPerUnit = 16;
        [SerializeField] private FilterMode _filterMode = FilterMode.Point;
        [SerializeField, Tooltip("위에서부터 처음 맞는 규칙 하나만 쓴다. 구체적인 규칙을 위에 둔다")] private List<UiImportRule> _rules = new();

        public string RootFolder => _rootFolder;
        public IReadOnlyList<string> SkipFolderNames => _skipFolderNames;
        public int PixelsPerUnit => _pixelsPerUnit;
        public FilterMode FilterMode => _filterMode;
        public IReadOnlyList<UiImportRule> Rules => _rules;

        internal void ResetRules(IEnumerable<UiImportRule> rules) => _rules = new List<UiImportRule>(rules);

        /// <summary>같은 경로 패턴이 없는 기본 규칙만 뒤에 붙인다(사용자가 고친 규칙은 그대로). 붙였으면 true.</summary>
        internal bool AddMissing(IEnumerable<UiImportRule> rules)
        {
            bool added = false;
            foreach (var rule in rules)
            {
                if (_rules.Exists(r => r.PathPattern == rule.PathPattern)) continue;
                _rules.Add(rule);
                added = true;
            }
            return added;
        }

        // 루트 기준 상대 경로(예: "book_ui/tabs/tab_red_normal.png")에 맞는 첫 규칙. 없으면 null(공통 값만 적용).
        public UiImportRule Find(string relativePath)
        {
            foreach (var rule in _rules)
                if (rule.Matches(relativePath)) return rule;
            return null;
        }
    }

    [Serializable]
    public sealed class UiImportRule
    {
        [SerializeField, Tooltip("루트 기준 경로 패턴. * 는 '/'를 넘지 않는 아무 글자. 예: book_ui/tabs/*.png")] private string _pathPattern;
        [SerializeField, Tooltip("9-slice 경계 (왼쪽·위·오른쪽·아래, UI_SPEC 표기 순서)")] private int _left, _top, _right, _bottom;
        [SerializeField, Tooltip("켜면 Sprite Mode Multiple로 칸 크기대로 자른다")] private bool _sliceGrid;
        [SerializeField] private Vector2Int _cellSize;
        [SerializeField, Tooltip("채우면 이 칸들만 이름을 붙여 자른다(스프라이트시트에서 필요한 조각만, 위쪽 기준 좌표)")]
        private List<UiSliceRect> _slices = new List<UiSliceRect>();

        [NonSerialized] private Regex _regex;

        public string PathPattern => _pathPattern;
        public bool SliceGrid => _sliceGrid;
        public Vector2Int CellSize => _cellSize;
        public IReadOnlyList<UiSliceRect> Slices => _slices;
        public bool SliceNamed => _slices != null && _slices.Count > 0;
        public bool HasBorder => _left + _top + _right + _bottom > 0;

        // Unity의 spriteBorder는 (x=왼쪽, y=아래, z=오른쪽, w=위) 순서라 여기서 바꿔 준다.
        public Vector4 ToUnityBorder() => new Vector4(_left, _bottom, _right, _top);

        public bool Matches(string relativePath)
        {
            if (string.IsNullOrEmpty(_pathPattern)) return false;
            _regex ??= new Regex("^" + Regex.Escape(_pathPattern).Replace("\\*", "[^/]*") + "$", RegexOptions.IgnoreCase);
            return _regex.IsMatch(relativePath);
        }

        public static UiImportRule Create(string pattern, int left, int top, int right, int bottom) =>
            new() { _pathPattern = pattern, _left = left, _top = top, _right = right, _bottom = bottom };

        public static UiImportRule Grid(string pattern, Vector2Int cellSize) =>
            new() { _pathPattern = pattern, _sliceGrid = true, _cellSize = cellSize };

        public static UiImportRule Named(string pattern, params UiSliceRect[] slices) =>
            new() { _pathPattern = pattern, _slices = new List<UiSliceRect>(slices) };
    }

    /// <summary>스프라이트시트에서 자를 한 조각. 좌표는 그림의 왼쪽 위 기준(px), 경계는 왼쪽·위·오른쪽·아래.</summary>
    [Serializable]
    public struct UiSliceRect
    {
        [SerializeField] private string _name;
        [SerializeField] private int _x, _yFromTop, _width, _height;
        [SerializeField] private int _left, _top, _right, _bottom;

        public UiSliceRect(string name, int x, int yFromTop, int width, int height, int left = 0, int top = 0, int right = 0, int bottom = 0)
        {
            _name = name;
            _x = x;
            _yFromTop = yFromTop;
            _width = width;
            _height = height;
            _left = left;
            _top = top;
            _right = right;
            _bottom = bottom;
        }

        public string Name => _name;

        public Rect ToUnityRect(int textureHeight) => new Rect(_x, textureHeight - _yFromTop - _height, _width, _height);
        public Vector4 ToUnityBorder() => new Vector4(_left, _bottom, _right, _top);
    }
}
