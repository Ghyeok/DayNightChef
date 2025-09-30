public abstract class State<T>
{
    //해당 상태를 시작할 때 1회 호출
    public abstract void Enter(T owner);
    //해당 상태를 업데이트할 때 매 프레임 호출
    public abstract void Execute(T owner);
    //해당 상태를 종료할 때 1회 호출
    public abstract void Exit(T owner);
}

public sealed class StateMachine<T>
{
    public State<T> Current { get; private set; }

    public void ChangeState(State<T> next, T owner)
    {
        if (next == null || next == Current) return;
        Current?.Exit(owner);
        Current = next;
        Current.Enter(owner);
    }

    public void Update(T owner) => Current?.Execute(owner);
}
