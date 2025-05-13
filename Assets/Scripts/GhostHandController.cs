using UnityEngine;
using Cup;
using TMPro;

public class GhostHandController : MonoBehaviour
{
    [SerializeField] private CupStateController cupStateController;
    [SerializeField] private GameObject centerEye;
    [SerializeField] private GameObject virtualCenterEye;
    [SerializeField] private GameObject offsetVirtualCenterEye;

    [SerializeField] private GameObject leftHandGrabPos;
    [SerializeField] private GameObject rightHandGrabPos;
    [SerializeField] private GameObject ovrLeftHand;
    [SerializeField] private GameObject ovrRightHand;

    [SerializeField] private GameObject mouth;
    [SerializeField] private GameObject cupRim;
    [SerializeField] private GameObject spawnPoint;

    public GameObject canvas;

    private GameObject ghostHandClone;
    private GameObject childOpenXRHandClone;
    private enum GhostHandState
    {
        NotCloned,
        WaitingToClone,
        WaitingToRecall
    }
    private GhostHandState ghostHandState = GhostHandState.NotCloned;
    private GameObject childOpenXRHand;
    private bool isLeftHand => cupStateController.grabbedByHand == CupStateController.GrabbedBy.LeftHand;

    public void SetGhostHandState(bool _isGrabbing)
    {
        if (cupStateController.grabbedByHand == CupStateController.GrabbedBy.None)
        {
            ghostHandState = GhostHandState.NotCloned;
        }
        else if (_isGrabbing && cupStateController.grabbedByHand != CupStateController.GrabbedBy.None)
        {
            ghostHandState = GhostHandState.WaitingToClone;
        }
        else if (!_isGrabbing && childOpenXRHandClone != null && childOpenXRHandClone.activeSelf && cupStateController.grabbedByHand != CupStateController.GrabbedBy.None)
        {
            ghostHandState = GhostHandState.WaitingToRecall;
        }
    }

    private void OnEnable()
    {
        cupStateController.onGrabbingChange += SetGhostHandState;
    }

    private void OnDisable()
    {
        cupStateController.onGrabbingChange -= SetGhostHandState;
    }

    void Update()
    {
        UpdateVirtualCenterEyePosition();
        if (ghostHandState == GhostHandState.NotCloned) return;

        GameObject targetHand;
        string childName;
        GameObject grabPos;

        if (isLeftHand)
        {
            targetHand = ovrLeftHand;
            childName = "OpenXRLeftHand";
            grabPos = leftHandGrabPos;
        }
        else
        {
            targetHand = ovrRightHand;
            childName = "OpenXRRightHand";
            grabPos = rightHandGrabPos;
        }


        if (ghostHandState == GhostHandState.WaitingToClone)
        {
            cupStateController.SyncWristPointToReal();
            HandleGhostHandEntry(targetHand, childName, grabPos);
        }
        else // ghostHandState == GhostHandState.WaitingToRecall
        {
            if (childOpenXRHandClone != null)
            {
                cupStateController.SyncWristPointToGhostHand(childOpenXRHandClone.transform.position, childOpenXRHandClone.transform.rotation);
            }
            HandleGhostHandExit(targetHand);
        }

        if (childOpenXRHand == null)
        {
            childOpenXRHand = GetChildByName(targetHand, childName);
        }
        float distance = Vector3.Distance(childOpenXRHand.transform.position, mouth.transform.position);
        canvas.GetComponentInChildren<TextMeshProUGUI>().text = "Exit : " + distance.ToString("F2");

        if (!cupStateController.IsGrabbing)
        {
            HandleGrabbingState();
        }
    }

    private void HandleGhostHandExit(GameObject ghostHand)
    {
        if (ghostHand == null || virtualCenterEye == null) return;

        float distance = Vector3.Distance(childOpenXRHand.transform.position, mouth.transform.position);
        // canvas.GetComponentInChildren<TextMeshProUGUI>().text = "Exit : " + distance.ToString("F2");
        canvas.GetComponentInChildren<TextMeshProUGUI>().text = canvas.GetComponentInChildren<TextMeshProUGUI>().text + "\ntrack: " + cupStateController.IsTrackedDataValid;

        if (distance >= 0.14f && ghostHandClone != null && cupStateController.IsTrackedDataValid)
        {
            Destroy(ghostHandClone);
            ghostHandClone = null;
            childOpenXRHandClone = null;
            cupStateController.IsCupGrabLocked = false;

            // childOpenXRHand.SetActive(true);
            cupStateController.SyncWristPointToReal();

            cupStateController.IsHandSwitchAllowed = true;
            ghostHandState = GhostHandState.WaitingToClone;
        }
    }

    private GameObject GetChildByName(GameObject parent, string childName)
    {
        Transform childTransform = parent.transform.Find(childName);
        return childTransform != null ? childTransform.gameObject : null;
    }

