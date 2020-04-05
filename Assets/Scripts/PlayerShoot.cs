using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerShoot : NetworkBehaviour
{
    public PlayerWeapon weapon;

    [SerializeField]
    private Camera cam;

    // Layer mask to restrict what objects player can hit
    [SerializeField]
    private LayerMask mask;

    // Start is called before the first frame update
    void Start()
    {
        if (cam == null)
        {
            Debug.Log("No camera referenced.");
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    // Method only called on client
    [Client]
    void Shoot()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, weapon.range, mask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                CmdPlayerShot(hit.collider.name);
            }
        }
    }

    // Method only called on server
    [Command]
    void CmdPlayerShot(string id)
    {
        Debug.Log(id + " has been shot.");
    }

}
