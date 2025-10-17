using System.Buffers;
using UnityEngine;

public abstract class AttackBehavior : MonoBehaviour
{
    protected bool _busy;
    protected Animal owner;
    public virtual void Setup(Animal a) => owner = a;

    // 공격 시작시 호출
    public abstract void OnEnter();

    // 매 프레임 호출, true면 공격 루프 계속, false면 공격 종료
    public abstract bool OnUpdate(float dt);

    // 공격 종료시 호출
    public abstract void OnExit();

}
