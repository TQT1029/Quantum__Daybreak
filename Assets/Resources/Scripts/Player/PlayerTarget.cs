using Unity.VisualScripting;
using UnityEngine;

public class PlayerTarget : MonoBehaviour
{
    private CharacterData _characterData => ReferenceManager.Instance.characterData;

    [Header("Interaction Settings")]
    [SerializeField] private LayerMask interactLayer; // Chỉ tương tác với layer "Interactable"

    // Biến lưu trữ vật thể đang được trỏ vào
    public InteractableConfig CurrentTarget { get; private set; }

    protected void FixedUpdate()
    {
        if (_characterData == null)
        {
            Debug.LogWarning("PlayerMovements: Chưa có Data!");
            return;
        }

        DetectTarget();
    }

    private void DetectTarget()
    {
        if (ReferenceManager.Instance.MainCamera == null) return;

        // Lấy vị trí chuột trong thế giới game
        Vector2 mousePos = InputManager.Instance.MousePosition; // Cần đảm bảo InputManager trả về Screen Position
        Vector2 worldPoint = ReferenceManager.Instance.MainCamera.ScreenToWorldPoint(mousePos);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, worldPoint, _characterData.InteractionRange, interactLayer);

        Debug.DrawRay(transform.position, worldPoint.normalized * _characterData.InteractionRange, hit.collider == null ? Color.red : Color.green, 0.5f);

        if (hit.collider != null)
        {
            // Thử lấy component InteractableConfig
            InteractableConfig interactable = hit.collider.GetComponent<InteractableConfig>();

            if (interactable == null)
            {
                hit.collider.transform.AddComponent<InteractableConfig>();
            }
            if (interactable != null)
            {
                ChangeTarget(interactable);
                return;
            }
        }

        // Nếu không bắn trúng gì, hoặc quá xa -> Xóa mục tiêu
        ClearTarget();
    }

    private void ChangeTarget(InteractableConfig newTarget)
    {
        if (CurrentTarget == newTarget) return;

        ClearTarget(); // Tắt outline cái cũ

        CurrentTarget = newTarget;
        CurrentTarget.ShowOutline(true); // Bật outline cái mới
    }

    private void ClearTarget()
    {
        if (CurrentTarget != null)
        {
            CurrentTarget.ShowOutline(false);
            CurrentTarget = null;
        }
    }
}