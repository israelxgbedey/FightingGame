using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    private Canvas canvas;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CreateCanvas();
    }

    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("GameUI");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
    }

    public Image CreateHealthBar(bool isPlayerOne)
    {
        // Container GameObject
        GameObject container = new GameObject(isPlayerOne ? "P1_Health" : "P2_Health");
        container.transform.SetParent(canvas.transform);
        RectTransform rect = container.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 25);

        if (isPlayerOne)
        {
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-160, -40);
        }
        else
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(160, -40);
        }

        // Background
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(container.transform);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = Color.black;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(bg.transform);
        Image fillImage = fill.AddComponent<Image>();

        // IMPORTANT: Set a default sprite for the fill
        // Create a simple white texture if none exists
        Texture2D whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
        Sprite whiteSprite = Sprite.Create(whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        fillImage.sprite = whiteSprite;

        fillImage.color = Color.green;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 1f; // Start at full

        // Set fill origin based on player
        if (!isPlayerOne)
        {
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Right; // Player 2 (left side)
        }
        else
        {
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left; // Player 1 (right side)
        }

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // Verify the fill image is properly configured
        Debug.Log($"Created health bar for {(isPlayerOne ? "Player 1" : "Player 2")} - Type: {fillImage.type}, FillMethod: {fillImage.fillMethod}");

        return fillImage;
    }
}