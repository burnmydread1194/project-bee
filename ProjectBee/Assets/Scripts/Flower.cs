using TMPro;
using UnityEngine;

public class Flower : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private SpriteRenderer flowerRenderer;
    [SerializeField] private GameObject[] petals;

    public int Value { get; private set; }

    private BeeMathGameManager gameManager;
    private Color normalColor;

    public void Initialize(int value, BeeMathGameManager manager)
    {
        gameManager = manager;
        Value = value;

        if (flowerRenderer != null)
            normalColor = flowerRenderer.color;

        UpdateVisuals();
    }
    
    public void SelectFlower()
    {
        if (gameManager != null)
            gameManager.SelectFlower(this);
    }

    public void TryRemovePetal()
    {
        if (gameManager != null)
            gameManager.RemovePetal(this);
    }

    public void RemovePetal()
    {
        if (Value <= 1)
            return;

        Value--;

        UpdateVisuals();
    }

    public void SetSelected(bool selected)
    {
        if (flowerRenderer == null)
            return;

        flowerRenderer.color = selected
            ? Color.yellow
            : normalColor;
    }

    private void UpdateVisuals()
    {
        if (numberText != null)
            numberText.text = Value.ToString();

        if (petals == null)
            return;

        for (int i = 0; i < petals.Length; i++)
        {
            petals[i].SetActive(i < Value);
        }
    }
}