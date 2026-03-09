using UnityEngine;
using UnityEngine.UI;

public class HealthBarDamage : MonoBehaviour
{
    public Image greenBar;
    public Image whiteBar;

    public float delay = 0.4f;
    public float shrinkSpeed = 1.5f;

    float timer;

    void Update()
    {
        if (whiteBar.fillAmount > greenBar.fillAmount)
        {
            timer += Time.deltaTime;

            if (timer >= delay)
            {
                whiteBar.fillAmount = Mathf.MoveTowards(
                    whiteBar.fillAmount,
                    greenBar.fillAmount,
                    shrinkSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            timer = 0f;
            whiteBar.fillAmount = greenBar.fillAmount;
        }
    }
}