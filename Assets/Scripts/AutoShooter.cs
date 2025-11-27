using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    public GameObject bulletPrefab; 
    public float fireRate = 0.5f;
    public float bulletSpeed = 10f;

    private float fireCooldown;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            Transform target = FindClosestEnemy();
            if (target != null)
            {
                Shoot(target.position);
                fireCooldown = fireRate;
            }
        }
    }

    void Shoot(Vector3 targetPos)
    {
        // spawn peluru
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // arahkan ke musuh
        Vector2 direction = (targetPos - transform.position).normalized;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
    }

    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(currentPos, enemy.transform.position);
            if (dist < minDist)
            {
                closest = enemy.transform;
                minDist = dist;
            }
        }
        return closest;
    }
}
