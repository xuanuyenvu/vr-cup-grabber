using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Security.Cryptography;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
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

    void OnEnable()
    {
        TCPClientManager.Instance.OnSmellChanged += ChangeSmell;
        TCPClientManager.Instance.OnLiquidColorChanged += ChangeLiquidColor;
        TCPClientManager.Instance.OnUserStudyFormOpened += SetUserStudyFormOpen;
        TCPClientManager.Instance.OnUserStudyFormClosed += CloseForm;
        TCPClientManager.Instance.OnVideoVisibilityChanged += ChangeVideoVisibility;
        TCPClientManager.Instance.OnVideoPlayPauseChanged += TogglePlayPauseVideo;
        TCPClientManager.Instance.OnRewindVideo += RewindVideo;
        TCPClientManager.Instance.OnSkipFowardVideo += SkipForwardVideo;
    }

    void OnDisable()
    {
        TCPClientManager.Instance.OnSmellChanged -= ChangeSmell;
        TCPClientManager.Instance.OnLiquidColorChanged -= ChangeLiquidColor;
        TCPClientManager.Instance.OnUserStudyFormOpened -= SetUserStudyFormOpen;
        TCPClientManager.Instance.OnUserStudyFormClosed -= CloseForm;
        TCPClientManager.Instance.OnVideoVisibilityChanged -= ChangeVideoVisibility;
        TCPClientManager.Instance.OnVideoPlayPauseChanged -= TogglePlayPauseVideo;
        TCPClientManager.Instance.OnRewindVideo -= RewindVideo;
        TCPClientManager.Instance.OnSkipFowardVideo -= SkipForwardVideo;
    }

    void OnValidate()
    {
        cupController.CurrentLiquidColor = _currentLiquidColor;
        SmellTasteManager.Instance.CurrentSmellType = _currentSmellType;

        userStudyFormManager.userId = _userId;
        userStudyFormManager.experimentType = _experimentType;
        userStudyFormManager.flavorType = _flavorType;

        DisplayUserStudyForm();

        videoTutorialManager.TogglePlayPause();
        videoTutorialManager.RewindToLast3Seconds();
        videoTutorialManager.SkipForward3Seconds();
    }

    private void DisplayUserStudyForm()
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

    private void ChangeSmell(SmellType smellType)
    {
        _currentSmellType = smellType;
        SmellTasteManager.Instance.CurrentSmellType = _currentSmellType;
    }

    private void ChangeLiquidColor(string liquidColor)
    {
        try
        {
            _currentLiquidColor = (LiquidColor)Enum.Parse(typeof(LiquidColor), liquidColor, true);
            Debug.Log($"Changed liquid color to: {_currentLiquidColor}");
            cupController.CurrentLiquidColor = _currentLiquidColor;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse liquid color '{liquidColor}': {ex.Message}");
        }
    }

    private void SetUserStudyFormOpen(string userId, string experimentType, string flavorType)
    {
        _userId = userId;
        _experimentType = (UserStudyFormManager.ExperimentType)Enum.Parse(typeof(UserStudyFormManager.ExperimentType), experimentType, true);
        _flavorType = (UserStudyFormManager.FlavorType)Enum.Parse(typeof(UserStudyFormManager.FlavorType), flavorType, true);

        userStudyFormManager.userId = _userId;
        userStudyFormManager.experimentType = _experimentType;
        userStudyFormManager.flavorType = _flavorType;

        isOpenForm = true;
        DisplayUserStudyForm();
    }

    private void CloseForm()
    {
        isOpenForm = false;
        DisplayUserStudyForm();
    }

    private void ChangeVideoVisibility(bool isVisible)
    {
        tutorialVideo.SetActive(isVisible);
    }

    private void TogglePlayPauseVideo(bool isPlaying)
    {
        videoTutorialManager.isClickPlayVideo = isPlaying;
        Debug.Log($"Video play state changed: {isPlaying}");
        videoTutorialManager.TogglePlayPause();
    }

    private void RewindVideo()
    {
        videoTutorialManager._isRewindToLast3Seconds = true;
        videoTutorialManager.RewindToLast3Seconds();
    }

    private void SkipForwardVideo()
    {
        videoTutorialManager._isSkipForward3Seconds = true;
        videoTutorialManager.SkipForward3Seconds();
    }
}
