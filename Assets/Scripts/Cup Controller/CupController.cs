using UnityEngine;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

[Serializable]
public class CupData
{
    public string type; // "cup"
    public float x;
    public float y;
    public float z;
    public float rotation;
    public string handleDirection;
    public bool is_stationary;
}

[Serializable]
public class ServerMessage
{
    public string type;
    public CupData data;
}

public enum LiquidColor
{
    Red,
    Black,
    Green,
    Neutral
}

public class CupController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cupGameObject;

    [Header("Liquid Visuals")]
    [SerializeField] private GameObject redLiquidVisual;
    [SerializeField] private GameObject blackLiquidVisual;
    [SerializeField] private GameObject greenLiquidVisual;
    [SerializeField] private GameObject neutralLiquidVisual;

    [Header("Fruit Decor Visuals")]
    [SerializeField] private GameObject strawberryDecor;
    [SerializeField] private GameObject lemonDecor;
    [SerializeField] private GameObject coffeeDecor;

    [Header("Transform Settings")]
    [SerializeField] private float scaleFactor = 0.01f; // mm to Unity units

    private CupData _latestCupData;
    private bool _hasNewCupData = false;
    private LiquidColor _currentLiquidColor;
    private Dictionary<LiquidColor, GameObject> _liquidObjects;
    private Dictionary<string, GameObject> _fruitDecorObjects;
    private string _currentFruitType = "None";

    public LiquidColor CurrentLiquidColor
    {
        get => _currentLiquidColor;
        set
        {
            _currentLiquidColor = value;
            UpdateLiquidVisual(value);
        }
    }

    private void Start()
    {
        // Subscribe to cup data events
        TCPClientManager.Instance.OnCupDataReceived += OnCupDataReceived;
    }

    void Awake()
    {
        _liquidObjects = new Dictionary<LiquidColor, GameObject>
        {
            { LiquidColor.Red, redLiquidVisual },
            { LiquidColor.Black, blackLiquidVisual },
            { LiquidColor.Green, greenLiquidVisual },
            { LiquidColor.Neutral, neutralLiquidVisual }
        };
        UpdateLiquidVisual(_currentLiquidColor);

        _fruitDecorObjects = new Dictionary<string, GameObject>
        {
            { "Strawberry", strawberryDecor },
            { "Lemon", lemonDecor },
            { "Coffee", coffeeDecor }
        };
        // Normalize: hide all fruit decor at startup
        HideAllFruitDecor();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when this component is destroyed
        if (TCPClientManager.Instance != null)
        {
            TCPClientManager.Instance.OnCupDataReceived -= OnCupDataReceived;
        }
    }

    private void OnCupDataReceived(JObject cupDataObject)
    {
        _latestCupData = cupDataObject.ToObject<CupData>();
        _hasNewCupData = true;
    }

    private void Update()
    {
        // Update cup position if new data is available
        if (_hasNewCupData)
        {
            UpdateCupTransform(_latestCupData);
            _hasNewCupData = false;
        }
    }

    private void UpdateCupTransform(CupData cd)
    {
        if (cupGameObject == null) return;

        // Convert millimeters to Unity units and adjust axis mapping
        Vector3 cupPos = new Vector3(cd.x * scaleFactor, cd.y * scaleFactor, cd.z * scaleFactor);
        cupGameObject.transform.localPosition = cupPos;

        // Apply rotation if handle direction data is available
        if (!string.IsNullOrEmpty(cd.rotation.ToString()))
        {
            cupGameObject.transform.localEulerAngles = new Vector3(0, cd.rotation, 0);
        }
    }

    public void SetFruitDecor(string fruitType)
    {
        _currentFruitType = NormalizeFruitType(fruitType);
        HideAllFruitDecor();

        if (_currentFruitType != "None" && _fruitDecorObjects != null &&
            _fruitDecorObjects.TryGetValue(_currentFruitType, out GameObject decorObject) && decorObject != null)
        {
            decorObject.SetActive(true);
        }
    }

    // Normalize fruit type from server: case-insensitive, empty/null/"None" -> "None".
    // Accepts "strawberry", "STRAWBERRY", "Strawberry" -> "Strawberry".
    private static string NormalizeFruitType(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "None";
        string trimmed = raw.Trim();
        if (string.Equals(trimmed, "None", System.StringComparison.OrdinalIgnoreCase)) return "None";
        if (string.Equals(trimmed, "Strawberry", System.StringComparison.OrdinalIgnoreCase)) return "Strawberry";
        if (string.Equals(trimmed, "Lemon", System.StringComparison.OrdinalIgnoreCase)) return "Lemon";
        if (string.Equals(trimmed, "Coffee", System.StringComparison.OrdinalIgnoreCase)) return "Coffee";
        Debug.LogWarning($"[CupController] Unknown fruitType '{raw}', treating as None.");
        return "None";
    }

    public void HideAllFruitDecor()
    {
        if (_fruitDecorObjects == null) return;
        foreach (var decor in _fruitDecorObjects.Values)
        {
            if (decor != null) decor.SetActive(false);
        }
    }

    public void ShowCurrentFruitDecor()
    {
        if (_currentFruitType != "None" && _fruitDecorObjects != null &&
            _fruitDecorObjects.TryGetValue(_currentFruitType, out GameObject decorObject) && decorObject != null)
        {
            decorObject.SetActive(true);
        }
    }

    private void UpdateLiquidVisual(LiquidColor color)
    {
        if (_liquidObjects != null)
        {
            foreach (var liquid in _liquidObjects.Values)
            {
                if (liquid != null)
                {
                    liquid.SetActive(false);
                }
            }

            if (_liquidObjects.TryGetValue(color, out GameObject liquidObject) && liquidObject != null)
            {
                liquidObject.SetActive(true);
            }
        }
    }

    public GameObject GetCurrentLiquidVisual()
    {
        if (_liquidObjects.TryGetValue(_currentLiquidColor, out GameObject liquidObject))
        {
            return liquidObject;
        }
        return null;
    }
}