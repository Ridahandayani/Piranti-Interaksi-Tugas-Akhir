using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;     // Prefab musuh
    public Transform player;           // Referensi player
    public float waveInterval = 20f;   // Jeda antar wave
    public int enemiesPerWave = 5;     // Jumlah musuh per wave
    private float timer;

    void Start()
    {
        timer = waveInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnWave();
            timer = waveInterval;
        }
    }

    void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            Vector2 screenPosition = RandomEdgePosition();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            Instantiate(enemyPrefab, worldPosition, Quaternion.identity);
        }

        // (Opsional) jumlah musuh bertambah tiap wave biar makin susah
        enemiesPerWave += 2;
    }

    Vector2 RandomEdgePosition()
    {
        float screenX, screenY;
        int side = Random.Range(0, 4); // 0 = kiri, 1 = kanan, 2 = atas, 3 = bawah

        if (side == 0) // kiri
        {
            screenX = 0;
            screenY = Random.Range(0, Screen.height);
        }
        else if (side == 1) // kanan
        {
            screenX = Screen.width;
            screenY = Random.Range(0, Screen.height);
        }
        else if (side == 2) // atas
        {
            screenX = Random.Range(0, Screen.width);
            screenY = Screen.height;
        }
        else // bawah
        {
            screenX = Random.Range(0, Screen.width);
            screenY = 0;
        }

        return new Vector2(screenX, screenY);
    }
}
