// 의뢰 게시판의 의뢰 한 줄. 이름·크리처의 한마디·필요 재료(+서식지 힌트)·필기 점수와 수락 버튼(못 받는 이유 포함)을 보여 준다.
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;

namespace VoxelMagic.UI
{
    public sealed class RequestEntryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _creatureLine;
        [SerializeField] private TMP_Text _needs;
        [SerializeField] private TMP_Text _reward;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private TMP_Text _acceptLabel;

        public RequestDefinition Request { get; private set; }

        public void Setup(RequestDefinition request, int reward, Action<RequestDefinition> onAccept)
        {
            Request = request;
            _title.text = request.Title;
            _creatureLine.text = $"\"{request.CreatureLine}\"";
            _needs.text = string.Format(GameTexts.NeedsFormat,
                string.Join(", ", request.Requirements.Where(r => r.Item != null).Select(r => $"{r.Item.DisplayName} {r.Amount}")));
            // 서식지 힌트 한 줄을 같은 줄 끝에 작게 (정확한 위치는 알려 주지 않는다). 의뢰에 힌트가 없으면 채집 재료의 힌트로.
            var hints = string.IsNullOrEmpty(request.HabitatHint)
                ? request.Requirements.Select(r => r.Item).OfType<GatherMaterialDefinition>()
                    .Select(m => m.HabitatHint).Where(h => !string.IsNullOrEmpty(h)).Distinct().ToList()
                : new System.Collections.Generic.List<string> { request.HabitatHint };
            if (hints.Count > 0)
                _needs.text += $"  <size=80%><color=#B8C4A8>· {string.Join(" / ", hints)}</color></size>";
            _reward.text = string.Format(GameTexts.RewardFormat, reward);
            _acceptButton.onClick.RemoveAllListeners();
            _acceptButton.onClick.AddListener(() => onAccept(Request));
        }

        public void Refresh(AcceptResult canAccept, int maxAccepted)
        {
            _acceptButton.interactable = canAccept == AcceptResult.Success;
            _acceptLabel.text = canAccept == AcceptResult.AlreadyAccepted ? GameTexts.Accepted : GameTexts.Accept;
            _status.text = canAccept == AcceptResult.SlotsFull ? GameTexts.RequestSlotsFullShort : Reason(canAccept, maxAccepted);
        }

        public static string Reason(AcceptResult result, int maxAccepted) => result switch
        {
            AcceptResult.SlotsFull => string.Format(GameTexts.RequestSlotsFullFormat, maxAccepted),
            AcceptResult.InventoryFull => GameTexts.RequestNoRoomForSeeds,
            AcceptResult.NotNight => GameTexts.RequestNotNight,
            AcceptResult.BoardClosed => GameTexts.RequestBoardClosed,
            AcceptResult.NotOnBoard => GameTexts.RequestNotOnBoard,
            _ => string.Empty
        };
    }
}
