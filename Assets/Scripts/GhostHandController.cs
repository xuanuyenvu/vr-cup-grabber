using UnityEngine;
using Cup;
using TMPro;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private GameObject[] handGrabPoses;

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
    private bool isDiffuseSmell = false;

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

        if (!cupStateController.IsGrabbing)
        {
            HandleGrabbingState();
        }
    }

    private void HandleGhostHandExit(GameObject ghostHand)
    {
        if (ghostHand == null || virtualCenterEye == null) return;

        float distance = Vector3.Distance(childOpenXRHand.transform.position, mouth.transform.position);
        // canvas.GetComponentInChildren<TextMeshProUGUI>().text = "Exit : " + distance.ToString("F2") + "\ntrack: " + cupStateController.IsTrackedDataValid;
    
        if (distance >= 0.13f && ghostHandClone != null && cupStateController.IsTrackedDataValid)
        {
            // StartCoroutine(FlyToDestination(ghostHandClone.transform, childOpenXRHand.transform, 0.5f));
            Destroy(ghostHandClone);
            ghostHandClone = null;
            childOpenXRHandClone = null;
            // cupStateController.gameObject.SetActive(true);
            // cupPrefabWithoutHandGrabPose.SetActive(false);

            childOpenXRHand.SetActive(true);
            cupStateController.SyncWristPointToReal();

            cupStateController.IsHandSwitchAllowed = true;
            ShowHandGrabPoseObject();
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

        if (distance <= 0.1f && !isDiffuseSmell)
        {
            Debug.Log("Start smell");
            SmellTasteManager.Instance.DiffuseSmell(new List<string> { "odor4" }, 900000);
            isDiffuseSmell = true;
        } 
        else if (distance >= 0.13f && isDiffuseSmell)
        {
            Debug.Log("Stop smell");
            SmellTasteManager.Instance.StopSmell(new List<string> { "odor4" });
            isDiffuseSmell = false;
        }

        if (distance <= 0.06f && ghostHandClone == null)
        {
            HideHandGrabPoseObject();
            cupStateController.IsHandSwitchAllowed = false;

            (handGrabPos.transform.position, handGrabPos.transform.rotation) = cupStateController.CalculateGhostHandSpawnTransform(spawnPoint.transform);

            ghostHandClone = Instantiate(ghostHand, handGrabPos.transform.position, handGrabPos.transform.rotation, handGrabPos.transform);
            childOpenXRHandClone = GetChildByName(ghostHandClone, childName);

            // // dòng for nằm nhằm xóa component HandVisual, 
            // // vì lúc spawn cần script HandVisual để lấy mesh, nhưng sau đó không cần nữa
            foreach (var comp in ghostHandClone.GetComponents<Component>())
            {
                if (!(comp is Transform))
                {
                    Destroy(comp);
                }
            }

            ApplyTransformToGhostHand(ghostHand, ghostHandClone);

            // StartCoroutine(FlyToDestination(ghostHandClone.transform, handGrabPos.transform, 0.5f));
            cupStateController.SyncWristPointToGhostHand(childOpenXRHandClone.transform.position, childOpenXRHandClone.transform.rotation);
            childOpenXRHand.SetActive(false);

            AttachCupToGhostHand();
            cupStateController.IsGrabbing = false;
            ghostHandState = GhostHandState.WaitingToRecall;
        }
    }

    private void ShowHandGrabPoseObject()
    {
        foreach (GameObject handGrabPose in handGrabPoses)
        {
            handGrabPose.SetActive(true);
        }
    }

    private void HideHandGrabPoseObject()
    {
        foreach (GameObject handGrabPose in handGrabPoses)
        {
            handGrabPose.SetActive(false);
        }
    }

    private void ApplyTransformToGhostHand(GameObject originalHand, GameObject clonedHand)
    {
        UpdateGhostHandPose(originalHand, clonedHand);

        ghostHandClone.transform.localPosition = Vector3.zero;
        GameObject ghostHandCloneMesh = GetChildByName(ghostHandClone, isLeftHand ? "OpenXRLeftHand" : "OpenXRRightHand");
        ghostHandCloneMesh.transform.localPosition = Vector3.zero;
        ghostHandCloneMesh.transform.localRotation = Quaternion.identity;
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

    private IEnumerator FlyToDestination(Transform ghostHand, Transform ghostHandDestination, float duration)
    {
        Vector3 startPosition = ghostHand.position;
        Quaternion startRotation = ghostHand.rotation;
        Vector3 endPosition = ghostHandDestination.position;
        Quaternion endRotation = ghostHandDestination.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            ghostHand.position = Vector3.Lerp(startPosition, endPosition, t);
            // ghostHand.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ghostHand.position = endPosition;
        ghostHand.rotation = endRotation;
    }
}