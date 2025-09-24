using UnityEngine;

public abstract class Organism : MonoBehaviour
{
    public MapType mapType;
    public Ingredient dropIngredient;

    public abstract void Init();
    public abstract void Updated();
    public abstract void Setup();
}
