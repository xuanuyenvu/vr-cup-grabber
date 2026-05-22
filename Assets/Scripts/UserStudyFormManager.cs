using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using TMPro;
using System;

[System.Serializable]
public struct QuestionUI
{
    public int id;
    public GameObject panel;
    public Slider slider;
    public TextMeshProUGUI buttonText;
}


public class UserStudyFormManager : MonoBehaviour
{
    public enum ExperimentType
    {
        ColorTaste,
        ColorTastePureWater,
        VisualFruitScentPureWater
    }
    public enum SmellType
    {
        Sweet,
        Sour,
        Bitter,
        Neutral,
        Strawberry,
        Lemon,
        Coffee,
        None
    }
    public enum TasteType
    {
        Sweet,
        Sour,
        Bitter,
        Neutral,
        None
    }

    [Header("Tutorial Form")]
    [SerializeField] private GameObject tutoFormUI;
    [SerializeField] private List<QuestionUI> tutoQuestions;

    [Space]
    [Header("User Study Form")]
    [SerializeField] private GameObject userStudyFormUI;
    [SerializeField] private QuestionUI questions1;
    [SerializeField] private List<QuestionUI> questions345;
    [SerializeField] private List<QuestionUI> pqQuestions;
    [SerializeField] private UserStudyManager userStudyManager;

    private List<QuestionUI> currentQuestions = new List<QuestionUI>();
    private List<float> questionValues = new List<float>(new float[16]);
    private int currentPage = -1;


    [HideInInspector] public ExperimentType experimentType = ExperimentType.ColorTaste;
    [HideInInspector] public string userId = "no-data";

    [HideInInspector] public SmellType smellType = SmellType.Sweet;
    [HideInInspector] public TasteType tasteType = TasteType.Sweet;
    [HideInInspector] public LiquidColor liquidColor = LiquidColor.Red;
    [HideInInspector] public string fruitType = "";



    void Start()
    {
        ResetValues();
        HideUserStudyFormUI();
    }

    public void ShowTutorialFormUI()
    {
        tutoFormUI.SetActive(true);
        foreach (var q in tutoQuestions)
        {
            q.panel.SetActive(false);
            q.slider.value = 0.1111111f; 
        }

        currentPage = 0;
        tutoQuestions[0].buttonText.text = "Tiếp tục";
        tutoQuestions[0].panel.SetActive(true);
    }

    public void NextTutorialPage()
    {
        if (!IsValid(tutoQuestions[currentPage])) return;
        tutoQuestions[currentPage].panel.SetActive(false);
        tutoQuestions[currentPage].slider.value = 0.1111111f;
        currentPage++;
        if (currentPage < tutoQuestions.Count)
        {
            if (currentPage == tutoQuestions.Count - 1)
            {
                tutoQuestions[currentPage].buttonText.text = "Gửi";
            }
            else
            {
                tutoQuestions[currentPage].buttonText.text = "Tiếp tục";
            }
            tutoQuestions[currentPage].panel.SetActive(true);
        }
        else
        {
            HideTutorialFormUI();
        }
    }

    public void HideTutorialFormUI()
    {
        tutoFormUI.SetActive(false);
        currentPage = -1;
        foreach (var q in tutoQuestions)
        {
            q.panel.SetActive(false);
            q.slider.value = 0.1111111f;
        }
    }

    public void ShowUserStudyFormUI()
    {
        userStudyFormUI.SetActive(true);
        currentPage = 0;
        currentQuestions.Clear();

        // 1. Q1 Like always first
        currentQuestions.Add(questions1);

        // 2. Shuffle taste questions (Fisher-Yates)
        for (int i = questions345.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            var temp = questions345[i];
            questions345[i] = questions345[randomIndex];
            questions345[randomIndex] = temp;
        }

        // 3. Add shuffled taste (preserve shuffle in original list for reference)
        foreach (var q in questions345)
        {
            q.panel.SetActive(false);
            q.slider.value = 0.1111111f;
            currentQuestions.Add(q);
        }

        // 4. Add PQ questions in fixed order (not shuffled)
        foreach (var q in pqQuestions)
        {
            q.panel.SetActive(false);
            q.slider.value = 0.1111111f;
            currentQuestions.Add(q);
        }

        // Show first question
        currentQuestions[0].panel.SetActive(true);
        currentQuestions[0].buttonText.text = "Tiếp tục";
    }

