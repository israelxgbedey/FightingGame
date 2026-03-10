using UnityEngine;

public class CounterShieldAnimation : MonoBehaviour
{
    [Header("Shield Frames")]
    public Sprite[] shieldFrames;
    public float frameRate = 12f;

    private SpriteRenderer spriteRenderer;
    private float frameTimer;
    private int frameIndex;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (shieldFrames == null || shieldFrames.Length == 0) return;

        frameTimer += Time.deltaTime;
        float frameTime = 1f / Mathf.Max(1f, frameRate);

        if (frameTimer >= frameTime)
        {
            frameTimer -= frameTime;

            frameIndex++;
            if (frameIndex >= shieldFrames.Length)
                frameIndex = 0;

            spriteRenderer.sprite = shieldFrames[frameIndex];
        }
    }
}