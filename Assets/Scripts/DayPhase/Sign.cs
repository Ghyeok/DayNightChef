using TMPro;
using UnityEngine;

public class Sign : MonoBehaviour
{
    [SerializeField] private Item[] items;
    [SerializeField] private Item item;
    [SerializeField] private SpriteRenderer image;
    [SerializeField] private TextMeshPro text;

    private GatherSpawner spawner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawner = GetComponentInParent<GatherSpawner>();
        SetData();
    }

    private void SetData()
    {
        items = spawner.SpawnItems;
        foreach (Item curItem in items)
        {
            if(curItem.item_Grade == ItemGrade.Normal)
            {
                item = curItem;
            }
        }

        image.sprite = item.item_image;
        text.text = item.item_name;
    }
}
