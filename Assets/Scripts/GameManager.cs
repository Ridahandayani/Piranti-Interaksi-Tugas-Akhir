using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float gameTime = 60f;   // durasi game 1 menit
    private float currentTime;

    public Text timerText;
    public GameObject winUI;

    private bool gameEnded = false;

    void Start()
    {
        currentTime = gameTime;
        if (winUI != null) winUI.SetActive(false);
    }

    void Update()
    {
        if (gameEnded) return;

        // kurangi waktu
        currentTime -= Time.deltaTime;

        // update text UI
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.Ceil(currentTime).ToString();
        }

        // jika waktunya habis → menang
        if (currentTime <= 0)
        {
            Win();
        }
    }

    void Win()
    {
        gameEnded = true;
        if (winUI != null) winUI.SetActive(true);

        Time.timeScale = 0f; // pause game
    }
}
