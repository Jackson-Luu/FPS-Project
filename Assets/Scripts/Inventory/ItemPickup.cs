using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class ItemPickup : NetworkBehaviour
{
    private PlayerUI playerUI;

    public Item item;

    private HashSet<NetworkConnection> _observers = new HashSet<NetworkConnection>();
    private NetworkIdentity networkIdentity;

    private void Start()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<Player>().isLocalPlayer)
        {
            playerUI = other.gameObject.GetComponent<PlayerComponents>().playerUIInstance.GetComponent<PlayerUI>();
            playerUI.ItemPickupEnable(gameObject, item.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<Player>().isLocalPlayer)
        {
            playerUI = other.gameObject.GetComponent<PlayerComponents>().playerUIInstance.GetComponent<PlayerUI>();
            playerUI.ItemPickupDisable();
        }
    }

    public override bool OnCheckObserver(NetworkConnection conn)
    {
        return true;
    }

    // Rebuild observer list for this object on the network
    public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
    {
        foreach (NetworkConnection observer in _observers)
        {
            observers.Add(observer);
        }

        return true;
    }

    // Edit observers list
    public void EditObservers(NetworkConnection netConn, bool addObserver)
    {
        if (addObserver)
        {
            _observers.Add(netConn);
        } else
        {
            _observers.Remove(netConn);
        }
        networkIdentity.RebuildObservers(false);
    }

    public void Despawn()
    {
        NetworkServer.Destroy(gameObject);
    }
}
