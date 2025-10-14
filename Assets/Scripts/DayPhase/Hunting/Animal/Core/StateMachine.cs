public interface IState<T>
{
    void Enter(T owner);
    void Execute(T owner, float dt); // delta time
    void Exit(T owner);
}

public sealed class StateMachine<T>
{
    readonly T owner;
    IState<T> current;

    public StateMachine(T owner)
    {
        this.owner = owner;
    }

    public void ChangeState(IState<T> next)
    {
        current?.Exit(owner);
        current = next;
        current?.Enter(owner);
    }

    public void Update(float dt) => current?.Execute(owner, dt);
}