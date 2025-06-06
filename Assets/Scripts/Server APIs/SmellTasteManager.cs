using System.Collections.Generic;
using UnityEngine;

public enum SmellType
{
    Sweet,
    Sour,
    Bitter,
    Neutral
}

public static class SmellTypeNames
{
    public static readonly Dictionary<SmellType, string> Names = new Dictionary<SmellType, string>
    {
        { SmellType.Sweet, "odor4" },
        { SmellType.Sour, "odor6" },
        { SmellType.Bitter, "odor5" },
        { SmellType.Neutral, "odor1" }
     };
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
        TCPClientManager.Instance.DiffuseSmell(new List<string> { SmellTypeNames.Names[CurrentSmellType] }, duration);
    }

    public void StopSmell()
    {
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