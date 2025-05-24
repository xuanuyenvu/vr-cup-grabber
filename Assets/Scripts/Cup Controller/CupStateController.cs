using System.Collections;
using UnityEngine;
using TMPro;
using System;

namespace Cup
{
    public struct HandInfo
    {
        public Vector3 position;
        public Quaternion rotation;

        public HandInfo(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }

    public class CupStateController : MonoBehaviour
    {
        [SerializeField] private GameObject cuppSphere;
        [SerializeField] private GameObject wristPoint; // vị trí cổ tay
        [SerializeField] private GameObject cupAttachPoint;
        public MeshRenderer cupMeshRenderer;
        public GameObject liquid;
        private bool isOnTable = true;
        private bool isNearTable = false;
        private bool isPendingRegrab = false;
        private bool isGrabbing = false;

        private float maxTableOffset = 0.06f; // chiều cao tối đa của cốc so với mặt bàn
        private float minTableOffset = -0.09f; // chiều cao tối thiểu của cốc so với mặt bàn
        private float floorOffset = -0.15f; // chiều cao của cốc so với mặt bàn. Nếu đạt giá trị này thì xem như cốc đã rớt xuống sàn 


        [Header("Settings for Ghost Hand Grabbing Logic")]
        private bool isHandSwitchAllowed = true; // cho phép thay đổi tay cầm cốc hay không
        private bool isCupGrabLocked = false; // biến khóa để tay không được cầm cốc nữa


        public enum GrabbedBy { LeftHand, RightHand, None };
        public GrabbedBy grabbedByHand = GrabbedBy.None;
        public event Action<bool> OnGrabbingChange;
        public event Action OnCupThrown;

        // [HideInInspector] public Vector3 cupPosition = new Vector3(0, 0, 0);
        // [HideInInspector] public Quaternion cupRotation = Quaternion.identity;
        [HideInInspector] public HandInfo leftHand = new HandInfo(Vector3.zero, Quaternion.identity);
        [HideInInspector] public HandInfo rightHand = new HandInfo(Vector3.zero, Quaternion.identity);
        [HideInInspector] public bool IsTrackedDataValid = false;
        public bool IsOnTable
        {
            get { return isOnTable; }
        }

        public bool IsNearTable
        {
            get { return isNearTable; }
        }

        public bool IsPendingRegrab
        {
            get { return isPendingRegrab; }
        }

        public bool IsGrabbing
        {
            get { return isGrabbing; }
            set
            {
                isGrabbing = value;
                OnGrabbingChange?.Invoke(isGrabbing);
            }
        }

        public bool IsHandSwitchAllowed
        {
            get { return isHandSwitchAllowed; }
            set { isHandSwitchAllowed = value; }
        }

        public bool IsCupGrabLocked
        {
            get { return isCupGrabLocked; }
            set { isCupGrabLocked = value; }
        }


        public bool IsLeftHandGrabbing()
        {
            return grabbedByHand == GrabbedBy.LeftHand;
        }

        public bool IsRightHandGrabbing()
        {
            return grabbedByHand == GrabbedBy.RightHand;
        }

        public TextMeshProUGUI debugText;

        public void UpdateGrabState(bool isGrabbing, bool isLeftHand)
        {
            grabbedByHand = isGrabbing ? (isLeftHand ? GrabbedBy.LeftHand : GrabbedBy.RightHand)
                                    : GrabbedBy.None;
        }


        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Table") && !isCupGrabLocked)
            {
                isOnTable = true;
                Debug.Log("Cup has landed on the table!");
                OnCupThrown.Invoke();
                AlignCupOnLanding();
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Table"))
            {
                isOnTable = false;
                // Debug.Log("Cup has left the table!");
                HandleCupLifted();
            }
        }

        private void AlignCupOnLanding()
        {
            // Since the cup does not remain stable after being released,
            // it must be rotated to align with its upright position.
            grabbedByHand = GrabbedBy.None;
            isPendingRegrab = false;

            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);

            Vector3 currentPosition = transform.localPosition;
            transform.localPosition = new Vector3(currentPosition.x, 0.02f, currentPosition.z);

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
            }
        }

        private void HandleCupLifted()
        {
            // UpdateCupAttachPoint();
        }

