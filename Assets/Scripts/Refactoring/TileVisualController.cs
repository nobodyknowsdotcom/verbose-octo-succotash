using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileVisualController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite source;

    public void HighlightTile()
    {
        Component comp;
        if (!TryGetComponent(typeof(SpriteRenderer), out comp))
        {
            spriteRenderer = gameObject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        }
        else
        {
            spriteRenderer = comp as SpriteRenderer;
        }
        spriteRenderer.sortingOrder = 1;
        spriteRenderer.sprite = source;
        var color = Color.red;
        color.a = 0.25f;
        spriteRenderer.color = color;
    }
}
