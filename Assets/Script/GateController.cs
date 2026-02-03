using UnityEngine;
using TMPro;

public class GateController : MonoBehaviour
{
    [Header("--- SETUP ---")]
    public int requiredAmount = 20; 
    public TextMeshProUGUI textDisplay; 
    public GameObject visualModel;      

    private Color gateColorID;

    public void Setup(int amount)
    {
        requiredAmount = amount;
        UpdateText();
    }

    public void SetColor(Color newColor)
    {
        gateColorID = newColor;
        if (visualModel != null)
        {
            Renderer rend = visualModel.GetComponent<Renderer>();
            if (rend == null) rend = visualModel.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = newColor;
        }
        UpdateText();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        TinyCharacter minion = other.GetComponent<TinyCharacter>();
        if (minion != null && IsSameColor(minion.myColorID, gateColorID))
        {
            requiredAmount--;
            UpdateText();
            Destroy(other.gameObject); 

            if (GameManager.Instance != null)
                GameManager.Instance.AddScore();

            if (requiredAmount <= 0)
            {
                requiredAmount = 0;
                gameObject.SetActive(false); 
            }
        }
    }

    void UpdateText()
    {
        if (textDisplay != null) textDisplay.text = requiredAmount.ToString();
    }

    bool IsSameColor(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) < 0.1f && Mathf.Abs(c1.g - c2.g) < 0.1f && Mathf.Abs(c1.b - c2.b) < 0.1f;
    }
}