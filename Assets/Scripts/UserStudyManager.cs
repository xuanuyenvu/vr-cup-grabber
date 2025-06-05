using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserStudyManager : MonoBehaviour
{
    [SerializeField] private CupController cupController;
    [SerializeField] private UserStudyFormManager userStudyFormManager;

    [Space][Header("Smell Settings")]
    [SerializeField] private SmellType _currentSmellType;

    [Space][Header("Liquid Settings")]
    [SerializeField] private LiquidColor _currentLiquidColor;

    [Space][Header("User Study Form Settings")]
    [SerializeField] private string _userId;
    [SerializeField] private UserStudyFormManager.ExperimentType _experimentType;
    [SerializeField] private UserStudyFormManager.FlavorType _flavorType;

    void OnValidate()
    {
        cupController.CurrentLiquidColor = _currentLiquidColor;
        SmellTasteManager.Instance.CurrentSmellType = _currentSmellType;

        userStudyFormManager.userId = _userId;
        userStudyFormManager.experimentType = _experimentType;
        userStudyFormManager.flavorType = _flavorType;
    }
}
