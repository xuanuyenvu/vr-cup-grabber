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
    [SerializeField] private UserStudyFormManager.TasteType _tasteType;
    [SerializeField] private UserStudyFormManager.SmellType _smellType;
    [SerializeField] private LiquidColor _liquidColor;
    private string _fruitType = "";
    public bool isOpenForm = false;
    public bool isOpenTuToForm = false;

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
        TCPClientManager.Instance.OnTutorialFormOpened += OpenTutorialForm;
        TCPClientManager.Instance.OnTutorialFormClosed += CloseTutorialForm;
        TCPClientManager.Instance.OnVisualFruitChanged += ChangeVisualFruit;
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
        TCPClientManager.Instance.OnTutorialFormOpened -= OpenTutorialForm;
        TCPClientManager.Instance.OnTutorialFormClosed -= CloseTutorialForm;
        TCPClientManager.Instance.OnVisualFruitChanged -= ChangeVisualFruit;
    }

    void OnValidate()
    {
        cupController.CurrentLiquidColor = _currentLiquidColor;
        SmellTasteManager.Instance.CurrentSmellType = _currentSmellType;

        userStudyFormManager.userId = _userId;
        userStudyFormManager.experimentType = _experimentType;

        userStudyFormManager.tasteType = _tasteType;
        userStudyFormManager.smellType = _smellType;
        userStudyFormManager.liquidColor = _liquidColor;

        DisplayUserStudyForm();
        DisplayTutorialForm();

        videoTutorialManager.TogglePlayPause();
        videoTutorialManager.RewindToLast3Seconds();
        videoTutorialManager.SkipForward3Seconds();
    }

    private void DisplayUserStudyForm()
    {
        if (isOpenForm)
        {
            userStudyFormManager.ShowUserStudyFormUI();
        }
        else
        {
            userStudyFormManager.HideUserStudyFormUI();
        }
    }

    private void DisplayTutorialForm()
    {
        if (isOpenTuToForm)
        {
            userStudyFormManager.ShowTutorialFormUI();
        }
        else
        {
            userStudyFormManager.HideTutorialFormUI();
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
            cupController.CurrentLiquidColor = _currentLiquidColor;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse liquid color '{liquidColor}': {ex.Message}");
        }
    }

    private void ChangeVisualFruit(string fruitType)
    {
        _fruitType = string.IsNullOrWhiteSpace(fruitType) ? "None" : fruitType;
        cupController.SetFruitDecor(_fruitType);
    }

    private void SetUserStudyFormOpen(string userId, string experimentType, string tasteType, string smellType, string liquidColor, string fruitType)
    {
        _userId = userId;
        _experimentType = (UserStudyFormManager.ExperimentType)Enum.Parse(typeof(UserStudyFormManager.ExperimentType), experimentType, true);

        _tasteType = (UserStudyFormManager.TasteType)Enum.Parse(typeof(UserStudyFormManager.TasteType), tasteType, true);
        _smellType = (UserStudyFormManager.SmellType)Enum.Parse(typeof(UserStudyFormManager.SmellType), smellType, true);
        _liquidColor = (LiquidColor)Enum.Parse(typeof(LiquidColor), liquidColor, true);
        _fruitType = string.IsNullOrWhiteSpace(fruitType) ? "None" : fruitType;

        // Apply visuals immediately so the cup matches the trial config from open_form,
        // even if the server doesn't follow up with a separate change_visual_fruit command.
        cupController.CurrentLiquidColor = _liquidColor;
        cupController.SetFruitDecor(_fruitType);

        userStudyFormManager.userId = _userId;
        userStudyFormManager.experimentType = _experimentType;

        userStudyFormManager.tasteType = _tasteType;
        userStudyFormManager.smellType = _smellType;
        userStudyFormManager.liquidColor = _liquidColor;
        userStudyFormManager.fruitType = _fruitType;

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

    private void OpenTutorialForm()
    {
        isOpenTuToForm = true;
        DisplayTutorialForm();
    }

    private void CloseTutorialForm()
    {
        isOpenTuToForm = false;
        DisplayTutorialForm();
    }
}
