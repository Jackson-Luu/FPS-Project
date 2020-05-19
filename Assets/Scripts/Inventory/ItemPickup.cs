using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class ItemPickup : NetworkBehaviour
{
    private PlayerUI playerUI;

    public Item item;

    private HashSet<NetworkConnection> _observers = new HashSet<NetworkConnection>();
    private NetworkIdentity networkIdentity;

    private void Awake()
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
    [Server]
    public void EditObservers(NetworkConnection netConn, bool addObserver)
    {
        if (addObserver && !_observers.Contains(netConn))
        {
            _observers.Add(netConn);
            networkIdentity.RebuildObservers(false);
            TargetSyncObserver(netConn, gameObject.transform.position, gameObject.transform.rotation);
        } else
        {
            _observers.Remove(netConn);
            networkIdentity.RebuildObservers(false);
        }
    }

    [TargetRpc]
    void TargetSyncObserver(NetworkConnection conn, Vector3 position, Quaternion rotation)
    {
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
    }

    public void Despawn()
    {
        NetworkServer.Destroy(gameObject);
    }
}
