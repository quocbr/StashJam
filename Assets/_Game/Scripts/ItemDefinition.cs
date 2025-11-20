using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")] public int id;

    public string displayName;
    public string shortCode;

    [Header("Visual (Editor only / debug)")]
    public Color color = Color.gray;

    public Sprite icon;
}