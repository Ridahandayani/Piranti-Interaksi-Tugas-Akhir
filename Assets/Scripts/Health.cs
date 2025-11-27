using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0; // biar nggak minus
            Die();
        }
    }

    void Die()
    {
        if (CompareTag("Enemy"))
        {
            // Drop XP orb kalau enemy mati
            GameObject orb = Resources.Load<GameObject>("XPOrb");
            if (orb != null)
            {
                Instantiate(orb, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
        else if (CompareTag("Player"))
        {
            Debug.Log("Player mati! Game Over.");

            // paksa update health bar jadi 0 dulu
            HealthBar hb = FindObjectOfType<HealthBar>();
            if (hb != null)
            {
                hb.SetHealth(0);
            }

            // sementara player dihancurkan
            Destroy(gameObject);

            // TODO: nanti bisa bikin GameManager untuk munculin UI Game Over
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        Debug.Log(gameObject.name + " healed. HP: " + currentHealth);
    }

    public int GetHealth()
    {
        return currentHealth;
    }
}
