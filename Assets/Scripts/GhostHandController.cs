using System.Collections;
using UnityEngine;
using Cup;
using System.Runtime.InteropServices;

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

    private GameObject ghostHandClone;
    private GameObject childOpenXRHandClone;
    private enum GhostHandState
    {
        NotCloned,
        WaitingToClone,
        WaitingToRecall,
        WaitingToGrab
    }
    private GhostHandState ghostHandState = GhostHandState.NotCloned;
    private GameObject childOpenXRHand;
    private GameObject grandChilHand;
    private HandCollider _ovrRightHandCollider;
    private HandCollider _ovrLeftHandCollider;

    public bool isLeftHand => cupStateController.grabbedByHand == CupStateController.GrabbedBy.LeftHand;

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

        _ovrLeftHandCollider = ovrLeftHand.GetComponent<HandCollider>();
        _ovrLeftHandCollider.onTriggerEnterAction += HandleHandNearHeadset;
        _ovrLeftHandCollider.onTriggerExitAction += HandleHandNearHeadset;

        _ovrRightHandCollider = ovrRightHand.GetComponent<HandCollider>();
        _ovrRightHandCollider.onTriggerEnterAction += HandleHandNearHeadset;
        _ovrRightHandCollider.onTriggerExitAction += HandleHandNearHeadset;

    }

    private void OnDisable()
    {
        cupStateController.onGrabbingChange -= SetGhostHandState;

        _ovrLeftHandCollider.onTriggerEnterAction -= HandleHandNearHeadset;
        _ovrLeftHandCollider.onTriggerExitAction -= HandleHandNearHeadset;

        _ovrRightHandCollider.onTriggerEnterAction -= HandleHandNearHeadset;
        _ovrRightHandCollider.onTriggerExitAction -= HandleHandNearHeadset;
    }

    void Update()
    {
        if (ghostHandState == GhostHandState.NotCloned) return;

        UpdateVirtualCenterEyePosition();

        if (ghostHandState == GhostHandState.WaitingToClone)
        {
            cupStateController.SyncWristPointToReal();
        }
        else if (ghostHandState == GhostHandState.WaitingToRecall)
        {
            if (childOpenXRHandClone != null)
            {
                cupStateController.SyncWristPointToGhostHand(childOpenXRHandClone.transform.position, childOpenXRHandClone.transform.rotation);
            }
        }

        if (!cupStateController.IsGrabbing)
        {
            HandleGrabbingState();
        }
    }

    private void HandleHandNearHeadset(HandType handType, HandCollisionStatus handCollisionStatus)
    {
        Debug.Log("HandleHandNearHeadset: " + handType + " - " + handCollisionStatus);
        if (ghostHandState == GhostHandState.NotCloned) return;

        GameObject targetHand;
        string childName, grandChildName;
        GameObject grabPos;

        if (isLeftHand && handType == HandType.LeftHand)
        {
            targetHand = ovrLeftHand;
            childName = "OpenXRLeftHand";
            grandChildName = "LeftHand";
            grabPos = leftHandGrabPos;
        }
        else if (!isLeftHand && handType == HandType.RightHand)
        {
            targetHand = ovrRightHand;
            childName = "OpenXRRightHand";
            grandChildName = "RightHand";
            grabPos = rightHandGrabPos;
        }
        else
        {
            return;
        }

        if (ghostHandState == GhostHandState.WaitingToClone && handCollisionStatus == HandCollisionStatus.Enter)
        {
            Debug.Log("TriggerEnter");
            HandleGhostHandEntry(targetHand, childName, grandChildName, grabPos);
        }
        else if (ghostHandState == GhostHandState.WaitingToRecall && handCollisionStatus == HandCollisionStatus.Exit) // ghostHandState == GhostHandState.WaitingToRecall
        {
            Debug.Log("TriggerExit");
            HandleGhostHandExit(targetHand);
        }
    }

    private void HandleGhostHandExit(GameObject ghostHand)
    {
        if (ghostHand == null || virtualCenterEye == null) return;

        // float dista nce = Vector3.Distance(childOpenXRHand.transform.position, offsetVirtualCenterEye.transform.position);
        // Debug.Log("Distance: " + distance);
        // Debug.Log("GH: " + ghostHand.name + " - child: " + childOpenXRHand.name + " - distance: " + distance);
        Destroy(ghostHandClone);
        ghostHandClone = null;
        childOpenXRHandClone = null;

        grandChilHand.SetActive(true);
        cupStateController.SyncWristPointToReal();
        ghostHandState = GhostHandState.WaitingToClone;
        // if (distance > 0.22f && ghostHandClone != null)
        // {

        // }
    }

    private GameObject GetChildByName(GameObject parent, string childName)
    {
        Transform childTransform = parent.transform.Find(childName);
        return childTransform != null ? childTransform.gameObject : null;
    }

    private void HandleGhostHandEntry(GameObject ghostHand, string childName, string grandChildName, GameObject handGrabPos)
    {
        if (ghostHand == null || virtualCenterEye == null) return;

        childOpenXRHand = GetChildByName(ghostHand, childName);
        grandChilHand = GetChildByName(childOpenXRHand, grandChildName);
        // float distance = Vector3.Distance(childOpenXRHand.transform.position, offsetVirtualCenterEye.transform.position);

        ghostHandClone = Instantiate(ghostHand, handGrabPos.transform.position, ghostHand.transform.rotation, handGrabPos.transform);
        childOpenXRHandClone = GetChildByName(ghostHandClone, childName);

        // dòng for nằm nhằm xóa component HandVisual, 
        // vì lúc spawn cần script HandVisual để lấy mesh, nhưng sau đó không cần nữa
        foreach (var comp in ghostHandClone.GetComponents<Component>())
        {
            if (!(comp is Transform))
            {
                // DestroyImmediate(comp);
                // comp.gameObject.SetActive(false);
                Destroy(comp);
            }
        }

        StartCoroutine(ApplyTransformToGhostHand(ghostHand, ghostHandClone, childOpenXRHand, grandChilHand));
        // if (distance <= 0.2f && ghostHandClone == null)
        // {

        // }
    }

    private IEnumerator ApplyTransformToGhostHand(GameObject originalHand, GameObject clonedHand, GameObject child, GameObject grandChild)
    {
        yield return StartCoroutine(UpdateGhostHandPose(originalHand, clonedHand));

        ghostHandClone.transform.localPosition = Vector3.zero;
        GameObject ghostHandCloneMesh = GetChildByName(ghostHandClone, isLeftHand ? "OpenXRLeftHand" : "OpenXRRightHand");
        ghostHandCloneMesh.transform.localPosition = Vector3.zero;

        cupStateController.SyncWristPointToGhostHand(child.transform.position, child.transform.rotation);
        grandChild.SetActive(false);

        AttachCupToGhostHand();
        Debug.Log("di toi day chua");
        ghostHandState = GhostHandState.WaitingToRecall;
    }

    private IEnumerator UpdateGhostHandPose(GameObject originalHand, GameObject clonedHand)
    {
        yield return new WaitForEndOfFrame(); // Chờ 1 frame để đảm bảo transform ổn định

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
            cupStateController.MakeCupInvisible();
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