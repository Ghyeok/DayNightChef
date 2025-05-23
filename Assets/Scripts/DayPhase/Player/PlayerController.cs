using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public VariableJoystick joystick;

    private CharacterController cc;
    private Animator anim;

    private void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        TestAttack();
    }

    private void Move()
    {
        float h = joystick.Horizontal;
        float v = joystick.Vertical;
        Vector3 dir = new Vector3(h, 0, v).normalized;

        cc.Move(dir * DayPhasePlayerManager.Instance.playerMoveSpeed * Time.deltaTime);

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            anim.SetBool("isWalk", true);
        }
        else if (dir.magnitude < 0.01f) anim.SetBool("isWalk", false);
    }

    private void TestAttack()
    {
        if (Input.GetKey(KeyCode.E)) anim.SetBool("isAttack", true);
        else anim.SetBool("isAttack", false);
    }
}