using UnityEngine;

public class FlowerCenter : MonoBehaviour
{
    private Flower flower;

    private void Awake()
    {
        flower = GetComponentInParent<Flower>();
    }

    private void OnMouseDown()
    {
        if (flower != null)
        {
            flower.SelectFlower();
        }
    }
}