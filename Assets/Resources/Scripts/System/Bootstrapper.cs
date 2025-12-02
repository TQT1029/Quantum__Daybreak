using UnityEngine;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        // Tạo một object gốc chứa tất cả hệ thống
        GameObject systemObject = Object.Instantiate((GameObject)Resources.Load("SystemPrefabs/SystemManagers"));
        Object.DontDestroyOnLoad(systemObject);
    }
}