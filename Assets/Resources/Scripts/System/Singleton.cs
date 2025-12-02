using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    // Biến kiểm soát việc ứng dụng đang tắt để tránh tạo lại Singleton
    private static bool _isQuitting = false;

    public static T Instance
    {
        get
        {
            if (_isQuitting)
            {
                // Trả về null hoặc warn nếu ai đó cố gọi khi game đang tắt
                return null;
            }

            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();

                // Nếu vẫn null thì in lỗi (Vì ta dùng Bootstrapper nên không tự New GameObject)
                if (_instance == null)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} not found. Make sure Bootstrapper is running.");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        DontDestroyOnLoad(gameObject);
        OnAwake();
    }

    protected virtual void OnAwake() { }

    // Ngăn chặn tạo lại instance khi tắt game
    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}