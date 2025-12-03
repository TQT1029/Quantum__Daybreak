using UnityEngine;

// Tạo menu để click chuột phải tạo data dễ dàng trong Unity
[CreateAssetMenu(fileName = "NewCharacterRole", menuName = "Game/Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Thông Tin Cơ Bản")]
    public string RoleName; // Tên vai trò (Kỹ Sư Sinh Học / Nhà Nghiên Cứu Thần Kinh)
    [TextArea(3, 5)]
    public string Description; // Mô tả xuất thân và điểm mạnh

    [Header("Chỉ Số Sinh Tồn (Survival Stats)")]
    [Tooltip("Máu tối đa của nhân vật")]
    public float MaxHealth = 100f;

    [Tooltip("Tốc độ di chuyển cơ bản")]
    public float MoveSpeed = 5f;

    [Tooltip("Khả năng kháng độc và bệnh dịch từ môi trường tiền sử ")]
    [Range(0f, 100f)]
    public float BioResistance;

    [Header("Chuyên Môn (Specializations)")]
    [Tooltip("Hiệu quả khi sử dụng Module Thần Kinh để thuần hóa khủng long ")]
    [Range(0f, 2.0f)]
    public float TamingEfficiency; // 1.0 là bình thường, cao hơn là tốt hơn

    [Tooltip("Khả năng chế tạo và sửa chữa thiết bị công nghệ/Neural Implant [cite: 23]")]
    [Range(0f, 100f)]
    public float TechSkill;

    [Tooltip("Khả năng thu thập mẫu gen và chế thuốc cứu thương ")]
    [Range(0f, 100f)]
    public float MedicalSkill;

    [Tooltip("Khoảng các tương tác")]
    public float InteractionRange = 3f;

    [Header("Trang Bị Khởi Đầu")]
    public string[] StartingItems; // Ví dụ: "Scanner", "Basic Knife", "Medkit"
}