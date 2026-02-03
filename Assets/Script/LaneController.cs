using UnityEngine;
using System.Collections.Generic;

public class LaneController : MonoBehaviour
{
    public WaitingLaneBuilder laneBuilder;
    public GameObject minionPrefab;
    
    [Header("--- CONFIG ---")]
    public int minionCount = 60; 
    public int groupSize = 20;
    public float spacingX = 0.5f; 
    public float spacingZ = 0.6f;

    public List<Color> otherColors = new List<Color>() { Color.red, Color.yellow };
    private Queue<TinyCharacter> waitingQueue = new Queue<TinyCharacter>();

    public void SpawnMinionsSequence(List<Color> targetColors)
    {
        if (laneBuilder != null && laneBuilder.lanePoints.Count == 0)
        {
            laneBuilder.BuildLane();
        }

        if (laneBuilder == null || laneBuilder.lanePoints.Count == 0) return;
        
        ClearExistingMinions();
        waitingQueue.Clear();

        Vector3 startPos = laneBuilder.lanePoints[0];
        Quaternion rot = laneBuilder.transform.rotation;

        for (int i = 0; i < minionCount; i++)
        {
            Color minionColor = (targetColors != null && targetColors.Count > 0) 
                ? targetColors[(i / groupSize) % targetColors.Count] 
                : otherColors[0];

            int row = i / 4; 
            int col = i % 4; 
            Vector3 offsetPos = new Vector3((col - 1.5f) * spacingX, 0, -row * spacingZ);
            
            SpawnSingleMinion(startPos + (rot * offsetPos), rot, minionColor, (col - 1.5f) * spacingX);
        }
    }

    void SpawnSingleMinion(Vector3 pos, Quaternion rot, Color c, float offset)
    {
        GameObject obj = Instantiate(minionPrefab, pos, rot, transform);
        obj.tag = "Player"; 

        TinyCharacter minion = obj.GetComponent<TinyCharacter>();
        if(minion) 
        {
            minion.SetColor(c);
            minion.SetLaneOffset(offset); 
        }
        
        EnsurePhysics(obj);
        waitingQueue.Enqueue(minion);
    }

    void EnsurePhysics(GameObject obj)
    {
        if(obj.GetComponent<Collider>() == null) 
        {
            BoxCollider box = obj.AddComponent<BoxCollider>();
            box.size = new Vector3(0.5f, 1f, 0.5f);
            box.center = new Vector3(0, 0.5f, 0);
        }
        if(obj.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.isKinematic = true; 
            rb.useGravity = false; 
        }
    }

    void ClearExistingMinions()
    {
        List<GameObject> toDelete = new List<GameObject>();
        foreach (Transform child in transform) 
        {
            if (child.CompareTag("Player") || child.GetComponent<TinyCharacter>() != null)
                toDelete.Add(child.gameObject);
        }
        foreach (var obj in toDelete) DestroyImmediate(obj);
    }

    public void ReleaseMinions()
    {
        StopAllCoroutines();
        StartCoroutine(ReleaseRoutine());
    }

    System.Collections.IEnumerator ReleaseRoutine()
    {
        if (laneBuilder == null) yield break;
        List<Vector3> path = laneBuilder.GetFullRoute();
        int roadStartIndex = laneBuilder.lanePoints.Count;

        while (waitingQueue.Count > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                if (waitingQueue.Count == 0) break;
                TinyCharacter m = waitingQueue.Dequeue();
                if(m != null) m.JumpToTrack(path, roadStartIndex);
            }
            yield return new WaitForSeconds(0.3f); 
        }
    }
}