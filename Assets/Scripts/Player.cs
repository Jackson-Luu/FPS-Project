using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private float maxHealth = 100f;

    [SyncVar]
    private float currentHealth;

    void Awake()
    {
        setDefaults();    
    }

    public void setDefaults()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(transform.name + " " + currentHealth +" " + amount);
    }
}
