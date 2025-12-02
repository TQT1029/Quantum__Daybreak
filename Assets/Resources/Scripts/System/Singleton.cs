using UnityEngine;

// Lớp này xử lý việc tạo Instance và DontDestroyOnLoad tự động
public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        DontDestroyOnLoad(gameObject);

        // Gọi hàm khởi tạo riêng nếu cần
        OnAwake();
    }

    protected virtual void OnAwake() { }
}