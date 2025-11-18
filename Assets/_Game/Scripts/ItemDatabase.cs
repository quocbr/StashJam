using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemDefinition> items = new List<ItemDefinition>();

    private Dictionary<int, ItemDefinition> _lookup;

    public void BuildLookup()
    {
        _lookup = new Dictionary<int, ItemDefinition>();
        foreach (var def in items)
        {
            if (def == null) continue;
            if (!_lookup.ContainsKey(def.id))
            {
                _lookup.Add(def.id, def);
            }
        }
    }

    public ItemDefinition GetById(int id)
    {
        if (_lookup == null)
            BuildLookup();

        _lookup.TryGetValue(id, out var def);
        return def;
    }
}