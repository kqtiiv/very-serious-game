using UnityEngine;

public class BiteHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private bool hasHit;
    private void OnEnable()
    {
        hasHit = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (hasHit)
            return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        hasHit = true;
        playerHealth.TakeDamage(damage);
    }
}