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
            playerUI.ItemPickupEnable(item.name);
            playerUI.onItemPickUpCallback += PickUp;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<Player>().isLocalPlayer)
        {
            playerUI = other.gameObject.GetComponent<PlayerComponents>().playerUIInstance.GetComponent<PlayerUI>();
            playerUI.ItemPickupDisable();
            playerUI.onItemPickUpCallback -= PickUp;
        }
    }

    void PickUp(Player player)
    {
        Debug.Log("HELLO SERVER");
        player.GetComponent<Inventory>().Add(item);
        CmdPickUp(player.gameObject);
    }

    [Command]
    void CmdPickUp(GameObject player)
    {
        Debug.Log("HELLO SERVER B");
        player.GetComponent<Inventory>().Add(item);
        Destroy(gameObject);
    }
}
