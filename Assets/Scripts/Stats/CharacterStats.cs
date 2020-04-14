using UnityEngine;
using Mirror;

public class CharacterStats : NetworkBehaviour
{
    public float maxHealth = 100f;

    [SyncVar]
    protected float currHealth;

    public Stat damage;

    private void Awake()
    {
        currHealth = maxHealth;
    }

    public void SetDefaults()
    {
        currHealth = maxHealth;
    }

    public void TakeDamage(float damage, string sourceID)
    {
        if (currHealth < damage) {
            currHealth = 0;
        } else {
            currHealth -= damage;
        }

        if (currHealth <= 0)
        {
            Die(sourceID);
        }
    }

    [ClientRpc]
    public void RpcTakeDamage(float damage, string sourceID)
    {
        if (currHealth < damage)
        {
            currHealth = 0;
        }
        else
        {
            currHealth -= damage;
        }

        if (currHealth <= 0)
        {
            Die(sourceID);
        }
    }

    public virtual void Die(string sourceID)
    {
        // Overwritten by target unit's die method
    }
}
