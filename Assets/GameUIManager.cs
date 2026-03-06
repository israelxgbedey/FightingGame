using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    private Canvas canvas;

    public Sprite healthBarSprite;

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
        GameObject container = new GameObject(isPlayerOne ? "P1_Health" : "P2_Health");
        container.transform.SetParent(canvas.transform);

        RectTransform rect = container.AddComponent<RectTransform>();

        rect.sizeDelta = new Vector2(500, 50);

        if (isPlayerOne)
        {
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-260, -60);
        }
        else
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(260, -60);
        }

        // Attach shake system
        container.AddComponent<HealthBarShake>();

        // Background
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(container.transform);

        Image bgImage = bg.AddComponent<Image>();
        bgImage.sprite = healthBarSprite;
        bgImage.type = Image.Type.Sliced;
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
        fillImage.sprite = healthBarSprite;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 1f;
        fillImage.color = Color.green;

        if (isPlayerOne)
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        else
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Right;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Debug.Log($"Created health bar for {(isPlayerOne ? "Player 1" : "Player 2")}");

        return fillImage;
    }

    // Method to trigger shake
    public void ShakeHealthBar(Image healthBar)
    {
        if (healthBar == null) return;

        HealthBarShake shake = healthBar.GetComponentInParent<HealthBarShake>();

        if (shake != null)
        {
            shake.Shake();
        }
    }
}