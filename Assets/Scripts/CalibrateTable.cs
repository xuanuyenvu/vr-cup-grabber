using UnityEngine;
using System.IO;

public class CalibrateTable : MonoBehaviour
{
    [SerializeField] private GameObject rightController;
    [SerializeField] private GameObject virtualController;
    [SerializeField] private GameObject cameraRig;
    [SerializeField] private GameObject virtualHeadset;
    [SerializeField] private GameObject tableAxis;
    [SerializeField] private GameObject virtualControllerTargetAxis;

    [Header("Calibration Settings")]
    [SerializeField] private HeadsetCalibrationData calibrationData;
    [SerializeField] private bool loadPositionOnStart = true;

    [Header("Auto Save")]
    [Tooltip("Tự động lưu vị trí khi thoát ứng dụng")]
    [SerializeField] private bool savePositionOnQuit = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    void Start()
    {
        if (loadPositionOnStart && calibrationData != null)
        {
            // Áp dụng vị trí đã lưu nếu có
            if (calibrationData.hasBeenCalibrated)
            {
                LoadHeadsetPosition();
            }
        }
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            GetHeadsetPositionRelativeToController();
            AdjustHeadsetRelativeToTableAxis();

            // Lưu vị trí sau khi hiệu chỉnh
            SaveHeadsetPosition();
        }
    }

    private void OnApplicationQuit()
    {
        if (savePositionOnQuit && calibrationData != null)
        {
            // Lưu vị trí hiện tại
            calibrationData.SaveCurrentTransform(cameraRig.transform);

            // Lưu xuống file nếu đang chạy build
            if (showDebugInfo)
            {
                Debug.Log($"Headset position saved on quit: {cameraRig.transform.position}");
            }
        }
    }

    public void GetHeadsetPositionRelativeToController()
    {
        virtualController.transform.position = rightController.transform.position;
        virtualController.transform.rotation = rightController.transform.rotation;

        virtualHeadset.transform.position = cameraRig.transform.position;
        virtualHeadset.transform.rotation = cameraRig.transform.rotation;
    }

    public void AdjustHeadsetRelativeToTableAxis()
    {
        virtualController.transform.position = virtualControllerTargetAxis.transform.position;
        virtualController.transform.rotation = virtualControllerTargetAxis.transform.rotation;

        cameraRig.transform.position = virtualHeadset.transform.position;
        cameraRig.transform.rotation = virtualHeadset.transform.rotation;
    }

    // Lưu vị trí và góc quay của Headset vào ScriptableObject
    public void SaveHeadsetPosition()
    {
        if (calibrationData != null)
        {
            calibrationData.SaveCurrentTransform(cameraRig.transform);

            if (showDebugInfo)
            {
                Debug.Log($"Headset position saved: {cameraRig.transform.position}, Time: {calibrationData.lastCalibrationTime}");
            }
        }
        else
        {
            Debug.LogError("Cannot save headset position: calibrationData is null. Assign a HeadsetCalibrationData asset in the Inspector.");
        }
    }

    // Khôi phục vị trí và góc quay của Headset từ ScriptableObject
    public void LoadHeadsetPosition()
    {
        if (calibrationData != null && calibrationData.hasBeenCalibrated)
        {
            cameraRig.transform.position = calibrationData.headsetPosition;
            cameraRig.transform.rotation = calibrationData.headsetRotation;

            if (showDebugInfo)
            {
                Debug.Log($"Headset position loaded: {calibrationData.headsetPosition}, Last calibration: {calibrationData.lastCalibrationTime}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("No saved headset calibration data found or calibrationData is null.");
            }
        }
    }

}