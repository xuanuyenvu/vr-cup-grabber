using UnityEngine;
using UnityEngine.UI;
using System.IO;

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
    [SerializeField] private GameObject question1Panel;
    [SerializeField] private Slider question1Slider;
    [SerializeField] private GameObject question2Panel;
    [SerializeField] private Slider question2Slider;
    private float question1Value;
    private float question2Value;
    private string fileName = "user_study_form.csv";

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
        question1Panel.SetActive(true);
        question2Panel.SetActive(false);
    }

    private void HideUI()
    {
        userStudyFormUI.SetActive(false);
        question1Panel.SetActive(false);
        question2Panel.SetActive(false);
    }

    public void NextPage()
    {
        // Nếu user chưa chọn giá trị cho question1, không lưu dữ liệu
        if (question1Slider.value == 0.1111111f)
        {
            return;
        }
        question1Value = question1Slider.value;
        question1Panel.SetActive(false);
        question2Panel.SetActive(true);
    }

    public void SubmitForm()
    {
        // Nếu user chưa chọn giá trị cho question2, không lưu dữ liệu
        if (question2Slider.value == 0.1111111f)
        {
            return;
        }
        question2Value = question2Slider.value;

        // Kiểm tra và tạo thư mục UserData nếu chưa tồn tại
        string userDataFolder = Path.Combine(Application.dataPath, "UserData");
        if (!Directory.Exists(userDataFolder))
        {
            Directory.CreateDirectory(userDataFolder);
        }

        string filePath = Path.Combine(userDataFolder, fileName);
        string csvData = $"{userId};{experimentType};{flavorType};{question1Value};{question2Value}\n";

        try
        {
            if (!File.Exists(filePath))
            {
                string csvHeader = "sep=;\nUserId;ExperimentType;FlavorType;Question1;Question2\n";
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
        question1Slider.value = 0.1111111f; // giá trị mặc định
        question2Slider.value = 0.1111111f; // giá trị mặc định
        question1Value = 0f;
        question2Value = 0f;
    }
}
