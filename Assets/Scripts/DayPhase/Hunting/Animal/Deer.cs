using UnityEngine;

public class Deer : Animals//IAttack, IDamagable
{
    public override void Init()
    {
        MapType = MapType.Warm;
        maxHP = 10f;
        currentHP = maxHP;
        attack = 3f;
        speed = 1.5f;
        attackRange = 2.5f;
        isDead = false;

        patrolRadius = 20f;
        waitSeconds = 2f;
        chaseDIstance = 25f;
        targetUpdateMinDelta = 0.5f;
    }
}
