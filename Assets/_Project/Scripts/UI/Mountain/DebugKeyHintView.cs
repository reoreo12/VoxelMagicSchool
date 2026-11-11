// 화면 왼쪽 아래(조작법 위)에 디버그 키 목록과 현재 시간대를 늘 띄운다.
using System.Text;
using TMPro;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class DebugKeyHintView : MonoBehaviour, IMountainContextBinder
    {
        [SerializeField] private TMP_Text _text;

        private MountainContext _context;

        public void Bind(MountainContext context)
        {
            _context = context;
            _context.Debug.Changed += Refresh;
            _context.Time.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_context == null) return;
            _context.Debug.Changed -= Refresh;
            _context.Time.Changed -= Refresh;
        }

        private void Refresh()
        {
            var sb = new StringBuilder();
            sb.AppendLine(MountainTexts.DebugHeader);
            foreach (var command in _context.Debug.Commands)
                sb.Append(command.Key).Append("  ").AppendLine(command.Label);
            sb.AppendFormat(MountainTexts.DebugTimeFormat, MountainTexts.TimeName(_context.Time.Time));
            _text.text = sb.ToString();
        }
    }
}
