using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MaieBlazorLib;
using MaieBlazorLib.LocalTierLister;

namespace MaieBlazorLib.LocalTierLister
{
    public static class TLMerger
    {
        public static TierList MergeTierLists(MergeBehaviour behaviour, TierList current, TierList incoming)
        {
            return behaviour.Merge(current, incoming);
        }
    }
}

public class ItemPosition
{
    public TierItem currentItem;
    public TierItem incomingItem;
    public int finalindex;
}

public abstract class MergeBehaviour
{
    /// <summary>
    /// The main method used, shouldn't be changed.
    /// </summary>
    public TierList Merge(TierList current, TierList incoming)
    {
        var res = new TierList(MergeName(current, incoming));
        var itemTable = GetItemTable(current.tiers, incoming.tiers);
        res.tiers = RebuildTiers(itemTable, current.tiers, incoming.tiers);
        return res;
    }

    /// <summary>
    /// Decide which name to use
    /// </summary>
    public abstract string MergeName(TierList current, TierList incoming);
    /// <summary>
    /// Build an item table with merged items (tiers kept track of by the Parent attribute)
    /// </summary>
    public abstract Dictionary<string, ItemPosition> GetItemTable(Dictionary<string, Tier> currentTiers, Dictionary<string, Tier> incomingTiers);
    /// <summary>
    /// Merges the tiers AND attributes the items in them.
    /// </summary>
    public abstract Dictionary<string, Tier> RebuildTiers(Dictionary<string, ItemPosition> itemTable, Dictionary<string, Tier> currentTiers, Dictionary<string, Tier> incomingTiers);
}

public static class MergeBehaviourTable
{
    public static MergeBehaviour MergeIntoCurrentShallow => MB_MergeIntoCurrentShallow.Instance;
    public static MergeBehaviour MergeIntoCurrent => MB_MergeIntoCurrent.Instance;
}

internal class MB_MergeIntoCurrentShallow : MergeBehaviour
{
    public static readonly MB_MergeIntoCurrentShallow Instance = new();

    public override string MergeName(TierList current, TierList incoming)
    {
        return incoming.name;
    }
    public override Dictionary<string, ItemPosition> GetItemTable(Dictionary<string, Tier> currentTiers, Dictionary<string, Tier> incomingTiers)
    {
        var itemTable = new Dictionary<string, ItemPosition>();

        foreach (var tier in currentTiers.Values)
        {
            for (int i = 0; i < tier.items.Count; i++)
            {
                itemTable[tier.items[i].name] = new ItemPosition { currentItem = tier.items[i], finalindex = i };
            }
        }

        foreach (var tier in incomingTiers.Values)
        {
            for (int i = 0; i < tier.items.Count; i++)
            {
                itemTable[tier.items[i].name] = new ItemPosition { currentItem = tier.items[i], finalindex = i };
            }
        }

        return itemTable;
    }

    public override Dictionary<string, Tier> RebuildTiers(Dictionary<string, ItemPosition> itemTable, Dictionary<string, Tier> currentTiers, Dictionary<string, Tier> incomingTiers)
    {
        var res = new Dictionary<string, Tier>();

        foreach(var tier in currentTiers.Values)
        {
            res[tier.name] = tier;
        }

        foreach (var tier in incomingTiers.Values)
        {
            res[tier.name] = tier;
            res[tier.name].items.Clear();
        }

        foreach (var item in itemTable.Values)
        {
            if (item.currentItem.parent != null && res.ContainsKey(item.currentItem.parent.name))
            {
                item.currentItem.parent = res[item.currentItem.parent.name];
                res[item.currentItem.parent.name].items.Add(item.currentItem);
            }
            else throw new KeyNotFoundException($"Parent tier of item {item.currentItem.name} was not found within the merged tiers ({(item.currentItem.parent != null ? item.currentItem.parent.name : "No parent registered")})");
        }

        foreach (var tier in res.Values)
        {
            var incomingOrder = tier.items
                .Where(i => itemTable[i.name].currentItem.parent.name == tier.name && incomingTiers.ContainsKey(tier.name) && incomingTiers[tier.name].items.Contains(i))
                .OrderBy(i => itemTable[i.name].finalindex)
                .ToList();

            var currentOnly = tier.items
                .Except(incomingOrder)
                .ToList();

            tier.items = incomingOrder.Concat(currentOnly).ToList();
        }

        return res;
    }
}

