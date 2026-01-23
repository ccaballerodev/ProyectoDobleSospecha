using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    private PlayerInputActions inputActions;
    private Vector2 moveInput;

    public float speed = 5f;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputActions.Player.Enable();
    }

    void OnEnable()
    {
        if (inputActions != null)
            inputActions.Player.Enable();
    }

    void OnDisable()
    {
        if (inputActions != null)
            inputActions.Player.Disable();
    }

    void Update()
    {
        if (!IsOwner) return;

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        transform.Translate(move * speed * Time.deltaTime);
    }
}
