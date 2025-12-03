using UnityEngine;

public class PlayerLooter : MonoBehaviour
{
    // Tham chiếu đến vật thể có thể loot đang ở gần người chơi
    private LootableObject currentLootable = null;

    // PHẢI có một Collider (với Is Trigger = true) trên người chơi,
    // cùng kích thước với collider của LootableObject, hoặc to hơn.

    private void Update()
    {
        // Kiểm tra xem nút E có được nhấn không
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Kiểm tra xem người chơi có đang ở gần vật thể nào có thể loot không
            if (currentLootable != null && currentLootable.CanBeLooted())
            {
                // Gọi phương thức để thực hiện quá trình loot
                Loot(currentLootable);
            }
        }
    }

    // Xử lý va chạm để xác định vật thể có thể loot nào đang ở gần
    private void OnTriggerEnter2D(Collider2D other)
    {
        LootableObject loot = other.GetComponent<LootableObject>();
        if (loot != null)
        {
            currentLootable = loot;
            // Lưu ý: LootableObject.cs cũng tự xử lý thông báo, nhưng bạn có thể
            // dùng sự kiện (Events) để giao tiếp phức tạp hơn.
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        LootableObject loot = other.GetComponent<LootableObject>();
        if (loot != null && currentLootable == loot)
        {
            currentLootable = null;
        }
    }

    // Phương thức chính để lấy vật phẩm và thêm vào inventory (kho đồ) của người chơi
    private void Loot(LootableObject lootSource)
    {
        var items = lootSource.GetLoot();

        Debug.Log("Đang loot từ " + lootSource.gameObject.name + ". Số vật phẩm: " + items.Count);

        // --- BƯỚC QUAN TRỌNG: Thêm vật phẩm vào Inventory THỰC TẾ ---
        // Trong một game thực tế, bạn sẽ gọi một phương thức trên InventoryManager
        // hoặc PlayerInventory để thêm các vật phẩm này vào kho đồ của người chơi.
        // Ví dụ: InventoryManager.Instance.AddItems(items);

        foreach (var item in items)
        {
            Debug.Log("Đã nhặt: " + item.itemName);
        }
        // -----------------------------------------------------------------

        // Hủy đối tượng có thể loot sau khi đã lấy hết đồ
        lootSource.DestroyAfterLoot();
        currentLootable = null; // Đảm bảo tham chiếu được đặt lại
    }
}