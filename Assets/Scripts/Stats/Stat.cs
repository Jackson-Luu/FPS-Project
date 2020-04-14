using UnityEngine;

[System.Serializable]
public class Stat
{
    [SerializeField]
    private float baseValue;

    private float finalValue;

    public float GetValue()
    {
        return finalValue;
    }

    public void AddModifier(float modifier)
    {
        if (modifier != 0)
        {
            finalValue += modifier;
        }
    }

    public void RemoveModifier(float modifier)
    {
        if (modifier != 0)
        {
            finalValue -= modifier;
        }
    }
}
