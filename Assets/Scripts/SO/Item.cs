using System.IO.Enumeration;
using UnityEngine;

public enum ItemType
{
    Animal,
    Fish,
    Gather,
    Grocery,

}

public enum MapType
{
    Warm,
    Hot,
    Cold,
    MaxCount,
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Item")]
public class Item : ScriptableObject
{
    public int item_number; // 아이템 번호
    public ItemType item_Type; // 아이템 타입
    public MapType item_MapType; // 아이템 맵 타입
    public string item_name; // 아이템 이름
    public int item_requireLevel; // 요구 레벨
    public Sprite item_image; // 아이템 이미지
    public float item_weight; // 아이템 무게
    public int item_maxcount; // 아이템 최대소지갯수
    public bool isGet = false; // 아이템 획득 여부
    public bool stackable = true; // 아이템이 중첩 가능한지 여부
    public int item_price; // 아이템 가격
}
