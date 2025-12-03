using UnityEngine;
using System.Collections.Generic;

public class LootableObject : MonoBehaviour
{
    // Cấu trúc đơn giản để đại diện cho một vật phẩm (Item)
    [System.Serializable]
    public struct LootItem
    {
        public string itemName;
        // Có thể thêm các thuộc tính khác như icon, số lượng, v.v.
        // public int quantity; 
    }

    // Danh sách các vật phẩm mà đối tượng này chứa
    public List<LootItem> itemsToLoot = new List<LootItem>();

    // Vùng nhìn thấy trên màn hình để thông báo người chơi có thể loot
    private bool playerIsNearby = false;

    // Gán tag cho đối tượng có thể loot. Cần có Collider với isTrigger = true.
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là người chơi không
        if (other.CompareTag("Player"))
        {
            playerIsNearby = true;
            Debug.Log("Nhấn E để loot " + gameObject.name);
            // Có thể hiện UI thông báo tại đây
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = false;
            Debug.Log("Người chơi đã rời xa " + gameObject.name);
            // Ẩn UI thông báo tại đây
        }
    }

    // Phương thức công khai để người chơi gọi khi họ muốn loot
    public List<LootItem> GetLoot()
    {
        // Trả về danh sách vật phẩm
        return itemsToLoot;
    }

    // Phương thức công khai để xóa đối tượng sau khi đã loot
    public void DestroyAfterLoot()
    {
        // Có thể thêm hiệu ứng (particles) hoặc animation mở rương trước khi hủy
        Destroy(gameObject);
    }

    // Thuộc tính để kiểm tra từ bên ngoài (script PlayerLooter)
    public bool CanBeLooted()
    {
        return playerIsNearby;
    }
}