using UnityEngine;

public abstract class UIScene : MonoBehaviour
{
    public Sprite[] largeFontSprites;
    public Sprite[] smallFontSprites;
    public Sprite[] playerSprites;
    public Sprite[] blockSprites;
    public GameObject fontPrefab;

    protected GameObject PlaceUI(int x, int y, Sprite sprite, int sortingOrder = 0)
    {
        var worldPos = Grid.Current.ToWorld(new Vector2Int(x, y));
        var dummy = Instantiate(fontPrefab, worldPos, Quaternion.identity);

        var renderer = dummy.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;

        return dummy;
    }

    protected void PlaceText(int x, int y, string text)
    {
        for (int i = 0; i < text.Length; i++)
            PlaceUI(x + i, y, smallFontSprites[text[i] % smallFontSprites.Length]);
    }

    protected void PlaceLargeText(int x, int y, string text)
    {
        for (int i = 0; i < text.Length; i++)
            PlaceUI(x + i, y, largeFontSprites[text[i] % largeFontSprites.Length]);
    }
}
