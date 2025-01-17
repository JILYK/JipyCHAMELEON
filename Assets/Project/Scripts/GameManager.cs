using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Синглтон

    [Header("Score Management")] public int currentScore = 0; // Текущий счёт
    public int highScore = 0; // Лучший счёт
    [Header("Best Score UI")] public TextMeshProUGUI bestScoreText;

    [Header("Health Management")] public List<Image> heartIcons = new List<Image>(); // Массив UI Image для сердец
    public Sprite heartOnSprite; // Спрайт для полного сердца
    public Sprite heartOffSprite; // Спрайт для пустого сердца

    [Header("UI Management")] public List<WindowData> windows = new List<WindowData>(); // Список всех окон

    public Button soundButton; // Кнопка звука
    public Sprite soundOnSprite; // Спрайт для включенного звука
    public Sprite soundOffSprite; // Спрайт для выключенного звука
    public TextMeshProUGUI scoreText;
    private bool isSoundOn = true; // Состояние звука (включен/выключен)

    private const string HighScoreKey = "HighScore"; // Ключ для хранения лучшего счёта
    private const string SoundKey = "Sound"; // Ключ для хранения состояния звука

    public void SettingButton(GameObject open)
    {
        if (open.activeSelf)
        {
            open.SetActive(false);
        }
        else
        {
            open.SetActive(true);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }

        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        isSoundOn = PlayerPrefs.GetInt(SoundKey, 1) == 1; 
    }

    void Start()
    {
        scoreText.text = "0";
        UpdateBestScoreUI();
        UpdateSoundButtonSprite(); 
    }

    private void UpdateBestScoreUI()
    {
        if (bestScoreText != null)
        {
            bestScoreText.text = $"Best Score: {highScore}";
        }
    }

    public void AddScore()
    {
        currentScore += 100;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    public void EndGame()
    {
        currentScore = 0;
    }

    public void ToggleWindowObjects(int windowIndex)
    {
        if (windowIndex < 0 || windowIndex >= windows.Count) return;

        foreach (var window in windows)
        {
            foreach (var obj in window.objects)
            {
                obj.SetActive(false);
            }
        }

        foreach (var obj in windows[windowIndex].objects)
        {
            obj.SetActive(true);
        }

        if (windows[windowIndex].title == "Home")
        {
            UpdateBestScoreUI();
        }
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;

        PlayerPrefs.SetInt(SoundKey, isSoundOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateSoundButtonSprite();

        AudioListener.volume = isSoundOn ? 1 : 0;
    }

    private void UpdateSoundButtonSprite()
    {
        if (soundButton != null)
        {
            soundButton.image.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }
    }
}

[System.Serializable]
public class WindowData
{
    public string title; // Заголовок окна
    public List<GameObject> objects = new List<GameObject>(); 
}