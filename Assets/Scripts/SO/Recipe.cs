using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public enum RecipeType
{
    Normal,
    Special,
}

[System.Serializable]
public class ItemRequirement
{
    public Item item; // 아이템 종류
    public int count; // 아이템 갯수
}

[CreateAssetMenu(fileName = "Recipe", menuName = "Recipe")]
public class Recipe : ScriptableObject
{
    public int recipe_number; // 레시피 번호
    public string recipe_name; // 레시피 이름
    public RecipeType recipe_type; // 레시피 타입
    public int recipePrice; // 레시피 가격
    public List<ItemRequirement> recipe_requireItems; // 레시피에 필요한 아이템 종류
}
