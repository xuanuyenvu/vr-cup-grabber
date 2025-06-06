using UnityEngine;

[CreateAssetMenu(fileName = "HeadsetCalibrationData", menuName = "VR/Headset Calibration Data", order = 1)]
public class HeadsetCalibrationData : ScriptableObject
{
    [Header("Headset Transform")]
    public Vector3 headsetPosition;
    public Quaternion headsetRotation;
    
    [Header("Calibration Info")]
    public string lastCalibrationTime;
    public bool hasBeenCalibrated = false;
    
    public void SaveCurrentTransform(Transform headsetTransform)
    {
        headsetPosition = headsetTransform.position;
        headsetRotation = headsetTransform.rotation;
        lastCalibrationTime = System.DateTime.Now.ToString();
        hasBeenCalibrated = true;
        
        #if UNITY_EDITOR
        // Đánh dấu asset đã thay đổi trong Editor
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}