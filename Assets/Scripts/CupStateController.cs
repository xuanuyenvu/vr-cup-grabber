using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cup
{
    public class CupStateController : MonoBehaviour
    {
        private bool isOnTable = true;
        private bool isNearTable = false;
        private float tableOffset = 0.03f;

        public bool IsNearTable
        {
            get { return isNearTable; }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Table"))
            {
                isOnTable = true;
                Debug.Log("Cup has landed on the table!");
                HandleCupLanded();
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Table"))
            {
                isOnTable = false;
                Debug.Log("Cup has left the table!");
                HandleCupLifted();
            }
        }

        private void HandleCupLanded()
        {
            // Since the cup does not remain stable after being released,
            // it must be rotated to align with its upright position.
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);
        }

        private void HandleCupLifted()
        {

        }

        void Update()
        {
            if (!isOnTable)
            {
                CheckCupPosition();
            }
        }

        private void CheckCupPosition()
        {
            if (CheckIfNearTable())
            {
                isNearTable = true;
                Debug.Log("Cup is near the table!");
            }
            else
            {
                isNearTable = false;
                Debug.Log("Cup is NOT near the table!");
            }

        }

        private bool CheckIfNearTable()
        {
            float cupHeight = transform.localPosition.y;
            Debug.Log("Cup height: " + cupHeight);
            return cupHeight <= tableOffset;
        }
    }
}
