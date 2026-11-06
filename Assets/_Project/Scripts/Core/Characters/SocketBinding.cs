// 소켓 하나의 위치 규칙: 어느 본(사람형 본 또는 모델 안의 이름)에 붙고, 그 본 기준으로 얼마나 옮기고 돌리는지.
using System;
using UnityEngine;

namespace VoxelMagic.Core
{
    [Serializable]
    public sealed class SocketBinding
    {
        [SerializeField] private SocketId _socket;
        [Tooltip("켜면 Humanoid 본으로, 끄면 모델 안의 Transform 이름으로 찾는다")]
        [SerializeField] private bool _useHumanBone = true;
        [SerializeField] private HumanBodyBones _humanBone = HumanBodyBones.RightHand;
        [SerializeField] private string _transformName;
        [Tooltip("본 기준 소켓 위치 (아바타마다 손 크기·손바닥 중심이 달라서)")]
        [SerializeField] private Vector3 _positionOffset;
        [Tooltip("본 기준 소켓 회전 (오일러 각)")]
        [SerializeField] private Vector3 _rotationOffset;

        public SocketBinding() { }

        public SocketBinding(SocketId socket, HumanBodyBones humanBone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            _socket = socket;
            _useHumanBone = true;
            _humanBone = humanBone;
            _positionOffset = positionOffset;
            _rotationOffset = rotationOffset;
        }

        public SocketBinding(SocketId socket, string transformName, Vector3 positionOffset, Vector3 rotationOffset)
        {
            _socket = socket;
            _useHumanBone = false;
            _transformName = transformName;
            _positionOffset = positionOffset;
            _rotationOffset = rotationOffset;
        }

        public SocketId Socket => _socket;
        public bool UsesHumanBone => _useHumanBone;
        public HumanBodyBones HumanBone => _humanBone;
        public string TransformName => _transformName;
        public Vector3 PositionOffset => _positionOffset;
        public Quaternion RotationOffset => Quaternion.Euler(_rotationOffset);
    }
}
