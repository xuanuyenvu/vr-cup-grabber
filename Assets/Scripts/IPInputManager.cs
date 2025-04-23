 using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IPInputManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ipInputFieldText; 
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

    private bool _isConnecting = false;
    private string _currentInput = "";

    void Start()
    {
        ipInputFieldText.text = TCPClientManager.Instance.ServerIP;
        UpdateInputField();
    }

    private void OnEnable()
    {
        TCPClientManager.Instance.OnConnected += OnServerConnected;
        TCPClientManager.Instance.OnDisconnected += OnServerDisconnected;
    }

    private void OnDisable()
    {
        if (TCPClientManager.Instance == null) return;
        
        TCPClientManager.Instance.OnConnected -= OnServerConnected;
        TCPClientManager.Instance.OnDisconnected -= OnServerDisconnected;
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

            _currentInput += number;
            Debug.Log("Current Input: " + _currentInput);
            UpdateInputField();
        }
    }

    public void OnDotPressed()
    {
        if (CanAddDot())
        {
            _currentInput += ".";
            UpdateInputField();
        }
    }

    public void OnDeletePressed()
    {
        if (_currentInput.Length > 0)
        {
            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
            UpdateInputField();
        }
        if (_currentInput.Length == 0)
        {
            placeholder.SetActive(true);
        }
    }

    public void OnEnterPressed()
    {
        if (_currentInput.Length > 0)
        {
            // Bấm vào nút disconnect
            bool isConnected = TCPClientManager.Instance.IsConnected;
            if (isConnected)
            {
                btn.GetComponent<Image>().sprite = connectedSprite;
                OnDisconnectToServer();                
            }
            else
            {
                btn.GetComponent<Image>().sprite = disconnectedSprite;
                OnConnectToServer(_currentInput);
            }

            ChangeStatus();
            Debug.Log("Final IP: " + _currentInput);
        }
    }


    private void UpdateInputField()
    {
        ipInputFieldText.text = _currentInput;
    }

    private bool CanAddDot()
    {
        if (_currentInput.Length == 0) return false;
        if (_currentInput.EndsWith(".")) return false;

        string[] parts = _currentInput.Split('.');
        if (parts.Length >= 4) return false;

        return true;
    }

    private bool CanAddCharacter(string character)
    {
        string[] parts = _currentInput.Split('.');
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
        return _currentInput;
    }

    private void ChangeStatus()
    {
        bool isConnected = TCPClientManager.Instance.IsConnected;

        if (_isConnecting)
        {
            statusLight.GetComponent<Image>().color = Color.yellow;
            statusText.GetComponent<TextMeshProUGUI>().text = "Waiting...";
        }
        else if (isConnected)
        {
            statusLight.GetComponent<Image>().color = Color.green;
            statusText.GetComponent<TextMeshProUGUI>().text = "Connected";
        }
        else
        {
            statusLight.GetComponent<Image>().color = Color.red;
            statusText.GetComponent<TextMeshProUGUI>().text = "Disconnected";
        }
    }

    private void OnConnectToServer(string ipNumber)
    {
        TCPClientManager.Instance.ServerIP = ipNumber;
        _ = TCPClientManager.Instance.ConnectToServer();

    }

    private void OnDisconnectToServer()
    {
        TCPClientManager.Instance.Disconnect();
    }

    private void OnServerDisconnected()
    {
        _isConnecting = false;
        ChangeStatus();
    }

    private void OnServerConnected()
    {
        _isConnecting = false;
        ChangeStatus();
    }
}