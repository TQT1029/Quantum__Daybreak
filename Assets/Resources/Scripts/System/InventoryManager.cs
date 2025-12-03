using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Cần thiết cho các thành phần UI (nếu dùng Text)

public class InventoryManager : MonoBehaviour
{
    // --- Thiết lập Singleton ---
    // Giúp các script khác (như PlayerLooter) dễ dàng truy cập
    public static InventoryManager Instance;

    // Danh sách lưu trữ các vật phẩm (chỉ lưu tên vật phẩm)
    private List<string> currentInventory = new List<string>();

    [Header("Giao diện UI")]
    // Tham chiếu đến đối tượng Text UI để hiển thị kho đồ (Tùy chọn)
    // Nếu bạn đang dùng TextMeshPro, hãy đổi thành TMPro.TextMeshProUGUI
    public Text inventoryDisplay;

    // --- Khởi tạo (Awake) ---
    void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;
            // Nếu bạn muốn đối tượng này tồn tại qua các Scene:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Hủy đối tượng nếu đã có instance khác
            Destroy(gameObject);
        }
    }

    // --- Phương thức Thêm Vật phẩm ---

    // 1. Phương thức chính để thêm một vật phẩm
    public void AddItem(string itemName)
    {
        currentInventory.Add(itemName);
        Debug.Log($"[Inventory] Đã nhặt: {itemName}");

        // Cập nhật giao diện UI sau khi thêm vật phẩm
        UpdateInventoryUI();
    }

    // 2. Phương thức nhận danh sách vật phẩm từ LootableObject
    public void AddItemsFromLoot(List<LootableObject.LootItem> items)
    {
        foreach (var item in items)
        {
            // Chỉ thêm tên vật phẩm vào kho đồ đơn giản
            AddItem(item.itemName);
        }
    }

    // --- Cập nhật Giao diện UI ---

    private void UpdateInventoryUI()
    {
        // Thoát nếu không có Text UI được gán
        if (inventoryDisplay == null) return;

        string display = "--- KHO ĐỒ ---\n";

        // Sử dụng Dictionary để đếm số lượng mỗi loại vật phẩm
        Dictionary<string, int> itemCounts = new Dictionary<string, int>();

        foreach (string item in currentInventory)
        {
            if (itemCounts.ContainsKey(item))
            {
                itemCounts[item]++;
            }
            else
            {
                itemCounts.Add(item, 1);
            }
        }

        // Tạo chuỗi hiển thị: Tên vật phẩm x Số lượng
        foreach (var pair in itemCounts)
        {
            display += $"{pair.Key} x {pair.Value}\n";
        }

        inventoryDisplay.text = display;
    }
}