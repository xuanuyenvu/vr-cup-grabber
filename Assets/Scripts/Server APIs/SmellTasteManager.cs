using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

public static class SmellTypeNames
{
    // Default fallback mappings (used if config file is missing)
    private static readonly Dictionary<SmellType, string> DefaultNames = new Dictionary<SmellType, string>
    {
        { SmellType.Sweet, "odor4" },
        { SmellType.Sour, "odor6" },
        { SmellType.Bitter, "odor5" },
        { SmellType.Neutral, "odor1" },
        { SmellType.Strawberry, "odor4" },
        { SmellType.Lemon, "odor5" },
        { SmellType.Coffee, "odor6" }
        // SmellType.None is intentionally omitted — means no scent
    };

    private static Dictionary<SmellType, string> _names;
    private static bool _loaded = false;

    public static Dictionary<SmellType, string> Names
    {
        get
        {
            if (!_loaded)
            {
                _names = LoadFromConfig();
                _loaded = true;
            }
            return _names;
        }
    }

    private static Dictionary<SmellType, string> LoadFromConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "odor_config.json");

        if (!File.Exists(configPath))
        {
            Debug.LogWarning($"[SmellTypeNames] Config not found at {configPath}, using defaults.");
            return new Dictionary<SmellType, string>(DefaultNames);
        }

        try
        {
            string json = File.ReadAllText(configPath);
            var config = JsonUtility.FromJson<OdorConfig>(json);

            // Start with defaults, then override with config values
            var result = new Dictionary<SmellType, string>(DefaultNames);

            if (!string.IsNullOrEmpty(config.odorMappings.Strawberry))
                result[SmellType.Strawberry] = config.odorMappings.Strawberry;
            if (!string.IsNullOrEmpty(config.odorMappings.Lemon))
                result[SmellType.Lemon] = config.odorMappings.Lemon;
            if (!string.IsNullOrEmpty(config.odorMappings.Coffee))
                result[SmellType.Coffee] = config.odorMappings.Coffee;

            Debug.Log($"[SmellTypeNames] Loaded odor config: Strawberry={result[SmellType.Strawberry]}, Lemon={result[SmellType.Lemon]}, Coffee={result[SmellType.Coffee]}");
            return result;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SmellTypeNames] Failed to load config: {ex.Message}. Using defaults.");
            return new Dictionary<SmellType, string>(DefaultNames);
        }
    }

    /// <summary>
    /// Force reload config from StreamingAssets (e.g. after editing the file at runtime).
    /// </summary>
    public static void ReloadConfig()
    {
        _loaded = false;
        _ = Names; // trigger reload
    }
}

[System.Serializable]
public class OdorMappings
{
    public string Strawberry;
    public string Lemon;
    public string Coffee;
}

[System.Serializable]
public class OdorConfig
{
    public OdorMappings odorMappings;
}

public class SmellTasteManager : MonoBehaviour
{
    // Singleton pattern
    private static SmellTasteManager _instance;
    private SmellType _currentSmellType;
    public static SmellTasteManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SmellTasteManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("SmellTasteManager");
                    _instance = go.AddComponent<SmellTasteManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public SmellType CurrentSmellType { get => _currentSmellType; set => _currentSmellType = value; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            _instance._currentSmellType = _currentSmellType;
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Smell methods
    public void DiffuseSmell(int duration)
    {
        if (CurrentSmellType == SmellType.None) return; // No scent for None
        TCPClientManager.Instance.DiffuseSmell(new List<string> { SmellTypeNames.Names[CurrentSmellType] }, duration);
    }

    public void StopSmell()
    {
        if (CurrentSmellType == SmellType.None) return; // No scent for None
        TCPClientManager.Instance.StopSmell(new List<string> { SmellTypeNames.Names[CurrentSmellType] });
    }

    // Taste methods
    public void DiffuseTaste(List<string> tastes, int duration, int speed)
    {
        TCPClientManager.Instance.DiffuseTaste(tastes, duration, speed);
    }

    public void StopTaste(List<string> tastes)
    {
        TCPClientManager.Instance.StopTaste(tastes);
    }
}