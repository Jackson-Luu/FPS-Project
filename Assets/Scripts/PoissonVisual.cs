using System.Collections.Generic;
using UnityEngine;

public class PoissonVisual : MonoBehaviour
{
    public float radius = 300;
    public Vector2 regionSize = new Vector2(8000,8000);
    public int rejectionSamples = 30;
    public float displayRadius = 75;

    public Vector2 offset = new Vector2(-4000, -4000);

    List<Vector2> points;

    void OnValidate()
    {
        points = PoissonDiscSample.GeneratePoints(radius, regionSize, rejectionSamples);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector2.zero, regionSize);
        if (points != null)
        {
            foreach (Vector2 point in points)
            {
                Gizmos.DrawSphere(point + offset, displayRadius);
            }
        }
    }
}
