using UnityEngine;


public class CalibrateTable : MonoBehaviour
{
    [SerializeField] private GameObject rightController;
    [SerializeField] private GameObject virtualController;
    [SerializeField] private GameObject cameraRig;
    [SerializeField] private GameObject virtualHeadset;
    [SerializeField] private GameObject tableAxis;
    [SerializeField] private GameObject virtualControllerTargetAxis;

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
        // virtualController.transform.position = new Vector3(8.11170006f, 1.09089994f, -0.333400011f);
        // virtualController.transform.rotation = Quaternion.Euler(new(44.962574f, 90.1477737f, 166.526947f));
        virtualController.transform.position = virtualControllerTargetAxis.transform.position;
        virtualController.transform.rotation = virtualControllerTargetAxis.transform.rotation;

        cameraRig.transform.position = virtualHeadset.transform.position;
        cameraRig.transform.rotation = virtualHeadset.transform.rotation;
    }
}