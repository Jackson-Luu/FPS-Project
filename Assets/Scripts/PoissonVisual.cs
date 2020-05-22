using System.Collections.Generic;
using UnityEngine;

public class PoissonVisual : MonoBehaviour
{
    public float radius = 50;
    public Vector2 regionSize = new Vector2(120, 120);
    public int rejectionSamples = 30;
    public float displayRadius = 75;

    List<Vector2> points;

    void OnValidate()
    {
        points = PoissonDiscSample.GeneratePoints(radius, regionSize, 0, rejectionSamples);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector2.zero, regionSize);
        if (points != null)
        {
            foreach (Vector2 point in points)
            {
                Gizmos.DrawSphere(point, displayRadius);
            }
        }
    }
}
