using UnityEngine;

public class LevelManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                LaneController clickedLane = hit.collider.GetComponentInParent<LaneController>();
                if (clickedLane != null) clickedLane.ReleaseMinions();
            }
        }
    }
}