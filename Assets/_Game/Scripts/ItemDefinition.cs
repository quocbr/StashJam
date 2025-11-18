using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    public int id;                 // ID duy nhất, dùng trong level (itemIds)

    public string displayName;     // Tên đầy đủ để đọc cho dễ (ex: "Golden Apple")
    public string shortCode;       // Mã ngắn để hiển thị trong ô (ex: "GA")

    [Header("Visual (Editor only / debug)")]
    public Color color = Color.gray;  // Màu để vẽ thanh trong Level Editor
    public Sprite icon;               // (optional) dùng sau nếu muốn hiển thị icon
}