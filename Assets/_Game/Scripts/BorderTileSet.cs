using UnityEngine;

[CreateAssetMenu(menuName = "Game/Board/Border Tile Set")]
public class BorderTileSet : ScriptableObject
{
    /*
     * sprites[mask]
     * mask = Up(1) | Right(2) | Down(4) | Left(8)
     * Ví dụ:
     * 0  = ô lẻ loi (4 cạnh đều có viền)
     * 1  = có hàng xóm phía trên (không vẽ viền phía trên)
     * 3  = có trên + phải, v.v...
     * 15 = 4 phía đều có box (ô ở giữa) -> thường vẽ tile "flat"
     */
    public Sprite[] sprites = new Sprite[16];

    public Sprite GetSprite(int mask)
    {
        if (mask < 0 || mask >= sprites.Length) return null;
        return sprites[mask];
    }
}