internal class MB_MergeIntoCurrent : MergeBehaviour
{
    public static readonly MB_MergeIntoCurrent Instance = new();

    public override string MergeName(TierList current, TierList incoming)
    {
        return incoming.name;
    }
    public override Dictionary<string, ItemPosition> GetItemTable(Dictionary<string, Tier> currentTiers, Dictionary<string, Tier> incomingTiers)
    {
        var itemTable = new Dictionary<string, ItemPosition>();

        foreach (var tier in currentTiers.Values)
        {
            for (int i = 0; i < tier.items.Count; i++)
            {
                var item = tier.items[i];

                if (!itemTable.TryGetValue(item.name, out var pos))
                {
                    pos = new ItemPosition { finalindex = i };
                    itemTable[item.name] = pos;
                }

                pos.finalindex = i;
                pos.currentItem = item;
            }
        }

        foreach (var tier in incomingTiers.Values)
        {
            for (int i = 0; i < tier.items.Count; i++)
            {
                var item = tier.items[i];

                if (!itemTable.TryGetValue(item.name, out var pos))
                {
                    pos = new ItemPosition { finalindex = i };
                    itemTable[item.name] = pos;
                }
                
                pos.finalindex = i;
                pos.incomingItem = item;
            }
        }

        return itemTable;
    }

    public override Dictionary<string, Tier> RebuildTiers(Dictionary<string, ItemPosition> itemTable, Dictionary<string, Tier> currentTiers, Dictionary<string, Tier> incomingTiers)
    {
        var res = new Dictionary<string, Tier>();

        foreach (var tier in currentTiers.Values)
        {
            res[tier.name] = tier;
        }

        foreach (var tier in incomingTiers.Values)
        {
            res[tier.name] = tier;
            res[tier.name].items.Clear();
        }

        foreach (var item in itemTable.Values)
        {
            var mergedItem = DeepMerge(item.currentItem, item.incomingItem);
            if (mergedItem == null) throw new NullReferenceException("Malformed item merge: ItemPosition with no current nor incoming.");

            var parentName = mergedItem.parent?.name;

            if (parentName == null || !res.ContainsKey(parentName))
                throw new KeyNotFoundException( $"Parent tier of item {mergedItem.name} was not found within merged tiers ({parentName ?? "No parent"})");

            mergedItem.parent = res[parentName];
            res[parentName].items.Add(mergedItem);
        }

        foreach (var tier in res.Values)
        {
            var incomingOrder = tier.items
                .Where(i =>
                {
                    var pos = itemTable[i.name];

                    var item =
                        pos.incomingItem != null ? pos.incomingItem :
                        pos.currentItem;

                    return item?.parent?.name == tier.name && pos.incomingItem != null;
                })
    .OrderBy(i => itemTable[i.name].finalindex)
    .ToList();

            var incomingSet = incomingOrder
    .Select(i => i.name)
    .ToHashSet();

            var currentOnly = tier.items
                .Where(i => !incomingSet.Contains(i.name))
                .ToList();

            tier.items = incomingOrder.Concat(currentOnly).ToList();
        }

        return res;
    }

    internal TierItem DeepMerge(TierItem current, TierItem incoming)
    {
        if (current == null && incoming == null) throw new NullReferenceException("No items found for the merge");
        if (current == null) return incoming;
        if (incoming == null) return current;

        var res = new TierItem();

        res.name = incoming.name;
        res.imgMime = String.IsNullOrEmpty(incoming.imgMime) ? current.imgMime : incoming.imgMime;
        res.imgLocal = String.IsNullOrEmpty(incoming.imgLocal) ? current.imgLocal : incoming.imgLocal;
        res.img = String.IsNullOrEmpty(incoming.img) ? current.img : incoming.img;
        res.notes = String.IsNullOrEmpty(incoming.notes) ? current.notes : incoming.notes;
        res.tags = incoming.tags.Length <= 0 ? current.tags : MergeCategories(current.tags, incoming.tags);
        res.parent = incoming.parent != null ? incoming.parent : current.parent;

        return res;
    }

    internal string[] MergeCategories(string[] a, string[] b)
    {
        var set = new HashSet<string>(a ?? []);
        if (b != null)
            foreach (var c in b)
                set.Add(c);

        return set.ToArray();
    }
}