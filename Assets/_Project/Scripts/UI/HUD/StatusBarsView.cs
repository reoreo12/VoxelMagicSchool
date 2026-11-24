// 화면 오른쪽 아래 기력(연두)·마력(보라) 세로 바 묶음(UI_SPEC 7-2, 13장: 오른쪽·아래 여백 같게, 바 사이 < 가장자리 여백).
// 값은 Core의 IPlayerVitals로만 읽는다. 아직 마력·기력 규칙 모델이 연결되지 않았으면 가득 찬 상태로 보여 준다.
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class StatusBarsView : ThemedWidget, IGameContextBinder
    {
        [SerializeField] private StatusBarView _mana;
        [SerializeField] private StatusBarView _stamina;

        private IPlayerVitals _vitals;

        public void Bind(GameContext context)
        {
            _vitals = context.Vitals;
            if (_vitals != null) _vitals.VitalsChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_vitals != null) _vitals.VitalsChanged -= Refresh;
        }

        private void Refresh()
        {
            if (Theme == null || _mana == null || _stamina == null) return;
            int inset = Theme.StatusBars.FillInset;
            _mana.SetValue(_vitals != null ? _vitals.ManaRatio : 1f, inset);
            _stamina.SetValue(_vitals != null ? _vitals.StaminaRatio : 1f, inset);
        }

        protected override void Apply(UiTheme theme)
        {
            var values = theme.StatusBars;
            if (_mana == null || _stamina == null) return;
            _mana.Layout(values, values.ManaFill, values.ManaCap, values.ManaIcon);
            _stamina.Layout(values, values.StaminaFill, values.StaminaCap, values.StaminaIcon);

            // 기력 → 마력 순으로 왼쪽부터(사용자 결정). 프레임 사이 간격은 BarGap, 크기는 프레임 기준(아이콘은 삐져나와도 됨).
            var staminaSize = Vector2Int.RoundToInt(_stamina.Rect.sizeDelta);
            var manaSize = Vector2Int.RoundToInt(_mana.Rect.sizeDelta);
            int width = staminaSize.x + values.BarGap + manaSize.x;
            int height = Mathf.Max(manaSize.y, staminaSize.y);

            var root = (RectTransform)transform;
            // 여백은 확대 뒤 화면 기준 값이라 묶음의 오른쪽 아래 기준점에 그대로 둔다. 묶음 전체를 배수만큼 키운다.
            PixelLayout.PlaceBottomRight(root, values.Margin, values.Margin, width, height);
            root.localScale = new Vector3(values.Scale, values.Scale, 1f);
            PixelLayout.PlaceBottomLeft(_stamina.Rect, 0, 0, staminaSize.x, staminaSize.y);
            PixelLayout.PlaceBottomLeft(_mana.Rect, staminaSize.x + values.BarGap, 0, manaSize.x, manaSize.y);
            Refresh();
        }
    }
}
