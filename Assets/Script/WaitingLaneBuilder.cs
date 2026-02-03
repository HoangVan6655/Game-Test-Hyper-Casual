using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WaitingLaneBuilder : MonoBehaviour
{
    [Header("--- SETUP ---")]
    public GameObject waitingPrefab; 
    public int length = 1; 
    public float spacing = 0f;     
    public float spawnHeight = 0.5f;
    public SimpleSquareBuilder mainMapToConnect; 

    [HideInInspector] public List<Vector3> lanePoints = new List<Vector3>();

    public void BuildLane()
    {
        List<GameObject> toRemove = new List<GameObject>();
        foreach (Transform child in transform) if (child.name.Contains("(Clone)")) toRemove.Add(child.gameObject);
        foreach (var child in toRemove) DestroyImmediate(child);
        
        lanePoints.Clear();
        if(waitingPrefab == null) return;

        Vector3 currentPos = transform.position;
        Vector3 direction = transform.forward; 

        for (int i = 0; i < length; i++)
        {
            Instantiate(waitingPrefab, currentPos, transform.rotation, transform).name = waitingPrefab.name + "(Clone)_" + i;
            lanePoints.Add(currentPos + new Vector3(0, spawnHeight, 0));
            currentPos += direction * spacing;
        }
    }

    public List<Vector3> GetFullRoute()
    {
        List<Vector3> fullRoute = new List<Vector3>(lanePoints);
        if (mainMapToConnect != null && mainMapToConnect.pathPoints.Count > 0)
        {
            Vector3 endOfLane = lanePoints[lanePoints.Count - 1];
            var closestPoint = mainMapToConnect.pathPoints.OrderBy(p => Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(endOfLane.x, 0, endOfLane.z))).First();

            int startIndex = mainMapToConnect.pathPoints.IndexOf(closestPoint);
            for (int i = startIndex; i < mainMapToConnect.pathPoints.Count; i++) fullRoute.Add(mainMapToConnect.pathPoints[i]);
            for (int i = 0; i < startIndex; i++) fullRoute.Add(mainMapToConnect.pathPoints[i]);
        }
        return fullRoute;
    }
}