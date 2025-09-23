using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item")]
public class Item : ScriptableObject
{
    public int item_number; // 아이템 번호
    public string item_name; // 아이템 이름
    public Sprite item_image; // 아이템 이미지
    public float item_weight; // 아이템 무게
    public int item_maxcount; // 아이템 최대소지갯수
    public bool isGet = false; // 아이템 획득 여부
    public bool stackable = true; // 아이템이 중첩 가능한지 여부
    public int item_price; // 아이템 가격
}
