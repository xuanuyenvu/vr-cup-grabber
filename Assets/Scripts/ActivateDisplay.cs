using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDisplay : MonoBehaviour
{
    void Start()
    {
        if (Display.displays.Length > 1)
        {
            Debug.Log("Activating additional display: ");
            Display.displays[0].Activate();
            Display.displays[1].Activate();
        }
    }
}
