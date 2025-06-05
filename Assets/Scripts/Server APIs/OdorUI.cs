using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OdorUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdownOdor;
    [SerializeField] private Button diffuseButton;
    [SerializeField] private Button stopButton;

    private void Start()
    {
        diffuseButton.onClick.AddListener(OnDiffuseButtonClicked);
        stopButton.onClick.AddListener(OnStopButtonClicked);   
    }

    public void OnDiffuseButtonClicked()
    {
        Debug.Log("Diffuse button clicked!");
        if (!TCPClientManager.Instance.IsConnected) 
        {
            Debug.LogWarning("Không thể kích hoạt mùi hương: Mất kết nối với server!");
            return;
        }
        // SmellTasteManager.Instance.DiffuseSmell(new List<string> { dropdownOdor.options[dropdownOdor.value].text }, 60000);
    }
    
    public void OnStopButtonClicked()
    {
        if (!TCPClientManager.Instance.IsConnected) 
        {
            Debug.LogWarning("Không thể kích hoạt mùi hương: Mất kết nối với server!");
            return;
        }
        // SmellTasteManager.Instance.StopSmell(new List<string> { dropdownOdor.options[dropdownOdor.value].text });
    }

}
