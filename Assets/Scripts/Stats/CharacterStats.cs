using UnityEngine;
using Mirror;

public class CharacterStats : NetworkBehaviour
{
    public float maxHealth = 100f;

    [SyncVar]
    protected float currHealth;

    public Stat damage;
    public Stat armor;

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
        damage -= armor.GetValue();
        damage = Mathf.Clamp(damage, 0, int.MaxValue);

        if (currHealth < damage) {
            currHealth = 0;
        } else {
            currHealth -= damage;
        }

        if (currHealth <= 0)
        {
            Die(sourceID);
            if (isServer)
            {
                RpcDie(sourceID);
            }
        }
    }

    public void Heal(float amount)
    {
        if (currHealth + amount > maxHealth)
        {
            currHealth = maxHealth;
        } else
        {
            currHealth += amount;
        }
    }

    [ClientRpc]
    public void RpcDie(string sourceID)
    {
        Die(sourceID);
    }

    public virtual void Die(string sourceID)
    {
        // Overwritten by target unit's die method
    }
}
