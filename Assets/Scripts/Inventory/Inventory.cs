using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Inventory : NetworkBehaviour
{
    public int space = 20;

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<Item> items = new List<Item>();

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
}
