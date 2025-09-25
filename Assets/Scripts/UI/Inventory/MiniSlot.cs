using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class MiniSlot : MonoBehaviour
{
    private Image itemImage;
    private TextMeshProUGUI countText;
    private TextMeshProUGUI itemWeight;

    private void Awake()
    {
        if(!itemImage) itemImage = transform.Find("ItemImage")?.GetComponent<Image>();
        if (!countText) countText = transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();
        if (!itemWeight) itemWeight = transform.Find("ItemWeight")?.GetComponent<TextMeshProUGUI>();
    }
    public void Clear()
    {
        if (itemImage) itemImage.sprite = null;
        if (itemImage) itemImage.enabled = false;
        if (countText) countText.text = "";
        if (itemWeight) itemWeight.text = "";
    }

    public void Set(InventoryManager.Entry e)
    {
        bool has = (e.item != null && e.count > 0);
        if (!has)
        {
            Clear();
            return; 
        }

        var spr = e.item.item_image;
        if (itemImage)
        { 
            itemImage.sprite = spr;
            itemImage.enabled = spr != null;
        }
        if (countText) countText.text = e.count.ToString();
        if (itemWeight) itemWeight.text = $"{e.item.item_weight * e.count}g";
    }
}
