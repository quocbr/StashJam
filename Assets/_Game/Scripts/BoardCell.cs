using UnityEngine;

public class BoardCell : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private void Reset()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = sprite;
    }
}