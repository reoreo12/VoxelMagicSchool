// 인벤토리 단위 테스트: 겹쳐 쌓기, 가득 참, 제거, 선택.
using NUnit.Framework;
using VoxelMagic.Core;

namespace VoxelMagic.Tests
{
    public class InventoryTests
    {
        [TearDown] public void TearDown() => TestAssets.DestroyAll();

        [Test]
        public void TryAdd_SameItem_StacksIntoOneSlot()
        {
            var radish = TestAssets.Produce("radish");
            var inventory = new Inventory(5);

            Assert.IsTrue(inventory.TryAdd(radish, 2));
            Assert.IsTrue(inventory.TryAdd(radish, 3));

            Assert.AreEqual(5, inventory.GetSlot(0).Count);
            Assert.IsTrue(inventory.GetSlot(1).IsEmpty);
        }

        [Test]
        public void TryAdd_OverMaxStack_SpillsIntoNextSlot()
        {
            var radish = TestAssets.Produce("radish", maxStack: 3);
            var inventory = new Inventory(5);

            Assert.IsTrue(inventory.TryAdd(radish, 5));

            Assert.AreEqual(3, inventory.GetSlot(0).Count);
            Assert.AreEqual(2, inventory.GetSlot(1).Count);
        }

        [Test]
        public void TryAdd_WhenFull_FailsAndChangesNothing()
        {
            var inventory = new Inventory(2);
            inventory.TryAdd(TestAssets.Produce("a"), 1);
            inventory.TryAdd(TestAssets.Produce("b"), 1);
            int changes = 0;
            inventory.SlotChanged += _ => changes++;

            Assert.IsFalse(inventory.TryAdd(TestAssets.Produce("c"), 1));
            Assert.AreEqual(0, changes);
        }

        [Test]
        public void TryRemove_NotEnough_FailsAndKeepsCount()
        {
            var radish = TestAssets.Produce("radish");
            var inventory = new Inventory(5);
            inventory.TryAdd(radish, 2);

            Assert.IsFalse(inventory.TryRemove(radish, 3));
            Assert.AreEqual(2, inventory.Count(radish));
        }

        [Test]
        public void TryRemove_All_EmptiesSlot()
        {
            var radish = TestAssets.Produce("radish");
            var inventory = new Inventory(5);
            inventory.TryAdd(radish, 2);

            Assert.IsTrue(inventory.TryRemove(radish, 2));
            Assert.IsTrue(inventory.GetSlot(0).IsEmpty);
        }

        [Test]
        public void CanAddAll_ChecksCombinedSpace()
        {
            var inventory = new Inventory(2);
            inventory.TryAdd(TestAssets.Produce("a"), 1);
            var needs = new[]
            {
                new ItemAmount(TestAssets.Produce("b"), 1),
                new ItemAmount(TestAssets.Produce("c"), 1)
            };

            Assert.IsFalse(inventory.CanAddAll(needs));
            Assert.IsTrue(inventory.CanAddAll(new[] { needs[0] }));
        }

        [Test]
        public void Select_ChangesSelectionAndIgnoresOutOfRange()
        {
            var inventory = new Inventory(5);
            int raised = -1;
            inventory.SelectionChanged += index => raised = index;

            inventory.Select(3);
            inventory.Select(9);

            Assert.AreEqual(3, inventory.SelectedIndex);
            Assert.AreEqual(3, raised);
        }
    }
}
