using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserStudyManager : MonoBehaviour
{
    [SerializeField] private CupController cupController;
    [SerializeField] private UserStudyFormManager userStudyFormManager;
    [SerializeField] private GameObject tutorialVideo;

    [Space]
    [Header("Tutorial Video Settings")]
    public VideoTutorialManager videoTutorialManager;

    [Space]
    [Header("Smell Settings")]
    [SerializeField] private SmellType _currentSmellType;

    [Space]
    [Header("Liquid Settings")]
    [SerializeField] private LiquidColor _currentLiquidColor;

    [Space]
    [Header("User Study Form Settings")]
    [SerializeField] private string _userId;
    [SerializeField] private UserStudyFormManager.ExperimentType _experimentType;
    [SerializeField] private UserStudyFormManager.FlavorType _flavorType;
    public bool isOpenForm = false;

    void OnValidate()
    {
        cupController.CurrentLiquidColor = _currentLiquidColor;
        SmellTasteManager.Instance.CurrentSmellType = _currentSmellType;

        userStudyFormManager.userId = _userId;
        userStudyFormManager.experimentType = _experimentType;
        userStudyFormManager.flavorType = _flavorType;

        OpenForm();
        OpenTutorialVideo();

        videoTutorialManager.TogglePlayPause();
        videoTutorialManager.RewindToLast3Seconds();
        videoTutorialManager.SkipForward3Seconds();
    }

    private void OpenForm()
    {
        if (isOpenForm)
        {
            userStudyFormManager.ShowUI();
        }
        else
        {
            userStudyFormManager.HideUI();
        }
    }

    private void OpenTutorialVideo()
    {
        // if (isOpenTutorialVideo)
        // {
        //     tutorialVideo.SetActive(true);
        // }
        // else
        // {
        //     tutorialVideo.SetActive(false);
        // }
    }
}
