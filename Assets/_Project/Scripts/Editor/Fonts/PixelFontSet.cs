// 한글 픽셀 폰트(갈무리11·스타더스트 3.0, Regular·Bold) TMP 에셋 굽기 설정(UI_SPEC 1-1, 13장).
// 폰트마다 고정(Static) 에셋 + 목록에 없는 글자만 받는 예비(Dynamic) 에셋을 만든다. 값은 인스펙터에서 조절한다.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace VoxelMagic.EditorTools
{
    [CreateAssetMenu(menuName = "VoxelMagic/UI/픽셀 폰트 설정", fileName = "PixelFontSet")]
    public sealed class PixelFontSet : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [SerializeField] private Font _source;
            [SerializeField, Tooltip("만들 에셋 이름(확장자 없음)")] private string _assetName;
            [SerializeField, Tooltip("폰트 설계 크기(px). 도트 1칸이 아틀라스 1픽셀이 되는 크기")] private int _samplingSize = 12;
            [SerializeField] private GlyphRenderMode _renderMode = GlyphRenderMode.RASTER_HINTED;

            public Font Source => _source;
            public string AssetName => _assetName;
            public int SamplingSize => _samplingSize;
            public GlyphRenderMode RenderMode => _renderMode;

            public Entry(Font source, string assetName, int samplingSize)
            {
                _source = source;
                _assetName = assetName;
                _samplingSize = samplingSize;
            }
        }

        [SerializeField] private string _outputFolder = "Assets/_Project/Fonts/Pixel";
        [SerializeField, Tooltip("기본 문자 목록(KS X 1001 한글 2,350자 + 기호)")] private string _charsetPath = "Assets/_Project/Fonts/Pretendard/KoreanCharset.txt";
        [SerializeField, Tooltip("이 폴더의 스크립트·에셋에 쓰인 글자도 넣는다")]
        private List<string> _textSourceFolders = new List<string> { "Assets/_Project/Scripts/Core", "Assets/_Project/Scripts/UI", "Assets/_Project/Data" };
        [SerializeField] private int _padding = 1;
        [SerializeField] private int _staticAtlasSize = 1024;
        [SerializeField, Tooltip("예비(Dynamic) 에셋 아틀라스 크기. 드문 글자만 받으므로 작게")] private int _fallbackAtlasSize = 256;
        [SerializeField] private List<Entry> _fonts = new List<Entry>();

        public string OutputFolder => _outputFolder;
        public string CharsetPath => _charsetPath;
        public IReadOnlyList<string> TextSourceFolders => _textSourceFolders;
        public int Padding => _padding;
        public int StaticAtlasSize => _staticAtlasSize;
        public int FallbackAtlasSize => _fallbackAtlasSize;
        public IReadOnlyList<Entry> Fonts => _fonts;

        internal void SetFonts(IEnumerable<Entry> fonts) => _fonts = new List<Entry>(fonts);
    }
}
