using UnityEngine;

public class HealthBarShake : MonoBehaviour
{
    RectTransform rect;

    Vector2 originalPosition;

    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 6f;

    float timer = 0f;
    bool shaking = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPosition = rect.anchoredPosition;
    }

    void Update()
    {
        if (!shaking) return;

        timer += Time.deltaTime;

        if (timer < shakeDuration)
        {
            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = Random.Range(-shakeMagnitude, shakeMagnitude);

            rect.anchoredPosition = originalPosition + new Vector2(x, y);
        }
        else
        {
            rect.anchoredPosition = originalPosition;
            shaking = false;
        }
    }

    public void Shake()
    {
        timer = 0f;
        shaking = true;
    }
}