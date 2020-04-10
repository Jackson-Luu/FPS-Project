using UnityEngine;

public class Util
{
    // Add all children recursively to new layer
    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) { return; }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) { continue; }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    /*
    // Constrain unit position to within map
    public static void ConstrainPlayerPosition(Transform transform, float groundRadius)
    {
        if (transform.position.x > groundRadius)
        {
            transform.position = new Vector3(groundRadius, transform.position.y, transform.position.z);
        }

        if (transform.position.z > groundRadius)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, groundRadius);
        }

        if (transform.position.x < -groundRadius)
        {
            transform.position = new Vector3(-groundRadius, transform.position.y, transform.position.z);
        }

        if (transform.position.z < -groundRadius)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -groundRadius);
        }
    }
    */
}
