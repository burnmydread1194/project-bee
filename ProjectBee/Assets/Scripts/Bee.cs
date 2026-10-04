using TMPro;
using UnityEngine;

public class Bee : MonoBehaviour
{
    [SerializeField] private TMP_Text requestText;
    [SerializeField] private SpriteRenderer beeRenderer;

    private Color normalColor;

    private void Awake()
    {
        if (beeRenderer != null)
        {
            normalColor = beeRenderer.color;
        }
    }

    public void Initialize(int requestedAmount)
    {
        if (requestText != null)
        {
            requestText.text = requestedAmount.ToString();
        }

        if (beeRenderer != null)
        {
            beeRenderer.color = normalColor;
        }
    }

    public void Success()
    {
        if (beeRenderer != null)
        {
            beeRenderer.color = Color.green;
        }
    }

    public void Fail()
    {
        if (beeRenderer != null)
        {
            beeRenderer.color = Color.red;
        }
    }
}