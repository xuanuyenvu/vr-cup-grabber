using UnityEngine;

[CreateAssetMenu(fileName = "MarkerRigOffsetData", menuName = "VR/Marker Rig Offset Data", order = 2)]
public class MarkerRigOffsetData : ScriptableObject
{
    [Header("Offset (Marker -> Rig) in Table-Local Space")]
    [Tooltip("Position offset from marker to rig")]
    public Vector3 offsetPosition;
    
    [Tooltip("Rotation offset from marker to rig")]
    public Quaternion offsetRotation = Quaternion.identity;
    
    [Header("Calibration Info")]
    public string lastCalibrationTime;
    public bool hasBeenCalibrated = false;
    
    /// <summary>
    /// Save offset between marker and rig (both in table-local space)
    /// </summary>
    public void SaveOffset(Vector3 markerPosLocal, Quaternion markerRotLocal, 
                           Vector3 rigPosLocal, Quaternion rigRotLocal)
    {
        // Offset: marker -> rig (in marker's local frame)
        // To reconstruct rig from marker: rigRot = markerRot * offsetRot
        //                                 rigPos = markerPos + markerRot * offsetPos
        offsetRotation = Quaternion.Inverse(markerRotLocal) * rigRotLocal;
        offsetPosition = Quaternion.Inverse(markerRotLocal) * (rigPosLocal - markerPosLocal);
        
        lastCalibrationTime = System.DateTime.Now.ToString();
        hasBeenCalibrated = true;
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// Compute rig pose from marker pose using saved offset
    /// </summary>
    public void ComputeRigPose(Vector3 markerPosLocal, Quaternion markerRotLocal,
                               out Vector3 rigPosLocal, out Quaternion rigRotLocal)
    {
        rigRotLocal = markerRotLocal * offsetRotation;
        rigPosLocal = markerPosLocal + markerRotLocal * offsetPosition;
    }
}
