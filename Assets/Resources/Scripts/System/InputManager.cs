using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10)]
public class InputManager : Singleton<InputManager>
{
    [Header("Di chuyển (Movement)")]
    public Vector2 MoveInput;

    [Header("Camera")]
    public Vector2 LookInput;

    [Header("Hành động (Actions)")]
    public bool IsInteracting; // Nút E hoặc F chung
    public bool IsPausing;     // Nút ESC

    // Gán các hàm này vào Unity Events trong PlayerInput component
    public void OnMove(InputAction.CallbackContext context) { MoveInput = context.ReadValue<Vector2>(); }

    public void OnInteract(InputAction.CallbackContext context) => IsInteracting = context.performed;

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsPausing = true;
            // Toggle pause logic mẫu
            if (GameManager.Instance.CurrentState == GameState.Gameplay)
                GameManager.Instance.ChangeState(GameState.Paused);
            else if (GameManager.Instance.CurrentState == GameState.Paused)
                GameManager.Instance.ChangeState(GameState.Gameplay);
        }
        else if (context.canceled)
        {
            IsPausing = false;
        }
    }
}