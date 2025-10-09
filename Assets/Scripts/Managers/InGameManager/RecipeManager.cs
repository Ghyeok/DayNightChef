using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeManager : SingletonManager<RecipeManager>
{
    [SerializeField]
    private List<Recipe> recipe = new();

    public override void Awake()
    {
        base.Awake();

        if (recipe != null || recipe.Count != 0)
            recipe = Resources.LoadAll<Recipe>("Recipes").ToList();
    }
}
