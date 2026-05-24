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
    private HashSet<int> answeredQuestionIds = new HashSet<int>();
    private HashSet<Slider> registeredAnswerSliders = new HashSet<Slider>();
    private int currentPage = -1;

    // Slider default values — user must drag to activate, default sits near centre
    private const float NeutralVasDefaultValue = 0.111f;   // Q1 -50..50 midpoint
    private const float MidScaleDefaultValue = 50.111f;     // 0..100 scales (taste + PQ)

    private float GetDefaultSliderValue(QuestionUI question)
    {
        return question.id == 1 ? NeutralVasDefaultValue : MidScaleDefaultValue;
    }

    /// <summary>Reset slider to its scale-appropriate default without marking as answered.</summary>
    private void ResetSliderWithoutAnswer(QuestionUI question)
    {
        if (question.slider != null)
            question.slider.SetValueWithoutNotify(GetDefaultSliderValue(question));
    }

    /// <summary>Add a one-time onValueChanged listener that marks the question as answered.</summary>
    private void RegisterAnswerListener(QuestionUI question)
    {
        if (question.slider == null || registeredAnswerSliders.Contains(question.slider)) return;
        int capturedId = question.id;
        question.slider.onValueChanged.AddListener(_ => answeredQuestionIds.Add(capturedId));
        registeredAnswerSliders.Add(question.slider);
    }


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
        answeredQuestionIds.Clear();
        tutoFormUI.SetActive(true);
        foreach (var q in tutoQuestions)
        {
            q.panel.SetActive(false);
            if (q.slider != null)
            {
                q.slider.value = 0f;
                int capturedId = q.id;
                q.slider.onValueChanged.AddListener(_ => answeredQuestionIds.Add(capturedId));
            }
        }

        currentPage = 0;
        tutoQuestions[0].buttonText.text = "Tiếp tục";
        tutoQuestions[0].panel.SetActive(true);
    }

    public void NextTutorialPage()
    {
        if (!IsValid(tutoQuestions[currentPage])) return;
        tutoQuestions[currentPage].panel.SetActive(false);
        if (tutoQuestions[currentPage].slider != null) tutoQuestions[currentPage].slider.value = 0f;
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
        answeredQuestionIds.Clear();
        tutoFormUI.SetActive(false);
        currentPage = -1;
        foreach (var q in tutoQuestions)
        {
            q.panel.SetActive(false);
            if (q.slider != null) q.slider.value = 0f;
        }
    }

    public void ShowUserStudyFormUI()
    {
        answeredQuestionIds.Clear();
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
            ResetSliderWithoutAnswer(q);
            currentQuestions.Add(q);
        }

        // 4. Add PQ questions in fixed order (not shuffled)
        if (pqQuestions != null)
        {
            foreach (var q in pqQuestions)
            {
                q.panel.SetActive(false);
                ResetSliderWithoutAnswer(q);
                currentQuestions.Add(q);
            }
        }

        // Track answered state via slider interaction
        foreach (var q in currentQuestions)
            RegisterAnswerListener(q);

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
        if (pqQuestions != null)
        {
            foreach (var q in pqQuestions)
            {
                q.panel.SetActive(false);
            }
        }

        userStudyManager.isOpenForm = false;
    }

    public void NextPage()
    {
        if (currentPage < 0 || currentPage >= currentQuestions.Count) return;

        QuestionUI current = currentQuestions[currentPage];

        if (!IsValid(current)) return;

        // Validate question id before accessing questionValues
        if (current.id < 1 || current.id > questionValues.Count)
        {
            Debug.LogError($"Invalid question id {current.id} (expected 1-{questionValues.Count})");
            return;
        }

        // Save slider value by question id
        questionValues[current.id - 1] = current.slider.value;

        // Hide current panel
        current.panel.SetActive(false);
        ResetSliderWithoutAnswer(current);

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
        return answeredQuestionIds.Contains(question.id); // Kiểm tra user đã kéo slider hay chưa
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

        string pqHeader = "PQ1-Presence;PQ2-VisualEngagement;PQ3-BeingThere;PQ4-NaturalMovement;PQ5-RealWorldReflection;" +
                          "PQ6-Predictability;PQ7-VisualInspection;PQ8-Immersion;PQ9-Adaptation;PQ10-TaskFocus";

        if (experimentType == ExperimentType.ColorTaste)
        {
            filePath = Path.Combine(userDataFolder, "user_study_color_taste_pq.csv");
            csvHeader = $"sep=;\nSubmitTime;UserId;ExperimentType;TasteType;LiquidColor;Q1-Like;Q2-Sweet;Q3-Bitter;Q4-Sour;Q5-Salty;Q6-Umami;{pqHeader}\n";

            csvData = $"{timestamp};{userId};{experimentType};{tasteType};{liquidColor};{questionsCsv}\n";
        }
        else if (experimentType == ExperimentType.VisualFruitScentPureWater)
        {
            filePath = Path.Combine(userDataFolder, "user_study_vfs_purewater_pq.csv");
            csvHeader = $"sep=;\nSubmitTime;UserId;ExperimentType;FruitType;SmellType;LiquidColor;Q1-Like;Q2-Sweet;Q3-Bitter;Q4-Sour;Q5-Salty;Q6-Umami;{pqHeader}\n";

            csvData = $"{timestamp};{userId};{experimentType};{fruitType};{smellType};{liquidColor};{questionsCsv}\n";
        }
        else
        {
            filePath = Path.Combine(userDataFolder, "user_study_color_smell_purewater_pq.csv");
            csvHeader = $"sep=;\nSubmitTime;UserId;ExperimentType;LiquidColor;SmellType;Q1-Like;Q2-Sweet;Q3-Bitter;Q4-Sour;Q5-Salty;Q6-Umami;{pqHeader}\n";

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
        answeredQuestionIds.Clear();
        currentPage = -1;
        currentQuestions.Clear();

        ResetSliderWithoutAnswer(questions1);
        foreach (var q in questions345)
            ResetSliderWithoutAnswer(q);
        if (pqQuestions != null)
        {
            foreach (var q in pqQuestions)
                ResetSliderWithoutAnswer(q);
        }

        for (int i = 0; i < questionValues.Count; i++)
        {
            questionValues[i] = 0f;
        }
    }
}
