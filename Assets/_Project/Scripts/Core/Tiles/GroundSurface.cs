// 바닥 종류 하나(풀·흙·진흙·짚·돌·나무판…). 나중에 발밑 파티클·발소리 등을 여기에 추가한다.
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Tiles/Ground Surface", fileName = "Surface_")]
    public sealed class GroundSurface : ScriptableObject
    {
        [Tooltip("화면·디버그에 보일 이름")]
        [SerializeField] private string _displayName;

        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;

        public void SetDisplayName(string displayName) => _displayName = displayName;
    }
}
