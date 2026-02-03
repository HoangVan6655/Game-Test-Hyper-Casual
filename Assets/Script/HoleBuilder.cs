using UnityEngine;
using System.Collections.Generic;

public class HoleBuilder : MonoBehaviour
{
    public GameObject holePrefab;
    public SimpleSquareBuilder mapBuilder;

    [Header("--- CONFIG ---")]
    public float distanceFromRoad = 8f; 
    
    public void BuildMultipleHoles(List<Color> colors, List<int> amounts)
    {
        if (mapBuilder == null || mapBuilder.pathPoints.Count == 0 || colors.Count != amounts.Count) return;

        foreach (Transform child in transform) Destroy(child.gameObject);

        int totalHoles = colors.Count;
        int totalPoints = mapBuilder.pathPoints.Count;
        int step = totalPoints / totalHoles;
        int startOffset = step / 2; 

        for (int i = 0; i < totalHoles; i++)
        {
            int pointIndex = (startOffset + (i * step)) % totalPoints;
            SpawnHoleAtPoint(pointIndex, "Hole_" + (i + 1), colors[i], amounts[i]);
        }
    }

    void SpawnHoleAtPoint(int index, string name, Color c, int amount)
    {
        Vector3 roadPos = mapBuilder.pathPoints[index];
        Vector3 nextPos = mapBuilder.pathPoints[(index + 1) % mapBuilder.pathPoints.Count];
        
        Vector3 roadDir = (nextPos - roadPos).normalized;
        Vector3 inDir = Vector3.Cross(roadDir, Vector3.up); 

        Vector3 finalPos = roadPos + (inDir * distanceFromRoad);
        Quaternion rot = Quaternion.LookRotation(-inDir);

        GameObject hole = Instantiate(holePrefab, finalPos, rot, transform);
        hole.name = name;

        GateController gate = hole.GetComponent<GateController>();
        if (gate != null) 
        {
            gate.Setup(amount);
            gate.SetColor(c);
        }
    }
}