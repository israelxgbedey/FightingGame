using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    private Canvas canvas;

    public Sprite healthBarSprite;

    private Image player1Special;
    private Image player2Special;

    public float specialBarWidth = 400f;   // Width of the special bar
    public Sprite specialBarSprite;        // Assign a custom sprite for the special bar

    private float p1SpecialValue = 0f;
    private float p2SpecialValue = 0f;
    private float maxSpecial = 100f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CreateCanvas();
    }

 void Update()
{
    // Player 1 test
    if (Input.GetKeyDown(KeyCode.Alpha0))
        ChangeSpecial(true, 10);

    if (Input.GetKeyDown(KeyCode.Alpha9))
        ChangeSpecial(true, -10);

    // Player 2 test
    if (Input.GetKeyDown(KeyCode.Alpha8))
        ChangeSpecial(false, 10);

    if (Input.GetKeyDown(KeyCode.Alpha7))
        ChangeSpecial(false, -10);
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

        // Increased height so special meter fits
        rect.sizeDelta = new Vector2(500, 70);

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

        container.AddComponent<HealthBarShake>();
        HealthBarDamage damageSystem = container.AddComponent<HealthBarDamage>();

        // ======================
        // HEALTH BAR HOLDER
        // ======================

        GameObject barHolder = new GameObject("HealthBar");
        barHolder.transform.SetParent(container.transform);

        RectTransform holderRect = barHolder.AddComponent<RectTransform>();
        holderRect.anchorMin = new Vector2(0, 1);
        holderRect.anchorMax = new Vector2(1, 1);
        holderRect.pivot = new Vector2(0.5f, 1);
        holderRect.sizeDelta = new Vector2(0, 50);
        holderRect.anchoredPosition = Vector2.zero;

        // ======================
        // BACKGROUND
        // ======================

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(barHolder.transform);

        Image bgImage = bg.AddComponent<Image>();
        bgImage.sprite = healthBarSprite;
        bgImage.type = Image.Type.Sliced;
        bgImage.color = Color.black;

        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // ======================
        // WHITE DAMAGE BAR
        // ======================

        GameObject damage = new GameObject("DamageFill");
        damage.transform.SetParent(bg.transform);

        Image damageImage = damage.AddComponent<Image>();
        damageImage.sprite = healthBarSprite;
        damageImage.type = Image.Type.Filled;
        damageImage.fillMethod = Image.FillMethod.Horizontal;
        damageImage.fillAmount = 1f;
        damageImage.color = Color.white;

        if (isPlayerOne)
            damageImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        else
            damageImage.fillOrigin = (int)Image.OriginHorizontal.Right;

        RectTransform damageRect = damage.GetComponent<RectTransform>();
        damageRect.anchorMin = Vector2.zero;
        damageRect.anchorMax = Vector2.one;
        damageRect.offsetMin = Vector2.zero;
        damageRect.offsetMax = Vector2.zero;

        // ======================
        // GREEN HEALTH BAR
        // ======================

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

        damageSystem.greenBar = fillImage;
        damageSystem.whiteBar = damageImage;

        // ======================
        // SPECIAL METER
        // ======================

        CreateSpecialMeter(container.transform, isPlayerOne);

        return fillImage;
    }

void CreateSpecialMeter(Transform parent, bool isPlayerOne)
{
    GameObject specialBG = new GameObject("SpecialBG");
    specialBG.transform.SetParent(parent);

    RectTransform bgRect = specialBG.AddComponent<RectTransform>();
    bgRect.anchorMin = new Vector2(0, 1);
    bgRect.anchorMax = new Vector2(0, 1);
    bgRect.pivot = new Vector2(0, 1);

    // ✅ Set the width of the special bar here
    bgRect.sizeDelta = new Vector2(specialBarWidth, 12);
    bgRect.anchoredPosition = new Vector2(0, -55);

    Image bgImage = specialBG.AddComponent<Image>();
    bgImage.color = Color.black;

    GameObject fill = new GameObject("SpecialFill");
    fill.transform.SetParent(specialBG.transform);

    Image meter = fill.AddComponent<Image>();
    meter.sprite = specialBarSprite != null ? specialBarSprite : healthBarSprite; // Use custom sprite
    meter.type = Image.Type.Filled;
    meter.fillMethod = Image.FillMethod.Horizontal;
    meter.fillAmount = 0f;
    meter.color = Color.blue;

    // ✅ Both fill LEFT → RIGHT
    meter.fillOrigin = (int)Image.OriginHorizontal.Left;

    RectTransform fillRect = fill.GetComponent<RectTransform>();
    fillRect.anchorMin = Vector2.zero;
    fillRect.anchorMax = Vector2.one;
    fillRect.offsetMin = Vector2.zero;
    fillRect.offsetMax = Vector2.zero;
    fillRect.pivot = new Vector2(0, 0.5f); // ensure fill grows from left

    if (isPlayerOne)
        player1Special = meter;
    else
        player2Special = meter;
}

    public void ChangeSpecial(bool isPlayerOne, float amount)
    {
        if (isPlayerOne)
        {
            p1SpecialValue = Mathf.Clamp(p1SpecialValue + amount, 0, maxSpecial);
            if (player1Special != null)
                player1Special.fillAmount = p1SpecialValue / maxSpecial;
        }
        else
        {
            p2SpecialValue = Mathf.Clamp(p2SpecialValue + amount, 0, maxSpecial);
            if (player2Special != null)
                player2Special.fillAmount = p2SpecialValue / maxSpecial;
        }
    }

    public void ShakeHealthBar(Image healthBar)
    {
        if (healthBar == null) return;

        HealthBarShake shake = healthBar.GetComponentInParent<HealthBarShake>();

        if (shake != null)
            shake.Shake();
    }
}