    public void HideUserStudyFormUI()
    {
        ResetValues();
        userStudyFormUI.SetActive(false);

        questions1.panel.SetActive(false);
        foreach (var q in questions345)
        {
            q.panel.SetActive(false);
        }
        foreach (var q in pqQuestions)
        {
            q.panel.SetActive(false);
        }

        userStudyManager.isOpenForm = false;
    }

    public void NextPage()
    {
        if (currentPage < 0 || currentPage >= currentQuestions.Count) return;

        QuestionUI current = currentQuestions[currentPage];

        if (!IsValid(current)) return;

        // Save slider value by question id
        questionValues[current.id - 1] = current.slider.value;

        // Hide current panel
        current.panel.SetActive(false);
        current.slider.value = 0.1111111f;

        currentPage++;

        if (currentPage < currentQuestions.Count)
        {
            QuestionUI next = currentQuestions[currentPage];
            next.buttonText.text = (currentPage == currentQuestions.Count - 1) ? "Gửi" : "Tiếp tục";
            next.panel.SetActive(true);
        }
        else
        {
            SubmitForm();
        }
    }

    private bool IsValid(QuestionUI question)
    {
        return question.slider.value != 0.1111111f; // Kiểm tra xem giá trị của slider có khác giá trị mặc định không
    }

    private void SubmitForm()
    {
        // Kiểm tra và tạo thư mục UserData nếu chưa tồn tại
        string userDataFolder = Path.Combine(Application.dataPath, "UserData");
        if (!Directory.Exists(userDataFolder))
        {
            Directory.CreateDirectory(userDataFolder);
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string questionsCsv = string.Join(";", questionValues);
        string filePath, csvData, csvHeader;

        if (experimentType == ExperimentType.ColorTaste)
        {
            filePath = Path.Combine(userDataFolder, "user_study_color_taste.csv");
            csvHeader = "sep=;\nSubmitTime;UserId;ExperimentType;TasteType;LiquidColor;Q1-Like;Q2-Sweet;Q3-Bitter;Q4-Sour;Q5-Salty;Q6-Umami\n";

            csvData = $"{timestamp};{userId};{experimentType};{tasteType};{liquidColor};{questionsCsv}\n";
        }
        else if (experimentType == ExperimentType.VisualFruitScentPureWater)
        {
            filePath = Path.Combine(userDataFolder, "user_study_vfs_purewater.csv");
            csvHeader = "sep=;\nSubmitTime;UserId;ExperimentType;FruitType;SmellType;LiquidColor;Q1-Like;Q2-Sweet;Q3-Bitter;Q4-Sour;Q5-Salty;Q6-Umami\n";

            csvData = $"{timestamp};{userId};{experimentType};{fruitType};{smellType};{liquidColor};{questionsCsv}\n";
        }
        else
        {
            filePath = Path.Combine(userDataFolder, "user_study_color_smell_purewater.csv");
            csvHeader = "sep=;\nSubmitTime;UserId;ExperimentType;LiquidColor;SmellType;Q1-Like;Q2-Sweet;Q3-Bitter;Q4-Sour;Q5-Salty;Q6-Umami\n";

            csvData = $"{timestamp};{userId};{experimentType};{liquidColor};{smellType};{questionsCsv}\n";
        }


        try
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, csvHeader);
            }

            File.AppendAllText(filePath, csvData);
            Debug.Log($"Form submitted successfully! File saved at: {filePath}");

            HideUserStudyFormUI();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save form: {ex.Message}");
        }
    }

    private void ResetValues()
    {
        currentPage = -1;
        currentQuestions.Clear();

        questions1.slider.value = 0.1111111f;
        foreach (var q in questions345)
        {
            q.slider.value = 0.1111111f;
        }
        foreach (var q in pqQuestions)
        {
            q.slider.value = 0.1111111f;
        }

        for (int i = 0; i < questionValues.Count; i++)
        {
            questionValues[i] = 0f;
        }
    }
}
