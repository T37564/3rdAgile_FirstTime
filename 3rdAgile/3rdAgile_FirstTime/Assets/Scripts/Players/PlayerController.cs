using UnityEngine;
// 将来：using Fusion;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour   // NetworkBehaviour
{
    [SerializeField] private PlayerInputHandler inputHandler;

    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float jumpForce = 50.0f;

    private Rigidbody rb;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        rb = GetComponent<Rigidbody>();
    }

    //いずれ public override void FixedUpdateNetwork()
    private void FixedUpdate()
    {
        #if Fusion化する時用
        //if (GetInput(out PlayerInput input))
        //{
        //    ここで入力
        //}
        #endif

        // 確認用
        var input = inputHandler.CurrentInput;

        Vector3 move = new Vector3(input.move.x, 0.0f, input.move.y);
        rb.MovePosition(rb.position + move * moveSpeed * Time.deltaTime);

        if (input.jump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        inputHandler.ClearOneShotInput();
    }
}
