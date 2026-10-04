using UnityEngine;

public class FlowerPetal : MonoBehaviour
{
    private Flower parentFlower;

    private void Awake()
    {
        parentFlower = GetComponentInParent<Flower>();
    }

    private void OnMouseDown()
    {
        if (parentFlower != null)
        {
            parentFlower.TryRemovePetal();
        }
    }
}