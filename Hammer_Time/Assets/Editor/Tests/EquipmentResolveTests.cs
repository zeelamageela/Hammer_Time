using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Which item is equipped in each slot after the arena reloads. Regression: after a purchase (which saves, which clears
/// CareerManager.loadedFromSave) the arena equipped inventory[0..3] - the first four owned items across ALL categories -
/// instead of the saved ids.
/// </summary>
public class EquipmentResolveTests
{
    GameObject go;
    EquipmentManager em;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("EquipResolveTest");
        em = go.AddComponent<EquipmentManager>();
        em.handles = Make(0, true);
        em.heads = Make(30, true);
        em.footwear = Make(60, true);
        em.apparel = Make(90, true);
    }

    [TearDown]
    public void TearDown() { Object.DestroyImmediate(go); }

    static Equipment[] Make(int firstId, bool handle)
    {
        var a = new Equipment[30];
        for (int i = 0; i < a.Length; i++) a[i] = new Equipment { id = firstId + i, cost = 100 + i };
        a[0].owned = true;   // starter item
        return a;
    }

    static int[] Ids(Equipment[] e) => new[] { e[0].id, e[1].id, e[2].id, e[3].id };

    [Test]
    public void BoughtSecondHandle_StaysEquipped_AndDoesNotLeakIntoTheHeadSlot()
    {
        em.handles[7].owned = true;   // bought handle 7 -> inventory now holds two handles before any head

        var r = em.ResolveActiveEquip(new[] { 7, 30, 60, 90 });

        CollectionAssert.AreEqual(new[] { 7, 30, 60, 90 }, Ids(r));
        Assert.IsTrue(r[0].active);
        Assert.IsFalse(em.handles[0].active, "the old handle must be unequipped");
    }

    [Test]
    public void BoughtItemsInEverySlot_AllStayEquipped()
    {
        em.handles[3].owned = em.heads[5].owned = em.footwear[9].owned = em.apparel[11].owned = true;

        var r = em.ResolveActiveEquip(new[] { 3, 35, 69, 101 });

        CollectionAssert.AreEqual(new[] { 3, 35, 69, 101 }, Ids(r));
    }

    [Test]
    public void MissingOrInvalidIds_FallBackToTheSameCategoriesStarterItem()
    {
        var r = em.ResolveActiveEquip(null);
        CollectionAssert.AreEqual(new[] { 0, 30, 60, 90 }, Ids(r));

        r = em.ResolveActiveEquip(new[] { 999, -1, 60 });   // unknown, missing, valid, absent
        CollectionAssert.AreEqual(new[] { 0, 30, 60, 90 }, Ids(r));
    }

    [Test]
    public void EquippedIdThatIsNotOwned_IsNotEquipped()
    {
        var r = em.ResolveActiveEquip(new[] { 12, 30, 60, 90 });   // handle 12 exists but was never bought
        Assert.AreEqual(0, r[0].id);
    }
}
