using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Codice.Client.BaseCommands;

public enum HandType
{
    LeftHand,
    RightHand
}

public enum HandCollisionStatus
{
    Enter,
    Exit
}

public class HandCollider : MonoBehaviour
{
    public event Action<HandType, HandCollisionStatus> onTriggerEnterAction;
    public event Action<HandType, HandCollisionStatus> onTriggerExitAction;

    [SerializeField] private HandType _handType;
    private bool _isTrigger = false;
    private int _triggerCount = 0;

    public bool IsTrigger
    {
        get => _isTrigger;
        set
        {
            if (_isTrigger != value)
            {
                _isTrigger = value;
                if (_isTrigger)
                {
                    onTriggerEnterAction?.Invoke(_handType, HandCollisionStatus.Enter);
                }
                else
                {
                    onTriggerExitAction?.Invoke(_handType, HandCollisionStatus.Exit);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Headset"))
        {
            _triggerCount++;
            if (_triggerCount == 1)
            {
                IsTrigger = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Headset"))
        {
            _triggerCount--;
            if (_triggerCount == 0)
            {
                IsTrigger = false;
            }
        }
    }
}
