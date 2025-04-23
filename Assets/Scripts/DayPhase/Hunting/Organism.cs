using UnityEngine;

public abstract class Organism : MonoBehaviour
{
    public DayPhaseManager.MapType mapType;
    public Ingredient dropIngredient;

    public abstract void Init();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
