// 뒷산 채집 재료. 자라는 서식지, 나오는 시간대, 의뢰에 보일 서식지 힌트, 땅에 놓일 때의 기본 도형 모양을 데이터로 정한다.
using UnityEngine;

namespace VoxelMagic.Core
{
    public enum GatherHabitatKind
    {
        TreeShade,   // 큰 나무 밑 그늘
        Bush,        // 덤불
        RockCrevice, // 바위틈
        Clearing     // 초입 평지
    }

    [CreateAssetMenu(menuName = "VoxelMagic/Items/Gather Material", fileName = "Gather_")]
    public sealed class GatherMaterialDefinition : ItemDefinition, ISizedItem
    {
        [SerializeField] private GatherHabitatKind _habitat;
        [Tooltip("방과후에 나오는지")]
        [SerializeField] private bool _appearsAfterSchool = true;
        [Tooltip("밤에 나오는지")]
        [SerializeField] private bool _appearsAtNight;
        [Tooltip("의뢰에 보여 줄 서식지 힌트 한 줄 (정확한 위치는 쓰지 않는다)")]
        [SerializeField] private string _habitatHint;
        [SerializeField] private FloatRange _sizeRangeCm = new FloatRange(3f, 8f);

        [Header("땅에 놓인 모양 (임시 기본 도형)")]
        [SerializeField] private Mesh _worldMesh;
        [SerializeField] private Vector3 _worldScale = new Vector3(0.2f, 0.2f, 0.2f);

        public GatherHabitatKind Habitat => _habitat;
        public string HabitatHint => _habitatHint;
        public FloatRange SizeRangeCm => _sizeRangeCm;
        public Mesh WorldMesh => _worldMesh;
        public Vector3 WorldScale => _worldScale;

        public bool AppearsAt(TimeOfDay time) => time == TimeOfDay.Night ? _appearsAtNight : _appearsAfterSchool;
    }
}
