using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System;

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

public class CupController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cupGameObject;

    [Header("Transform Settings")]
    [SerializeField] private float scaleFactor = 0.01f; // mm to Unity units

    private CupData _latestCupData;
    private bool _hasNewCupData = false;

    private void Start()
    {
        // Subscribe to cup data events
        TCPClientManager.Instance.OnCupDataReceived += OnCupDataReceived;
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
        Debug.Log("Cup data received: " + cupDataObject.ToString());
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
}