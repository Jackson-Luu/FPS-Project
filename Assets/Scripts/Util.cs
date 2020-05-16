using UnityEngine;

public class Util
{
    // Add all children recursively to new layer
    public static void SetLayerRecursively(GameObject obj, int newLayer, string exclude = null)
    {
        if (obj == null) { return; }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) { continue; }
            if (exclude != null && child.name.Equals(exclude)) { continue; }
            SetLayerRecursively(child.gameObject, newLayer, exclude);
        }
    }

    // Rotate objects to align with normal
    public static void AlignTransform(Transform transform, Vector3 normal)
    {
        Vector3 proj = transform.forward - (Vector3.Dot(transform.forward, normal)) * normal;
        transform.rotation = Quaternion.LookRotation(proj, normal);
    }
}
