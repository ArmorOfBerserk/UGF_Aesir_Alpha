using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    private InputActions inputActions;

    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnRightStick;
    public event Action OnJump;
    public event Action OnAttack;
    public event Action OnDash;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        inputActions = new();

        inputActions.Player.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        inputActions.Player.Move.canceled += ctx => OnMove?.Invoke(Vector2.zero);

        inputActions.Player.Jump.performed += ctx => OnJump?.Invoke();

        inputActions.Player.RightStick.performed += ctx => OnRightStick?.Invoke(ctx.ReadValue<Vector2>());
        inputActions.Player.RightStick.canceled += ctx => OnRightStick?.Invoke(Vector2.zero);

        inputActions.Player.Attack.performed += ctx => OnAttack?.Invoke();

        inputActions.Player.Dash.performed += ctx => OnDash?.Invoke();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}