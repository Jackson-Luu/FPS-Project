using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Inventory : NetworkBehaviour
{
    public int space = 20;

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<Item> items = new List<Item>();

    public EquipmentManager equipmentManager;
    public Player player;

    private void Start()
    {
        equipmentManager = GetComponent<EquipmentManager>();
        player = GetComponent<Player>();
    }

    public bool Add (Item item)
    {
        if (items.Count >= space)
        {
            return false;
        }

        items.Add(item);
        if (onItemChangedCallback != null) { onItemChangedCallback.Invoke(); }

        return true;
    }
    
    public void Remove (Item item)
    {
        items.Remove(item);

        if (onItemChangedCallback != null) { onItemChangedCallback.Invoke(); }
    }

    public void UseItem(Item item)
    {
        item.Use(player);
        Remove(item);
    }
}
