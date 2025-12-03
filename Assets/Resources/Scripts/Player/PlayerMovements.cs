using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovements : MonoBehaviour
{
    // Lấy dữ liệu nhân vật từ ReferenceManager
    private CharacterData _characterData => ReferenceManager.Instance.characterData;
    [SerializeField] private Rigidbody2D _rb;

    // Nếu true: Nhân vật sẽ xoay mặt theo hướng di chuyển
    // Nếu false: Chỉ di chuyển vị trí (thích hợp cho game Pixel Art dùng Animation để đổi hướng)
    [SerializeField] private bool _rotateTowardsDirection = true;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Kiểm tra an toàn: Nếu chưa có data thì không di chuyển


        if (_characterData == null)
        {
            Debug.LogWarning("PlayerMovements: Chưa có Data!");
            return;
        }
        Move(_characterData.MoveSpeed);

    }
    private void Move(float _moveSpeed)
    {
        Vector2 input = InputManager.Instance.MoveInput;

        _rb.linearVelocity = input * _moveSpeed;

        if (_rotateTowardsDirection && input != Vector2.zero)
        {
            RotateCharacter(input);
        }
    }

    private void RotateCharacter(Vector2 direction)
    {
        // Tính góc xoay dựa trên hướng vector (Atan2 trả về radian, cần đổi sang độ)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Xoay quanh trục Z (trục độ sâu trong 2D)
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}