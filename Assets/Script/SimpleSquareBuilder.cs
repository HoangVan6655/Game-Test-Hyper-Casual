using UnityEngine;
using System.Collections.Generic;

public class SimpleSquareBuilder : MonoBehaviour
{
    [Header("--- SETTINGS ---")]
    public GameObject roadPrefab;       
    public Camera centerCamera; 

    [Header("--- DIMENSIONS ---")]
    public int sideLength = 10;      
    public int rectWidth = 15;       
    public int rectHeight = 10;      
    public float spacing = 1.0f;     
    public Vector3 startRotation = new Vector3(-90, 0, 90); 

    [Header("--- CORNERS ---")]
    public bool autoCorner = true;
    public bool useCornerFiller = true;
    public float cornerRadius = 2.0f;
    public float roadWidthReal = 1.2f;
    [Range(5, 50)] public int cornerSegments = 20;
    [Range(1.0f, 2.5f)] public float fillerScale = 1.3f;
    [Range(-1.0f, 1.0f)] public float fillerOffset = 0.0f;
    [Range(1.0f, 1.2f)] public float widthExpand = 1.05f;

    [HideInInspector] public List<Vector3> pathPoints = new List<Vector3>(); 
    private List<GameObject> spawnedRoads = new List<GameObject>();

    public void BuildSquare()
    {
        BuildGenericShape(new int[] { sideLength, sideLength, sideLength, sideLength }, -90f);
    }

    public void BuildRectangle()
    {
        BuildGenericShape(new int[] { rectWidth, rectHeight, rectWidth, rectHeight }, -90f);
    }

    public void BuildTriangle()
    {
        int len = sideLength + 5;
        BuildGenericShape(new int[] { len, len, len }, -120f);
    }

    void BuildGenericShape(int[] sides, float turnAngle)
    {
        ClearMap();
        Vector3 currentPos = transform.position;
        Vector3 moveDirection = Vector3.right;
        Quaternion currentRot = Quaternion.Euler(startRotation);

        for (int i = 0; i < sides.Length; i++) 
        {
            for (int j = 0; j < sides[i]; j++)
            {
                SpawnObject(roadPrefab, currentPos, currentRot, Vector3.one);
                pathPoints.Add(currentPos + new Vector3(0, 0.5f, 0));
                currentPos += moveDirection * spacing;
            }

            if (autoCorner)
            {
                BuildSeamlessCorner(currentPos, moveDirection, currentRot, turnAngle);
                
                if (useCornerFiller && Mathf.Abs(Mathf.Abs(turnAngle) - 90f) < 1f)
                {
                    SpawnCornerFillers(currentPos, moveDirection, currentRot);
                }

                Vector3 leftDir = Quaternion.Euler(0, -90, 0) * moveDirection; 
                Vector3 pivot = currentPos + (leftDir * cornerRadius); 
                Vector3 newDir = Quaternion.Euler(0, turnAngle, 0) * moveDirection; 
                Vector3 newLeft = Quaternion.Euler(0, -90, 0) * newDir;
                
                currentPos = pivot - (newLeft * cornerRadius);
                moveDirection = newDir;
                currentRot = Quaternion.Euler(0, turnAngle, 0) * currentRot;
            }
            else
            {
                moveDirection = Quaternion.Euler(0, turnAngle, 0) * moveDirection;
                currentRot = Quaternion.Euler(0, turnAngle, 0) * currentRot;
            }
        }
    }

    void BuildSeamlessCorner(Vector3 startPos, Vector3 forwardDir, Quaternion startRot, float totalAngle)
    {
        Vector3 leftDir = Quaternion.Euler(0, -90, 0) * forwardDir;
        Vector3 pivot = startPos + (leftDir * cornerRadius);
        
        float arcLength = (Mathf.Abs(totalAngle) / 360f) * 2 * Mathf.PI * cornerRadius;
        float segmentLength = arcLength / cornerSegments;
        float lengthScale = Mathf.Max(1.0f, (segmentLength / spacing) * 1.05f); 
        Vector3 cornerScale = new Vector3(widthExpand, lengthScale, 1.0f);

        for (int k = 1; k <= cornerSegments; k++)
        {
            float t = (float)k / cornerSegments;
            float angleDeg = totalAngle * t; 
            
            Vector3 finalPos = pivot + (Quaternion.Euler(0, angleDeg, 0) * (-leftDir * cornerRadius));
            Quaternion finalRot = Quaternion.Euler(0, angleDeg + (totalAngle / cornerSegments * 0.5f), 0) * startRot;
            
            SpawnObject(roadPrefab, finalPos, finalRot, cornerScale);
            pathPoints.Add(finalPos + new Vector3(0, 0.5f, 0));
        }
    }

    void SpawnCornerFillers(Vector3 startPos, Vector3 dir, Quaternion rot)
    {
        SpawnSingleFiller(startPos, dir, rot, 0f);
        SpawnSingleFiller(startPos, dir, rot, -90f);
        SpawnSingleFiller(startPos, dir, rot, -45f);
    }

    void SpawnSingleFiller(Vector3 startPos, Vector3 dir, Quaternion rot, float angle)
    {
        Vector3 left = Quaternion.Euler(0, -90, 0) * dir;
        Vector3 pivot = startPos + (left * cornerRadius);
        Vector3 pos = pivot + (Quaternion.Euler(0, angle, 0) * (-left * cornerRadius));
        Quaternion fRot = Quaternion.Euler(0, angle, 0) * rot;
        
        GameObject obj = Instantiate(roadPrefab, pos + (fRot * new Vector3(0, fillerOffset, 0)), fRot, transform);
        obj.transform.localScale = Vector3.Scale(obj.transform.localScale, new Vector3(widthExpand, fillerScale, 1f));
        obj.transform.position -= new Vector3(0, 0.01f, 0);
        spawnedRoads.Add(obj);
    }

    void SpawnObject(GameObject prefab, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        if (prefab == null) return;
        GameObject obj = Instantiate(prefab, pos, rot, transform);
        obj.transform.localScale = Vector3.Scale(obj.transform.localScale, scale);
        spawnedRoads.Add(obj);
    }

    public void ClearMap()
    {
        pathPoints.Clear();
        foreach (var road in spawnedRoads) if (road != null) DestroyImmediate(road);
        spawnedRoads.Clear();
    }
}