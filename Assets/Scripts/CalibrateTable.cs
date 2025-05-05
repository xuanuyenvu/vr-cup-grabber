using UnityEngine;


public class CalibrateTable : MonoBehaviour
{
    [SerializeField] private GameObject rightController;
    [SerializeField] private GameObject virtualController;
    [SerializeField] private GameObject cameraRig;
    [SerializeField] private GameObject virtualHeadset;
    [SerializeField] private GameObject tableAxis;
    [SerializeField] private Vector3 virtualControllerPosition;
    [SerializeField] private Vector3 virtualControllerRotation;

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Q)) // Nhấn Q để lưu vị trí headset
        // {
        //     GetHeadsetPositionRelativeToController();
        // }
        // if (Input.GetKeyDown(KeyCode.E)) // Nhấn E để di chuyển controller và điều chỉnh headset
        // {
        //     AdjustHeadsetRelativeToTableAxis();
        // }
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            GetHeadsetPositionRelativeToController();
            AdjustHeadsetRelativeToTableAxis();
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
        // Vị trí controller phải trong scene
        // virtualController.transform.position = new Vector3(0.588f, 0.814499974f, 0.150999993f);
        // virtualController.transform.rotation = Quaternion.Euler(new(44.9479828f, 256.104828f, 164.477997f));
        virtualController.transform.position = virtualControllerPosition;
        virtualController.transform.rotation = Quaternion.Euler(virtualControllerRotation);

        cameraRig.transform.position = virtualHeadset.transform.position;
        cameraRig.transform.rotation = virtualHeadset.transform.rotation;
    }
}