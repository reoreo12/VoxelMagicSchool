// 캐릭터 장착 상태·소켓 프로필 단위 테스트: 장착·교체·해제 이벤트, 소켓 없는 정의 거절, 프로필 조회.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class CharacterEquipmentTests
    {
        private SocketId _rightHand;
        private SocketId _back;
        private List<(SocketId socket, AttachmentDefinition previous, AttachmentDefinition now)> _events;
        private EquipmentState _state;

        [SetUp]
        public void SetUp()
        {
            _rightHand = TestAssets.Create<SocketId>();
            _back = TestAssets.Create<SocketId>();
            _state = new EquipmentState();
            _events = new List<(SocketId, AttachmentDefinition, AttachmentDefinition)>();
            _state.Changed += (socket, previous, now) => _events.Add((socket, previous, now));
        }

        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        private AttachmentDefinition Attachment(SocketId socket) =>
            TestAssets.Create<AttachmentDefinition>(("_socket", socket));

        [Test]
        public void Get_WhenEmpty_ReturnsNull()
        {
            Assert.IsNull(_state.Get(_rightHand));
            Assert.IsNull(_state.Get(null));
        }

        [Test]
        public void Equip_RaisesChangedOnce()
        {
            var wand = Attachment(_rightHand);

            Assert.IsTrue(_state.Equip(wand));

            Assert.AreSame(wand, _state.Get(_rightHand));
            Assert.AreEqual(1, _events.Count);
            Assert.AreEqual((_rightHand, (AttachmentDefinition)null, wand), _events[0]);
        }

        [Test]
        public void Equip_SameSocket_ReplacesPrevious()
        {
            var wand = Attachment(_rightHand);
            var torch = Attachment(_rightHand);
            _state.Equip(wand);

            Assert.IsTrue(_state.Equip(torch));

            Assert.AreSame(torch, _state.Get(_rightHand));
            Assert.AreEqual((_rightHand, wand, torch), _events[1]);
        }

        [Test]
        public void Equip_SameAttachmentAgain_DoesNothing()
        {
            var wand = Attachment(_rightHand);
            _state.Equip(wand);

            Assert.IsFalse(_state.Equip(wand));
            Assert.AreEqual(1, _events.Count);
        }

        [Test]
        public void Equip_DifferentSockets_KeepsBoth()
        {
            var wand = Attachment(_rightHand);
            var bag = Attachment(_back);

            _state.Equip(wand);
            _state.Equip(bag);

            Assert.AreSame(wand, _state.Get(_rightHand));
            Assert.AreSame(bag, _state.Get(_back));
        }

        [Test]
        public void Equip_WithoutSocketOrNull_IsRejected()
        {
            Assert.IsFalse(_state.Equip(Attachment(null)));
            Assert.IsFalse(_state.Equip(null));
            Assert.AreEqual(0, _events.Count);
        }

        [Test]
        public void Unequip_RaisesChangedWithNull()
        {
            var wand = Attachment(_rightHand);
            _state.Equip(wand);

            Assert.IsTrue(_state.Unequip(_rightHand));

            Assert.IsNull(_state.Get(_rightHand));
            Assert.AreEqual((_rightHand, wand, (AttachmentDefinition)null), _events[1]);
        }

        [Test]
        public void Unequip_EmptySocket_DoesNothing()
        {
            Assert.IsFalse(_state.Unequip(_rightHand));
            Assert.AreEqual(0, _events.Count);
        }

        [Test]
        public void SocketProfile_TryGet_FindsOnlyListedSockets()
        {
            var binding = new SocketBinding(_rightHand, HumanBodyBones.RightHand, Vector3.zero, Vector3.zero);
            var profile = TestAssets.Create<SocketProfile>(("_bindings", new List<SocketBinding> { binding }));

            Assert.IsTrue(profile.TryGet(_rightHand, out var found));
            Assert.AreSame(binding, found);
            Assert.IsFalse(profile.TryGet(_back, out _));
            Assert.IsFalse(profile.TryGet(null, out _));
        }
    }
}
