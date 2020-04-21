using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Inventory : NetworkBehaviour
{
    public int space = 15;

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<Item> items = new List<Item>();

    public EquipmentManager equipmentManager;
    public Player player;

    private void Start()
    {
        if (equipmentManager == null)
        {
            equipmentManager = GetComponent<EquipmentManager>();
        }
        if (player == null)
        {
            player = GetComponent<Player>();
        }
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

    public void UseItem(int slot)
    {
        if (items[slot] != null)
        {
            CmdUseItem(slot);
            Remove(items[slot]);
        }
    }

    [Command]
    void CmdUseItem(int slot)
    {
        if (items[slot] != null)
        {
            items[slot].Use(player);
            Remove(items[slot]);
        }
    }

    public void equipItem(int slot)
    {
        CmdEquipItem(slot);
    }

    [Command]
    void CmdEquipItem(int slot)
    {
        if (items[slot] is Equipment)
        {
            equipmentManager.Equip((Equipment)items[slot]);
            RpcEquipItem(slot);
            Remove(items[slot]);
        }
    }

    [ClientRpc]
    void RpcEquipItem(int slot)
    {
        equipmentManager.Equip((Equipment)items[slot]);
        Remove(items[slot]);
    }

    public void UnEquipItem(int slot)
    {
        CmdUnEquipItem(slot);
    }

    [Command]
    void CmdUnEquipItem(int slot)
    {
        if (equipmentManager.GetEquipment((EquipmentSlot)slot) != null)
        {
            equipmentManager.UnEquip((EquipmentSlot)slot);
            RpcUnEquipItem(slot);
        }
    }

    [ClientRpc]
    void RpcUnEquipItem(int slot)
    {
        equipmentManager.UnEquip((EquipmentSlot)slot);
    }
}
