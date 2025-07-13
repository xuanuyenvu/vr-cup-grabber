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
        ColorTastePureWater
    }
    public enum SmellType
    {
        Sweet,
        Sour,
        Bitter,
        Neutral
    }
    public enum TasteType
    {
        Sweet,
        Sour,
        Bitter,
        Neutral
    }

    [Header("Tutorial Form")]
    [SerializeField] private GameObject tutoFormUI;
    [SerializeField] private List<QuestionUI> tutoQuestions;

    [Space]
    [Header("User Study Form")]
    [SerializeField] private GameObject userStudyFormUI;
    [SerializeField] private QuestionUI questions1;
    [SerializeField] private List<QuestionUI> questions345;
    [SerializeField] private UserStudyManager userStudyManager;
    private List<float> questionValues = new List<float> { 0f, 0f, 0f, 0f, 0f, 0f };
    private int currentPage = -1;


    [HideInInspector] public ExperimentType experimentType = ExperimentType.ColorTaste;
    [HideInInspector] public string userId = "no-data";

    [HideInInspector] public SmellType smellType = SmellType.Sweet;
    [HideInInspector] public TasteType tasteType = TasteType.Sweet;
    [HideInInspector] public LiquidColor liquidColor = LiquidColor.Red;



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
        questions1.panel.SetActive(true);

        foreach (var q in questions345)
        {
            q.panel.SetActive(false);
        }

        // xáo trộn các phần tử trong question345
        for (int i = questions345.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            var temp = questions345[i];
            questions345[i] = questions345[randomIndex];
            questions345[randomIndex] = temp;
        }
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

        userStudyManager.isOpenForm = false;
    }

    public void NextPage()
    {
        switch (currentPage)
        {
            case 0:
                if (!IsValid(questions1)) return;
                questionValues[0] = questions1.slider.value;

                questions1.panel.SetActive(false);
                questions345[0].buttonText.text = "Tiếp tục";
                questions345[0].panel.SetActive(true);

                currentPage++;
                break;
            case 1:
                if (!IsValid(questions345[0])) return;
                questionValues[questions345[0].id - 1] = questions345[0].slider.value;

                questions345[0].panel.SetActive(false);
                questions345[1].buttonText.text = "Tiếp tục";
                questions345[1].panel.SetActive(true);

                currentPage++;
                break;
            case 2:
                if (!IsValid(questions345[1])) return;
                questionValues[questions345[1].id - 1] = questions345[1].slider.value;

                questions345[1].panel.SetActive(false);
                questions345[2].buttonText.text = "Tiếp tục";
                questions345[2].panel.SetActive(true);

                currentPage++;
                break;
            case 3:
                if (!IsValid(questions345[2])) return;
                questionValues[questions345[2].id - 1] = questions345[2].slider.value;

                questions345[2].panel.SetActive(false);
                questions345[3].buttonText.text = "Tiếp tục";
                questions345[3].panel.SetActive(true);

                currentPage++;
                break;
            case 4:
                if (!IsValid(questions345[3])) return;
                questionValues[questions345[3].id - 1] = questions345[3].slider.value;

                questions345[3].panel.SetActive(false);
                questions345[4].buttonText.text = "Gửi";
                questions345[4].panel.SetActive(true);

                currentPage++;
                break;
            case 5:
                if (!IsValid(questions345[4])) return;
                questionValues[questions345[4].id - 1] = questions345[4].slider.value;

                questions345[4].panel.SetActive(false);
                SubmitForm();
                break;
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

        questions1.slider.value = 0.1111111f;
        foreach (var q in questions345)
        {
            q.slider.value = 0.1111111f;
        }

        for (int i = 0; i < questionValues.Count; i++)
        {
            questionValues[i] = 0f;
        }
    }
}
