using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    [SerializeField] private Image[] hearts;
    [SerializeField] private string breakTrigger = "Break";

    public void SetHealth(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            Animator heartAnimator = hearts[i].GetComponent<Animator>();

            if (hearts.Length - i <= currentHealth)
            {
                hearts[i].enabled = true;
            }
            else
            {
                if (heartAnimator != null)
                    heartAnimator.SetTrigger(breakTrigger);
            }
        }
    }
}