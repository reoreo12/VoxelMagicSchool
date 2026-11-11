// 화면 왼쪽 아래(조작법 위)에 디버그 키 목록과 현재 시간대·마법진 강제 결과를 늘 띄운다.
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
            _context.Capture.TuftChanged += OnTuftChanged; // 강제 결과가 쓰이면 줄을 갱신
            Refresh();
        }

        private void OnDestroy()
        {
            if (_context == null) return;
            _context.Debug.Changed -= Refresh;
            _context.Time.Changed -= Refresh;
            _context.Capture.TuftChanged -= OnTuftChanged;
        }

        private void OnTuftChanged(int _) => Refresh();

        private void Refresh()
        {
            var sb = new StringBuilder();
            sb.AppendLine(MountainTexts.DebugHeader);
            foreach (var command in _context.Debug.Commands)
                sb.Append(command.Key).Append("  ").AppendLine(command.Label);
            sb.AppendFormat(MountainTexts.DebugTimeFormat, MountainTexts.TimeName(_context.Time.Time)).Append('\n');
            sb.AppendFormat(MountainTexts.DebugForcedFormat, MountainTexts.ForcedName(_context.Capture.Forced));
            _text.text = sb.ToString();
        }
    }
}
