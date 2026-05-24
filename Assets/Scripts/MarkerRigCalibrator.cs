using UnityEngine;

/// <summary>
/// Calibrate cameraRig position based on tracked marker offset.
/// 
/// Workflow:
/// 1. Use old calibration (CalibrateTable) to get rig in correct position
/// 2. Press O to capture offset between marker and rig
/// 3. Press Space to apply/test: snap rig to marker + offset
/// </summary>
public class MarkerRigCalibrator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraRig;
    [SerializeField] private Transform tableAxis;
    [Tooltip("Leave null to auto-get from TCPClientManager.Instance")]
    [SerializeField] private Transform hmdTrackingMarker;
    
    [Header("Offset Data")]
    [SerializeField] private MarkerRigOffsetData offsetData;
    
    [Header("Settings")]
    [SerializeField] private bool loadOffsetOnStart = true;
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("Keybindings")]
    [SerializeField] private KeyCode captureKey = KeyCode.O;
    [SerializeField] private KeyCode applyKey = KeyCode.Space;
    
    private void Start()
    {
        // Auto-get marker reference from TCPClientManager if not assigned
        if (hmdTrackingMarker == null)
        {
            var tcpManager = TCPClientManager.Instance;
            if (tcpManager != null && tcpManager.hmdTrackingMarker != null)
            {
                hmdTrackingMarker = tcpManager.hmdTrackingMarker.transform;
                if (showDebugInfo)
                    Debug.Log("[MarkerRigCalibrator] Auto-assigned hmdTrackingMarker from TCPClientManager");
            }
            else
            {
                Debug.LogWarning("[MarkerRigCalibrator] Could not auto-get hmdTrackingMarker from TCPClientManager");
            }
        }
        
        // Load saved offset if available
        if (loadOffsetOnStart && offsetData != null && offsetData.hasBeenCalibrated)
        {
            if (showDebugInfo)
                Debug.Log($"[MarkerRigCalibrator] Loaded offset from {offsetData.lastCalibrationTime}");
        }
    }
    
    private void Update()
    {
        // Capture offset: Press O after old calibration is done and rig is correct
        if (Input.GetKeyDown(captureKey))
        {
            CaptureOffset();
        }
        
        // Apply offset: Press Space to snap rig based on marker
        if (Input.GetKeyDown(applyKey))
        {
            ApplyRigFromMarker();
        }
    }
    
    /// <summary>
    /// Capture offset between marker and rig (call when rig is in correct position)
    /// Both poses are computed in tableAxis local space
    /// </summary>
    public void CaptureOffset()
    {
        if (!ValidateReferences()) return;
        
        // Get marker pose in table-local space (use InverseTransformPoint for consistency)
        // Don't use localPosition as marker's parent may not be tableAxis
        Vector3 markerPosLocal = tableAxis.InverseTransformPoint(hmdTrackingMarker.position);
        Quaternion markerRotLocal = Quaternion.Inverse(tableAxis.rotation) * hmdTrackingMarker.rotation;
        
        // Get rig pose in table-local space
        Vector3 rigPosLocal = tableAxis.InverseTransformPoint(cameraRig.position);
        Quaternion rigRotLocal = Quaternion.Inverse(tableAxis.rotation) * cameraRig.rotation;
        
        // Save offset
        offsetData.SaveOffset(markerPosLocal, markerRotLocal, rigPosLocal, rigRotLocal);
        
        if (showDebugInfo)
        {
            Debug.Log($"[MarkerRigCalibrator] Offset captured!");
            Debug.Log($"  Marker (table-local): pos={markerPosLocal}, rot={markerRotLocal.eulerAngles}");
            Debug.Log($"  Rig (table-local): pos={rigPosLocal}, rot={rigRotLocal.eulerAngles}");
            Debug.Log($"  Offset: pos={offsetData.offsetPosition}, rot={offsetData.offsetRotation.eulerAngles}");
        }
    }
    
    /// <summary>
    /// Apply rig pose from current marker pose + saved offset
    /// </summary>
    public void ApplyRigFromMarker()
    {
        if (!ValidateReferences()) return;
        
        if (!offsetData.hasBeenCalibrated)
        {
            Debug.LogWarning("[MarkerRigCalibrator] No offset captured yet! Press O to capture first.");
            return;
        }
        
        // Get current marker pose in table-local space (use InverseTransformPoint for consistency)
        Vector3 markerPosLocal = tableAxis.InverseTransformPoint(hmdTrackingMarker.position);
        Quaternion markerRotLocal = Quaternion.Inverse(tableAxis.rotation) * hmdTrackingMarker.rotation;
        
        // Compute rig pose from marker + offset
        offsetData.ComputeRigPose(markerPosLocal, markerRotLocal, 
                                   out Vector3 rigPosLocal, out Quaternion rigRotLocal);
        
        // Convert back to world space and apply
        cameraRig.position = tableAxis.TransformPoint(rigPosLocal);
        cameraRig.rotation = tableAxis.rotation * rigRotLocal;
        
        if (showDebugInfo)
        {
            Debug.Log($"[MarkerRigCalibrator] Rig applied from marker!");
            Debug.Log($"  Marker (table-local): pos={markerPosLocal}, rot={markerRotLocal.eulerAngles}");
            Debug.Log($"  Rig (world): pos={cameraRig.position}, rot={cameraRig.rotation.eulerAngles}");
        }
    }
    
    private bool ValidateReferences()
    {
        if (cameraRig == null)
        {
            Debug.LogError("[MarkerRigCalibrator] cameraRig is not assigned!");
            return false;
        }
        if (tableAxis == null)
        {
            Debug.LogError("[MarkerRigCalibrator] tableAxis is not assigned!");
            return false;
        }
        if (hmdTrackingMarker == null)
        {
            Debug.LogError("[MarkerRigCalibrator] hmdTrackingMarker is not assigned!");
            return false;
        }
        if (offsetData == null)
        {
            Debug.LogError("[MarkerRigCalibrator] offsetData ScriptableObject is not assigned!");
            return false;
        }
        return true;
    }
}
