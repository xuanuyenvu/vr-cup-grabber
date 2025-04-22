using System.Collections.Generic;
using UnityEngine;

public class SmellTasteManager : MonoBehaviour
{
    // Singleton pattern
    private static SmellTasteManager _instance;
    public static SmellTasteManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SmellTasteManager");
                _instance = go.AddComponent<SmellTasteManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Smell methods
    public void DiffuseSmell(List<string> smells, int duration)
    {
        TCPClientManager.Instance.DiffuseSmell(smells, duration);
    }
    
    public void StopSmell(List<string> smells)
    {
        TCPClientManager.Instance.StopSmell(smells);
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