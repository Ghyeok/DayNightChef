using UnityEngine;
using System.Collections.Generic;

public class DayPhaseManager : SingletonManager<DayPhaseManager>
{
    //현재 존재하는 animals
    public List<Animals> animalList = new List<Animals>();
    public enum PlayerState
    {
        Alive,
        Dead,
    }
    public enum AnimalStates
    {
        Patrol = 0,
        Attack,
        Chase,
        Die
    }

    public enum PlayerBehavior
    {
        Hunting,
        Fishing,
        Gathering,
        MaxCount,
    }

    public enum MapType
    {
        Warm,
        Hot,
        Cold,
        MaxCount,
    }

    public enum UpgradeType
    {
        Hp,
        MoveSpeed,
        Knife,
        Fishing,
        Bag,
        MaxCount,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(animalList == null)
            animalList = new List<Animals>();
    }

    // Update is called once per frame
    void Update()
    {
        if(animalList == null) return; 
        for(int i = 0; i < animalList.Count; ++i)
        {
            if(animalList[i] != null)
            {
                animalList[i].Updated();
            }
        }
    }
}
