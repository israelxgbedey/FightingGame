using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 1f;

    public float visibleDuration = 3f;   // Stay visible this long
    public float fadeDuration = 1f;      // How long fade takes

    private TMP_Text text;
    private float timer = 0f;
    private bool isFading = false;

    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        // Float upward always
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        // After visible duration, start fading
        if (timer >= visibleDuration)
        {
            isFading = true;
        }

        // Handle fade
        if (isFading)
        {
            float fadeAmount = Time.deltaTime / fadeDuration;

            Color c = text.color;
            c.a -= fadeAmount;
            text.color = c;

            // Destroy when fully transparent
            if (c.a <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}