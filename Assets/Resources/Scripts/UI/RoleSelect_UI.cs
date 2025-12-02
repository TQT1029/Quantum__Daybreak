using System; // Dùng để xử lý Enum
using System.Collections.Generic; // Dùng cho List
using TMPro; // Thư viện cho TextMeshPro (Dropdown chuẩn mới)
using UnityEngine;

public class RoleSelect_UI : MonoBehaviour
{
    private TMP_Dropdown roleDropdown => UIManager.Instance.roleDropdown;
    private void Start()
    {

        // Đăng ký sự kiện: Khi giá trị thay đổi -> Gọi hàm OnRoleChanged
        roleDropdown.onValueChanged.AddListener(OnRoleChanged);

        // Set giá trị mặc định ban đầu (để GameManager biết ngay khi game bật)
        OnRoleChanged(roleDropdown.value);
    }

    /// <summary>
    /// Hàm xử lý chính khi người chơi chọn 1 mục trong Dropdown
    /// </summary>
    /// <param name="index">0 = BioEngineer, 1 = NeuroScientist, ...</param>
    public void OnRoleChanged(int index)
    {
        // Gọi sang GameManager để set role dựa trên số thứ tự
        UIManager.Instance.SetRole(index);

        //Debug.Log($"[UI] Đã chọn Role: {(RoleType)index}");
    }
}