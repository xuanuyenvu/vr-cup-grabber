using System.Collections;
using UnityEngine;
using Cup;

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
    public bool isLeftHand => cupStateController.grabbedByHand == CupStateController.GrabbedBy.LeftHand;
    private GameObject childOpenXRHand;

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
        if (ghostHandState == GhostHandState.NotCloned) return;

        UpdateVirtualCenterEyePosition();

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
        else // GhostHandState == GhostHandState.WaitingToRecall
        {
            if (childOpenXRHandClone != null) 
            {
                cupStateController.SyncWristPointToGhostHand(childOpenXRHandClone.transform.position, childOpenXRHandClone.transform.rotation);
            }
            HandleGhostHandExit(targetHand);
        }

        if(!cupStateController.IsGrabbing)
        {
            HandleGrabbingState();
        }
    }

    private void HandleGhostHandExit(GameObject ghostHand)
    {
        if (ghostHand == null || virtualCenterEye == null) return;

        float distance = Vector3.Distance(childOpenXRHand.transform.position, offsetVirtualCenterEye.transform.position);
        Debug.Log("Distance: " + distance);
        // Debug.Log("GH: " + ghostHand.name + " - child: " + childOpenXRHand.name + " - distance: " + distance);
        if (distance > 0.22f && ghostHandClone != null)
        {
            Destroy(ghostHandClone);
            ghostHandClone = null;
            childOpenXRHandClone = null;

            childOpenXRHand.SetActive(true);
            cupStateController.SyncWristPointToReal();
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
        float distance = Vector3.Distance(childOpenXRHand.transform.position, offsetVirtualCenterEye.transform.position);

        if (distance <= 0.2f && ghostHandClone == null)
        {
            ghostHandClone = Instantiate(ghostHand, handGrabPos.transform.position, ghostHand.transform.rotation, handGrabPos.transform);
            childOpenXRHandClone = GetChildByName(ghostHandClone, childName);

            // dòng for nằm nhằm xóa component HandVisual, 
            // vì lúc spawn cần script HandVisual để lấy mesh, nhưng sau đó không cần nữa
            foreach (var comp in ghostHandClone.GetComponents<Component>())
            {
                if (!(comp is Transform))
                {
                    DestroyImmediate(comp);
                }
            }

            StartCoroutine(ApplyTransformToGhostHand(ghostHand, ghostHandClone, childOpenXRHand));
        }
    }

    private IEnumerator ApplyTransformToGhostHand(GameObject originalHand, GameObject clonedHand, GameObject child)
    {
        yield return StartCoroutine(UpdateGhostHandPose(originalHand, clonedHand));

        ghostHandClone.transform.localPosition = Vector3.zero;
        GameObject ghostHandCloneMesh = GetChildByName(ghostHandClone, isLeftHand ? "OpenXRLeftHand" : "OpenXRRightHand");
        ghostHandCloneMesh.transform.localPosition = Vector3.zero;

        cupStateController.SyncWristPointToGhostHand(child.transform.position, child.transform.rotation);
        child.SetActive(false);

        AttachCupToGhostHand();
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
        if (!cupStateController.IsOnTable  && cupStateController.grabbedByHand != CupStateController.GrabbedBy.None)
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