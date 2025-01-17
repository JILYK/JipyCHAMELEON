using UnityEngine;
using TMPro;

public class TrainingManager : MonoBehaviour
{
    private const string TrainingKey = "TrainingCompleted";


    public TextMeshProUGUI trainingText; 

    // Тексты для отображения в процессе обучения
    private string[] trainingMessages = new string[]
    {
        "Hello, friend! I am Jipy, your chameleon, and I will help you get familiar with this game.",
        "Sticky Sphere. Be careful! If you click on the sticky sphere, it will pull you towards itself! Don’t relax, it might trap you. Here's a tip: you can release it anytime and fly away.",
        "Bombs. Uh-oh! Bombs can drain your health, so don't let them get too close!",
        "Hearts. If your health starts dropping, look for hearts. Collect them to recover your strength and keep going!",
        "Goal. Your task is to climb as high as possible! The higher you go, the more points you will score. Don’t miss your chance to become a leader!"
    };

    private int currentMessageIndex = 0;


    public void OnEnable()
    {
        if (IsTrainingCompleted())
        {
            gameObject.SetActive(false);
            Debug.Log("Обучение уже пройдено.");
            return;
        }


        Debug.Log("Начало обучения...");
        UpdateTrainingText();
    }


    private void CompleteTraining()
    {
        PlayerPrefs.SetInt(TrainingKey, 1);
        PlayerPrefs.Save();
        Debug.Log("Обучение завершено!");

        gameObject.SetActive(false);
    }


    private void UpdateTrainingText()
    {
        if (currentMessageIndex < trainingMessages.Length)
        {
            trainingText.text = trainingMessages[currentMessageIndex];
        }
        else
        {
            CompleteTraining();
        }
    }


    public void NextTrainingMessage()
    {
        if (currentMessageIndex < trainingMessages.Length - 1)
        {
            currentMessageIndex++;
            UpdateTrainingText();
        }
        else
        {
            CompleteTraining();
        }
    }


    public void ResetTraining()
    {
        // Сбрасываем состояние обучения
        PlayerPrefs.SetInt(TrainingKey, 0);
        PlayerPrefs.Save();
        Debug.Log("Обучение сброшено.");
        currentMessageIndex = 0;
        UpdateTrainingText();
    }


    public bool IsTrainingCompleted()
    {
        return PlayerPrefs.GetInt(TrainingKey, 0) == 1;
    }
}