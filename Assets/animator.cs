using UnityEngine;

public class FrameAnimator : MonoBehaviour
{
    [Header("Animation Frames (PNG Images)")]
    public Sprite[] frames;

    [Header("Animation Settings")]
    public float framesPerSecond = 12f;
    public bool loop = true;

    private SpriteRenderer sr;
    private int frameIndex = 0;
    private float timer = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (frames != null && frames.Length > 0)
        {
            sr.sprite = frames[0]; // start with the first frame
        }
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;

        float frameTime = 1f / Mathf.Max(1f, framesPerSecond);
        
        if (timer >= frameTime)
        {
            timer -= frameTime;
            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                if (loop) frameIndex = 0;
                else return;  // stop at last frame
            }

            sr.sprite = frames[frameIndex];
        }
    }
}
