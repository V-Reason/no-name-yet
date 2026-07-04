using NUnit.Framework;
using RPG2D.Item;
using UnityEngine;

namespace RPG2D.Tests.Item
{
    public class ItemTests
    {
        [Test]
        public void TryPickupMagnet_WhenEmpty_SetsHasMagnetAndNotifies()
        {
            GameObject player = new GameObject("Player");
            PlayerItemHolder holder = player.AddComponent<PlayerItemHolder>();
            ConsumeInitialMagnet(player, holder);
            bool? notifiedValue = null;
            holder.OnMagnetChanged += hasMagnet => notifiedValue = hasMagnet;

            bool result = holder.TryPickupMagnet();

            Assert.IsTrue(result);
            Assert.IsTrue(holder.HasMagnet);
            Assert.AreEqual(true, notifiedValue);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TryPickupMagnet_WhenAlreadyHasMagnet_FailsAndKeepsState()
        {
            GameObject player = new GameObject("Player");
            PlayerItemHolder holder = player.AddComponent<PlayerItemHolder>();
            bool eventRaised = false;
            holder.OnMagnetChanged += _ => eventRaised = true;

            bool result = holder.TryPickupMagnet();

            Assert.IsFalse(result);
            Assert.IsTrue(holder.HasMagnet);
            Assert.IsFalse(eventRaised);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void HasMagnet_WhenHolderCreated_StartsWithOneMagnet()
        {
            GameObject player = new GameObject("Player");
            PlayerItemHolder holder = player.AddComponent<PlayerItemHolder>();

            Assert.IsTrue(holder.HasMagnet);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TryUseMagnet_WhenReady_SpawnsHeldPrefabAndConsumesMagnet()
        {
            GameObject player = new GameObject("Player");
            Transform holdPoint = new GameObject("ItemHoldPoint").transform;
            holdPoint.SetParent(player.transform);
            GameObject prefab = new GameObject("MagnetHeldPrefab");

            PlayerItemHolder holder = player.AddComponent<PlayerItemHolder>();
            holder.SetMagnetUseContext(holdPoint, prefab);
            bool? notifiedValue = null;
            holder.OnMagnetChanged += hasMagnet => notifiedValue = hasMagnet;

            bool result = holder.TryUseMagnet();

            Assert.IsTrue(result);
            Assert.IsFalse(holder.HasMagnet);
            Assert.AreEqual(false, notifiedValue);
            Assert.AreEqual(1, holdPoint.childCount);
            Assert.AreEqual("MagnetHeldPrefab(Clone)", holdPoint.GetChild(0).name);

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TryUseMagnet_WhenMissingPrefab_FailsWithoutConsuming()
        {
            GameObject player = new GameObject("Player");
            Transform holdPoint = new GameObject("ItemHoldPoint").transform;
            holdPoint.SetParent(player.transform);
            PlayerItemHolder holder = player.AddComponent<PlayerItemHolder>();
            holder.SetMagnetUseContext(holdPoint, null);

            bool result = holder.TryUseMagnet();

            Assert.IsFalse(result);
            Assert.IsTrue(holder.HasMagnet);
            Assert.AreEqual(0, holdPoint.childCount);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TryPickupFrom_WhenColliderBelongsToHolder_AddsMagnet()
        {
            GameObject player = new GameObject("Player");
            player.AddComponent<BoxCollider2D>();
            PlayerItemHolder holder = player.AddComponent<PlayerItemHolder>();
            ConsumeInitialMagnet(player, holder);
            MagnetPickupItem pickup = new GameObject("MagnetPickup").AddComponent<MagnetPickupItem>();

            bool result = pickup.TryPickupFrom(player.GetComponent<Collider2D>());

            Assert.IsTrue(result);
            Assert.IsTrue(holder.HasMagnet);
            Object.DestroyImmediate(pickup.gameObject);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TryPickupFrom_WhenColliderIsHeldItemChild_DoesNotPickupMagnet()
        {
            GameObject player = new GameObject("Player");
            PlayerItemHolder holder = player.AddComponent<PlayerItemHolder>();
            ConsumeInitialMagnet(player, holder);
            GameObject heldItem = new GameObject("MagnetHeld");
            heldItem.transform.SetParent(player.transform);
            Collider2D heldItemCollider = heldItem.AddComponent<CircleCollider2D>();
            MagnetPickupItem pickup = new GameObject("MagnetPickup").AddComponent<MagnetPickupItem>();

            bool result = pickup.TryPickupFrom(heldItemCollider);

            Assert.IsFalse(result);
            Assert.IsFalse(holder.HasMagnet);
            Object.DestroyImmediate(pickup.gameObject);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TickLifetime_WhenDurationReached_MarksHeldItemExpired()
        {
            MagnetHeldItem heldItem = new GameObject("MagnetHeld").AddComponent<MagnetHeldItem>();
            heldItem.SetDuration(2f);

            heldItem.TickLifetime(1f);
            Assert.IsFalse(heldItem.IsExpired);

            heldItem.TickLifetime(1f);
            Assert.IsTrue(heldItem.IsExpired);
        }

        private static void ConsumeInitialMagnet(GameObject player, PlayerItemHolder holder)
        {
            Transform holdPoint = new GameObject("ItemHoldPoint").transform;
            holdPoint.SetParent(player.transform);
            GameObject prefab = new GameObject("MagnetHeldPrefab");
            holder.SetMagnetUseContext(holdPoint, prefab);

            Assert.IsTrue(holder.TryUseMagnet());

            Object.DestroyImmediate(prefab);
            while (holdPoint.childCount > 0)
            {
                Object.DestroyImmediate(holdPoint.GetChild(0).gameObject);
            }
        }
    }
}
