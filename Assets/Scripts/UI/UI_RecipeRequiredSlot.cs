using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RecipeRequiredSlot : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemName; // 아이템 이름
    [SerializeField] private TextMeshProUGUI itemCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetData(Item item, int required)
    {
        itemImage.sprite = item.item_image;
        itemName.text = item.item_name;

        int remain = WarehouseManager.Instance.GetCount(item);
        itemCount.text = $"{remain} / {required}";

        if (remain < required)
        {
            itemCount.color = Color.red;
        }
        else
        {
            itemCount.color = Color.white;
        }
    }
}
