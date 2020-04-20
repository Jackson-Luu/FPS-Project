using UnityEngine;
using Mirror;

public class ItemPickup : NetworkBehaviour
{
    private PlayerUI playerUI;

    public Item item;

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

    public void Despawn()
    {
        NetworkServer.Destroy(gameObject);
    }
}
