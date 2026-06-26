using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private HeartUI heartUI;

    [SerializeField] private GameManager gameManager;

    private int currentHealth;

    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        heartUI.SetHealth(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        heartUI.SetHealth(currentHealth);

        Debug.Log($"Player damaged. Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("Player died");
            if (gameManager != null)
            {
                gameManager.GameOver();
            }
        }
    }
}