        void Update()
        {
            if (!isOnTable)
            {
                // UpdateSpherePosition();
                CheckCupPosition();
            }
        }

        private void UpdateSpherePosition()
        {
            if (grabbedByHand == GrabbedBy.RightHand)
            {
                wristPoint.transform.position = rightHand.position;
                wristPoint.transform.rotation = rightHand.rotation;
            }
            else if (grabbedByHand == GrabbedBy.LeftHand)
            {
                wristPoint.transform.position = leftHand.position;
                wristPoint.transform.rotation = leftHand.rotation;
            }
        }

        private void CheckCupPosition()
        {
            if (CheckIfNearTable())
            {
                isNearTable = true;
            }
            else
            {
                isNearTable = false;
                if (CheckIfOnFloor())
                {
                    ResetCupPosition();
                }
            }

        }

        private bool CheckIfNearTable()
        {
            float cupHeight = transform.localPosition.y;
            return cupHeight <= maxTableOffset && cupHeight >= minTableOffset;
        }

        private bool CheckIfOnFloor()
        {
            return transform.localPosition.y <= floorOffset;
        }

        private void ResetCupPosition()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                // rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                transform.localPosition = new Vector3(0.4f, 0.02f, 0.4f);
                // transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                transform.rotation = Quaternion.identity;

                StartCoroutine(ReenableGravity(rb));
            }
        }

        private IEnumerator ReenableGravity(Rigidbody rb)
        {
            yield return new WaitForSeconds(0.5f);
            rb.useGravity = true;
        }

        private void HideCup()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
            }

            cupMeshRenderer.enabled = false;
            liquid.SetActive(false);
        }

        private void ShowCup()
        {
            cupMeshRenderer.enabled = true;
            liquid.SetActive(true);
        }

        public void SyncWristPointToGhostHand(Vector3 position, Quaternion rotation)
        {
            wristPoint.transform.position = position;
            wristPoint.transform.rotation = rotation;
        }

        public void SyncWristPointToReal()
        {
            UpdateSpherePosition();
        }

        private void MoveToHandPosition()
        {
            transform.position = cupAttachPoint.transform.position;
            transform.rotation = cupAttachPoint.transform.rotation;
        }

        public void MarkCupForRegrab()
        {
            isPendingRegrab = true;
        }

        public void MakeCupInvisible()
        {
            if (IsHandSwitchAllowed)
            {
                MarkCupForRegrab();
                HideCup();
            }
        }

        public void PlaceCupInHand()
        {
            MoveToHandPosition();

            if (IsTrackedDataValid && !cupMeshRenderer.enabled)
            {
                ShowCup();
            }

            if (isGrabbing)
            {
                isPendingRegrab = false;
            }
        }

        public void DetermineGrabbingHand()
        {
            if (!IsHandSwitchAllowed) return;

            float distanceToLeftHand = Vector3.Distance(transform.position, leftHand.position);
            float distanceToRightHand = Vector3.Distance(transform.position, rightHand.position);

            grabbedByHand = (distanceToLeftHand < distanceToRightHand) ? GrabbedBy.LeftHand : GrabbedBy.RightHand;
            UpdateCupAttachPoint();
        }

        private void UpdateCupAttachPoint()
        {
            cupAttachPoint.transform.position = this.transform.position;
            cupAttachPoint.transform.rotation = this.transform.rotation;
        }

        public (Vector3 position, Quaternion rotation) CalculateGhostHandSpawnTransform(Transform cupTargetTransform)
        {
            return MoveParentToAlignChild(wristPoint.transform, cupAttachPoint.transform, cupTargetTransform);
        }

        private (Vector3 position, Quaternion rotation) MoveParentToAlignChild(Transform parent, Transform child, Transform childTargetTransform)
        {
            Vector3 localPositionParent = child.InverseTransformPoint(parent.position);
            Vector3 targetPositionParent = childTargetTransform.TransformPoint(localPositionParent);

            Quaternion localRotationParent = Quaternion.Inverse(child.rotation) * parent.rotation;
            Quaternion targetRotationParent = childTargetTransform.rotation * localRotationParent;

            return (targetPositionParent, targetRotationParent);
        }

    }
}
