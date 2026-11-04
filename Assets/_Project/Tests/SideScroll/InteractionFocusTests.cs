// 복도 상호작용 대상(안내 문구) 상태 단위 테스트.
using NUnit.Framework;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class InteractionFocusTests
    {
        [Test]
        public void Set_NotifiesOnlyWhenPromptChanges()
        {
            var state = new InteractionFocusState();
            int changes = 0;
            state.Changed += () => changes++;
            Assert.IsFalse(state.HasTarget);

            var prompt = InteractionPrompt.Available(SideScrollTexts.PortalToFarm, Vector3.up);
            state.Set(prompt);
            state.Set(prompt);
            Assert.IsTrue(state.HasTarget);
            Assert.AreEqual(1, changes, "같은 문구로는 알리지 않음");

            state.Clear();
            state.Clear();
            Assert.IsFalse(state.HasTarget);
            Assert.AreEqual(2, changes);
        }
    }
}
