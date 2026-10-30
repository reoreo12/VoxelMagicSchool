// 의뢰 게시판의 의뢰 한 줄. 이름·크리처의 한마디·필요 아이템·보상과 수락 버튼(불가 이유 포함)을 보여 준다.
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

        public void Setup(RequestDefinition request, Action<RequestDefinition> onAccept)
        {
            Request = request;
            _title.text = request.Title;
            _creatureLine.text = $"\"{request.CreatureLine}\"";
            _needs.text = string.Format(GameTexts.NeedsFormat,
                string.Join(", ", request.Requirements.Select(r => $"{r.Item.DisplayName} {r.Amount}")));
            _reward.text = string.Format(GameTexts.RewardFormat, request.BaseReward);
            _acceptButton.onClick.AddListener(() => onAccept(Request));
        }

        public void Refresh(RequestStatus status, AcceptResult canAccept)
        {
            bool available = status == RequestStatus.Available && canAccept == AcceptResult.Success;
            _acceptButton.interactable = available;
            _acceptLabel.text = status == RequestStatus.Active ? GameTexts.Accepted
                : status == RequestStatus.Completed ? GameTexts.RequestCompleted
                : GameTexts.Accept;
            _status.text = status != RequestStatus.Available ? string.Empty : canAccept switch
            {
                AcceptResult.AlreadyActive => GameTexts.RequestAlreadyActive,
                AcceptResult.InventoryFull => GameTexts.RequestNoRoomForSeeds,
                _ => string.Empty
            };
        }
    }
}
