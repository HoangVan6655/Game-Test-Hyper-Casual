using UnityEngine;
using System.Collections.Generic;

public class TinyCharacter : MonoBehaviour
{
    [Header("--- VISUAL ---")]
    public Renderer bodyRenderer; 
    public float runSpeed = 5f;

    [HideInInspector] public Color myColorID; 

    private float laneOffset = 0f; 
    private bool isRunning = false;
    private List<Vector3> runPath;
    
    private int currentPathIndex = 0; 
    private Vector3 virtualCenterPos; 
    private int loopStartIndex = 0;   

    public void SetColor(Color c)
    {
        myColorID = c; 
        if (bodyRenderer) bodyRenderer.material.color = c;
    }

    public void SetLaneOffset(float offset) { laneOffset = offset; }

    public void JumpToTrack(List<Vector3> path, int loopStart)
    {
        if (path == null || path.Count == 0) return;

        runPath = path;
        loopStartIndex = loopStart;

        currentPathIndex = FindClosestPathIndex();
        virtualCenterPos = runPath[currentPathIndex];
        
        UpdatePositionFromVirtualCenter();
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning || runPath == null || runPath.Count == 0) return;

        int nextIndex = (currentPathIndex + 1);
        if (nextIndex >= runPath.Count) nextIndex = loopStartIndex;

        Vector3 targetCenter = runPath[nextIndex];
        virtualCenterPos = Vector3.MoveTowards(virtualCenterPos, targetCenter, runSpeed * Time.deltaTime);

        if (Vector3.Distance(virtualCenterPos, targetCenter) < 0.05f)
        {
            currentPathIndex = nextIndex;
        }

        UpdatePositionFromVirtualCenter();
    }

    void UpdatePositionFromVirtualCenter()
    {
        int nextIndex = currentPathIndex + 1;
        if (nextIndex >= runPath.Count) nextIndex = loopStartIndex;

        Vector3 roadDir = (runPath[nextIndex] - virtualCenterPos).normalized;
        
        if (roadDir == Vector3.zero) 
        {
            int nextNext = nextIndex + 1;
            if (nextNext >= runPath.Count) nextNext = loopStartIndex;
            roadDir = (runPath[nextNext] - virtualCenterPos).normalized;
        }

        Vector3 rightDir = Vector3.Cross(roadDir, Vector3.up);
        transform.position = virtualCenterPos + (rightDir * laneOffset);

        if (roadDir != Vector3.zero)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(roadDir), Time.deltaTime * 10f);
    }

    int FindClosestPathIndex()
    {
        int bestIndex = 0;
        float closestDist = Mathf.Infinity;
        for (int i = 0; i < Mathf.Min(50, runPath.Count); i++) 
        {
            float d = Vector3.Distance(transform.position, runPath[i]);
            if (d < closestDist)
            {
                closestDist = d;
                bestIndex = i;
            }
        }
        return bestIndex;
    }
}