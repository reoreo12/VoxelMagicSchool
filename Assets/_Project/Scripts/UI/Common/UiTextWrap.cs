// 한글 문장을 낱말(띄어쓰기) 단위로 줄바꿈한다. TMP 기본 설정은 한글을 글자마다 끊어서 "상식/이야"처럼 낱말 중간이 잘리기 때문.
// 프로젝트 TMP 설정을 바꾸지 않고, 바뀔 때만 한 번 계산해 '\n'을 넣은 문자열을 돌려준다(한 낱말이 폭보다 길면 그 낱말만 글자 단위로 끊는다).
using System.Text;
using TMPro;

namespace VoxelMagic.UI
{
    public static class UiTextWrap
    {
        private static readonly StringBuilder Result = new StringBuilder();
        private static readonly StringBuilder Line = new StringBuilder();

        public static string ByWords(TMP_Text label, string text, float width)
        {
            if (string.IsNullOrEmpty(text) || label == null || width <= 0f) return text;
            Result.Clear();
            Line.Clear();
            foreach (var word in text.Split(' '))
            {
                string candidate = Line.Length == 0 ? word : Line + " " + word;
                if (Line.Length > 0 && label.GetPreferredValues(candidate).x > width)
                {
                    Flush();
                    Line.Append(word);
                }
                else
                {
                    Line.Clear();
                    Line.Append(candidate);
                }
            }
            Flush();
            return Result.ToString();
        }

        private static void Flush()
        {
            if (Line.Length == 0) return;
            if (Result.Length > 0) Result.Append('\n');
            Result.Append(Line);
            Line.Clear();
        }
    }
}
