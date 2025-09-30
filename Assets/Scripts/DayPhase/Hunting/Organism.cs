using UnityEngine;

public abstract class Organism : MonoBehaviour
{
    public MapType MapType;
    public Item DropItem;

    public abstract void Init();
    public abstract void Updated();
    public abstract void Setup();
}
