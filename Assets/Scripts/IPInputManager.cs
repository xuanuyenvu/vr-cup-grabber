using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IPInputManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI inputFieldText; 
    [SerializeField] private GameObject placeholder;  
    [SerializeField] private GameObject ui; 
    [SerializeField] private GameObject statusLight;
    [SerializeField] private GameObject statusText;

    [SerializeField] private Sprite connectedSprite;
    [SerializeField] private Sprite disconnectedSprite;
    [SerializeField] private Image btn;

    public enum ConnectionStatus
    {
        Disconnected,
        Waiting,
        Connected
    }

    private ConnectionStatus currentStatus = ConnectionStatus.Disconnected;
    private bool isConnected = false;
    private string currentInput = "";

    void Start()
    {
        UpdateInputField();
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            ToggleUI();
        }
    }

    private void ToggleUI()
    {
        if (ui.activeSelf)
        {
            ui.SetActive(false);
        }
        else
        {
            ui.SetActive(true);
        }
    }

    public void OnNumberPressed(string number)
    {
        if (CanAddCharacter(number))
        {
            if (placeholder.activeSelf)
            {
                placeholder.SetActive(false);
            }

            currentInput += number;
            Debug.Log("Current Input: " + currentInput);
            UpdateInputField();
        }
    }

    public void OnDotPressed()
    {
        if (CanAddDot())
        {
            currentInput += ".";
            UpdateInputField();
        }
    }

    public void OnDeletePressed()
    {
        if (currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateInputField();
        }
        if (currentInput.Length == 0)
        {
            placeholder.SetActive(true);
        }
    }

    public void OnEnterPressed()
    {
        if (currentInput.Length > 0)
        {
            if (isConnected)
            {
                isConnected = false; 
                currentStatus = ConnectionStatus.Disconnected;
                btn.GetComponent<Image>().sprite = connectedSprite;
                OnDisconnectToServer();
                
                currentInput = ""; 
                UpdateInputField();
                placeholder.SetActive(true);
            }
            else
            {
                isConnected = true; 
                currentStatus = ConnectionStatus.Waiting;
                btn.GetComponent<Image>().sprite = disconnectedSprite;
                OnConnectToServer(currentInput);
            }

            ChangeStatus();
            Debug.Log("Final IP: " + currentInput);
        }
    }


    private void UpdateInputField()
    {
        inputFieldText.text = currentInput;
    }

    private bool CanAddDot()
    {
        if (currentInput.Length == 0) return false;
        if (currentInput.EndsWith(".")) return false;

        string[] parts = currentInput.Split('.');
        if (parts.Length >= 4) return false;

        return true;
    }

    private bool CanAddCharacter(string character)
    {
        string[] parts = currentInput.Split('.');
        string currentPart = parts[parts.Length - 1];

        if (currentPart.Length >= 3) return false;

        string testValue = currentPart + character;
        if (int.TryParse(testValue, out int num))
        {
            return num <= 255;
        }

        return false;
    }

    public string GetFinalIP()
    {
        return currentInput;
    }

    private void ChangeStatus()
    {
        if (currentStatus == ConnectionStatus.Connected)
        {
            statusLight.GetComponent<Image>().color = Color.green;
            statusText.GetComponent<TextMeshProUGUI>().text = "Connected";
        }
        else if (currentStatus == ConnectionStatus.Waiting)
        {
            statusLight.GetComponent<Image>().color = Color.yellow;
            statusText.GetComponent<TextMeshProUGUI>().text = "Waiting...";
        }
        else if (currentStatus == ConnectionStatus.Disconnected)
        {
            statusLight.GetComponent<Image>().color = Color.red;
            statusText.GetComponent<TextMeshProUGUI>().text = "Disconnected";
        }
    }

    private void OnConnectToServer(string ipNumber)
    {
        
    }

    private void OnDisconnectToServer()
    {
        
    }

    private void SetSuccess()
    {
        currentStatus = ConnectionStatus.Connected;
        ChangeStatus();
    }
}