    private void HandleGhostHandEntry(GameObject ghostHand, string childName, GameObject handGrabPos)
    {
        if (ghostHand == null || virtualCenterEye == null) return;

        childOpenXRHand = GetChildByName(ghostHand, childName);
        float distance = Vector3.Distance(cupRim.transform.position, mouth.transform.position);
        // canvas.GetComponentInChildren<TextMeshProUGUI>().text = "Entry : " + distance.ToString("F2");

        if (distance <= 0.03f && ghostHandClone == null)
        {
            cupStateController.IsHandSwitchAllowed = false;
            // float angleStep = 6f; 
            // int maxTries = 60;    

            // for (int i = 0; i < maxTries; i++)
            // {
            //     (handGrabPos.transform.position, handGrabPos.transform.rotation) = 
            //         cupStateController.CalculateGhostHandSpawnTransform(spawnPoint.transform);

            //     Vector3 localPos = mouth.transform.InverseTransformPoint(handGrabPos.transform.position);
            //     // Debug.Log($"localPos: {localPos}");

            //     if (isLeftHand && localPos.x < -1.75f && localPos.y < 0.6f)
            //         break;
            //     if (!isLeftHand && localPos.x > 1.75f && localPos.y < 0.6f)
            //         break;

            //     spawnPoint.transform.Rotate(Vector3.up, angleStep, Space.Self);
            // }
            // Debug.Log($"SpawnPoint: {mouth.transform.InverseTransformPoint(handGrabPos.transform.position)}");
            (handGrabPos.transform.position, handGrabPos.transform.rotation) = cupStateController.CalculateGhostHandSpawnTransform(spawnPoint.transform);

            ghostHandClone = Instantiate(ghostHand, handGrabPos.transform.position, handGrabPos.transform.rotation, handGrabPos.transform);
            childOpenXRHandClone = GetChildByName(ghostHandClone, childName);

            // dòng for nằm nhằm xóa component HandVisual, 
            // vì lúc spawn cần script HandVisual để lấy mesh, nhưng sau đó không cần nữa
            foreach (var comp in ghostHandClone.GetComponents<Component>())
            {
                if (!(comp is Transform))
                {
                    Destroy(comp);
                }
            }

            cupStateController.IsCupGrabLocked = true;
            ApplyTransformToGhostHand(ghostHand, ghostHandClone, childOpenXRHand);
        }
    }

    private void ApplyTransformToGhostHand(GameObject originalHand, GameObject clonedHand, GameObject child)
    {
        UpdateGhostHandPose(originalHand, clonedHand);

        ghostHandClone.transform.localPosition = Vector3.zero;
        GameObject ghostHandCloneMesh = GetChildByName(ghostHandClone, isLeftHand ? "OpenXRLeftHand" : "OpenXRRightHand");
        ghostHandCloneMesh.transform.localPosition = Vector3.zero;
        ghostHandCloneMesh.transform.localRotation = Quaternion.identity;

        cupStateController.SyncWristPointToGhostHand(child.transform.position, child.transform.rotation);
        // child.SetActive(false);

        AttachCupToGhostHand();
        cupStateController.IsGrabbing = false;
        ghostHandState = GhostHandState.WaitingToRecall;
    }

    private void UpdateGhostHandPose(GameObject originalHand, GameObject clonedHand)
    {
        SkinnedMeshRenderer originalRenderer = originalHand.GetComponentInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer clonedRenderer = clonedHand.GetComponentInChildren<SkinnedMeshRenderer>();

        if (originalRenderer && clonedRenderer)
        {
            clonedRenderer.sharedMesh = originalRenderer.sharedMesh;
        }

        CopyBoneTransforms(originalHand.transform, clonedHand.transform);
    }

    private void CopyBoneTransforms(Transform source, Transform target)
    {
        for (int i = 0; i < source.childCount; i++)
        {
            Transform sourceChild = source.GetChild(i);
            Transform targetChild = target.GetChild(i);

            if (targetChild != null)
            {
                targetChild.position = sourceChild.position;
                targetChild.rotation = sourceChild.rotation;

                // Đệ quy copy transform của ngón tay con
                CopyBoneTransforms(sourceChild, targetChild);
            }
        }
    }

    private void UpdateVirtualCenterEyePosition()
    {
        if (virtualCenterEye != null && centerEye != null)
        {
            virtualCenterEye.transform.position = centerEye.transform.position;
            virtualCenterEye.transform.rotation = centerEye.transform.rotation;
        }
    }

    private void AttachCupToGhostHand()
    {
        if (!cupStateController.IsOnTable && cupStateController.grabbedByHand != CupStateController.GrabbedBy.None)
        {
            cupStateController.MarkCupForRegrab();
        }
    }

    private void HandleGrabbingState()
    {
        if (cupStateController.IsPendingRegrab && cupStateController.grabbedByHand != CupStateController.GrabbedBy.None)
        {
            cupStateController.PlaceCupInHand();
        }
    }
}