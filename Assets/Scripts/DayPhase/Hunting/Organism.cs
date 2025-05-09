using UnityEngine;

public abstract class Organism : MonoBehaviour
{
    public DayPhaseManager.MapType mapType;
    public Ingredient dropIngredient;

    public abstract void Init();
    public abstract void Updated();
    public abstract void Setup();
}
