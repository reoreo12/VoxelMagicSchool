// 고정(Static) 폰트 아틀라스에 넣을 글자를 모은다: 기본 문자 목록(KS X 1001 한글 2,350자 + 기호) + 폴더 안 스크립트·데이터 에셋에 쓰인 글자.
// YAML의 \uXXXX 표기도 풀어서 읽는다. Pretendard와 픽셀 폰트 빌더가 같이 쓴다.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VoxelMagic.EditorTools
{
    public static class FontCharsetCollector
    {
        private static readonly Regex Escaped = new Regex(@"\\u([0-9A-Fa-f]{4})");

        public static string Collect(string charsetPath, IEnumerable<string> textSourceFolders)
        {
            var chars = new SortedSet<char>(File.ReadAllText(charsetPath).Where(c => !char.IsControl(c)));
            for (char c = ' '; c <= '~'; c++) chars.Add(c); // ASCII는 항상 넣는다
            foreach (var folder in textSourceFolders)
            {
                if (!Directory.Exists(folder)) continue;
                foreach (var path in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                             .Where(p => p.EndsWith(".cs") || p.EndsWith(".asset")))
                {
                    var text = Escaped.Replace(File.ReadAllText(path),
                        m => ((char)System.Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
                    foreach (var c in text)
                        if (c > 0x7F && !char.IsControl(c) && !char.IsSurrogate(c)) chars.Add(c);
                }
            }
            return new StringBuilder().Append(chars.ToArray()).ToString();
        }
    }
}
