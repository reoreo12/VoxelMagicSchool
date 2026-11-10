// 씬 이동 연출 설정(픽셀 계단 원형 페이드의 시간·블록 크기·디더 폭·색).
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Config/Scene Transition", fileName = "SceneTransitionConfig")]
    public sealed class SceneTransitionConfig : ScriptableObject
    {
        [Tooltip("화면 가장자리부터 원이 좁아져 다 가릴 때까지 (초)")]
        [SerializeField, Min(0.05f)] private float _fadeOutDuration = 0.4f;
        [Tooltip("다 가린 상태로 잠깐 멈추는 시간 (초, 로딩 시간과 별개)")]
        [SerializeField, Min(0f)] private float _holdDuration = 0.05f;
        [Tooltip("새 씬에서 원이 넓어지며 다 보일 때까지 (초)")]
        [SerializeField, Min(0.05f)] private float _fadeInDuration = 0.4f;
        [Tooltip("픽셀 블록 한 칸 크기 (화면 px, 1080p 기준 — 해상도에 맞춰 늘고 줆)")]
        [SerializeField, Min(1f)] private float _blockSize = 12f;
        [Tooltip("원 테두리의 디더(점점 가려짐) 폭 (블록 수)")]
        [SerializeField, Min(0f)] private float _ditherBlocks = 3f;
        [SerializeField] private Color _color = Color.black;
        [Tooltip("원 중심이 플레이어를 따라감 (끄면 화면 가운데)")]
        [SerializeField] private bool _centerOnPlayer = true;
        [Tooltip("UI 정렬 순서 (게임 UI보다 위, 환경설정 UI보다 아래로 두면 됨)")]
        [SerializeField] private int _sortingOrder = 500;

        public float FadeOutDuration => _fadeOutDuration;
        public float HoldDuration => _holdDuration;
        public float FadeInDuration => _fadeInDuration;
        public float BlockSize => _blockSize;
        public float DitherBlocks => _ditherBlocks;
        public Color Color => _color;
        public bool CenterOnPlayer => _centerOnPlayer;
        public int SortingOrder => _sortingOrder;
    }
}
