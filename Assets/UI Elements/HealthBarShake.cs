using UnityEngine;
using System.Collections;

public class HealthBarShake : MonoBehaviour
{
    RectTransform rect;

    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 6f;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(DoShake());
    }

    IEnumerator DoShake()
    {
        Vector2 originalPos = rect.anchoredPosition;

        float timer = 0f;

        while (timer < shakeDuration)
        {
            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = Random.Range(-shakeMagnitude, shakeMagnitude);

            rect.anchoredPosition = originalPos + new Vector2(x, y);

            timer += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = originalPos;
    }
}