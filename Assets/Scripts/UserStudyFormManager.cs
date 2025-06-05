using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using TMPro;

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
        Color,
        SmellOrthonasal,
        SmellRetronasal
    }
    public enum FlavorType
    {
        Sweet,
        Sour,
        Bitter
    }

    [SerializeField] private GameObject userStudyFormUI;
    [SerializeField] private List<QuestionUI> questions12;
    [SerializeField] private List<QuestionUI> questions345;
    private List<float> questionValues = new List<float> { 0f, 0f, 0f, 0f, 0f };
    private string fileName = "user_study_form.csv";
    private int currentPage = -1;


    [HideInInspector] public ExperimentType experimentType = ExperimentType.Color;
    [HideInInspector] public FlavorType flavorType = FlavorType.Sweet;
    [HideInInspector] public string userId = "no-data";

    void Start()
    {
        ResetValues();
        HideUI();
    }

    private void ShowUI()
    {
        userStudyFormUI.SetActive(true);
        currentPage = 0;
        questions12[0].panel.SetActive(true);

        questions12[1].panel.SetActive(false);
        foreach (var q in questions345)
        {
            q.panel.SetActive(false);
        }

        // xáo trộn các phần tử trong question345
        for (int i = questions345.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            var temp = questions345[i];
            questions345[i] = questions345[randomIndex];
            questions345[randomIndex] = temp;
        }
    }

    private void HideUI()
    {
        userStudyFormUI.SetActive(false);
        foreach (var q in questions12)
        {
            q.panel.SetActive(false);
        }
        foreach (var q in questions345)
        {
            q.panel.SetActive(false);
        }
    }

   public void NextPage()
    {
        switch (currentPage)
        {
            case 0:
                if (!IsValid(questions12[0])) return;
                questionValues[currentPage] = questions12[0].slider.value;

                questions12[0].panel.SetActive(false);
                questions12[1].panel.SetActive(true);

                currentPage++;
                break;
            case 1:
                if (!IsValid(questions12[1])) return;
                questionValues[currentPage] = questions12[1].slider.value;

                questions12[1].panel.SetActive(false);
                questions345[0].panel.SetActive(true);

                currentPage++;
                break;
            case 2:
                if (!IsValid(questions345[0])) return;
                questionValues[currentPage] = questions345[0].slider.value;

                questions345[0].panel.SetActive(false);
                questions345[1].panel.SetActive(true);

                currentPage++;
                break;
            case 3:
                if (!IsValid(questions345[1])) return;
                questionValues[currentPage] = questions345[1].slider.value;

                questions345[1].panel.SetActive(false);
                questions345[2].buttonText.text = "Gửi";
                questions345[2].panel.SetActive(true);

                currentPage++;
                break;
            case 4:
                if (!IsValid(questions345[2])) return;
                questionValues[currentPage] = questions345[2].slider.value;

                questions345[2].panel.SetActive(false);
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

        string filePath = Path.Combine(userDataFolder, fileName);

        string questionsCsv = string.Join(";", questionValues);
        string csvData = $"{userId};{experimentType};{flavorType};{questionsCsv}\n";

        try
        {
            if (!File.Exists(filePath))
            {
                string csvHeader = "sep=;\nUserId;ExperimentType;FlavorType;Q1-Like;Q2-Intense;Q3-Sweet;Q4-Bitter;Q5-Sour\n";
                File.WriteAllText(filePath, csvHeader);
            }

            File.AppendAllText(filePath, csvData);
            Debug.Log($"Form submitted successfully! File saved at: {filePath}");

            ResetValues();
            HideUI();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save form: {ex.Message}");
        }
    }

    private void ResetValues()
    {
        foreach (var q in questions12)
        {
            q.slider.value = 0.1111111f;
        }
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
