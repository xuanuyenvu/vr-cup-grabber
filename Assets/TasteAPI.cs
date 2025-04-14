using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TasteAPI
{
    private readonly SerialPortManager _serialPortManager;
    
    public TasteAPI(SerialPortManager serialPortManager)
    {
        _serialPortManager = serialPortManager;
    }
    
    public async void TriggerTaste(OdorType odorType, int duration, int intensity)
    {
        string command = $"{(int)odorType} 1 {duration} {intensity}";
        try
        {
            _serialPortManager.SendCommand(command);
            Debug.Log($"Taste triggered: {command}");

            await Task.Delay(duration);

            StopTaste(odorType);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error triggering taste: {ex.Message}");
        }
        
    }
    
    public async void PullTaste(OdorType odorType, int duration, int intensity)
    {
        string command = $"{(int)odorType} 2 {duration} {intensity}";
        try
        {
            _serialPortManager.SendCommand(command);
            Debug.Log($"Taste triggered: {command}");

            await Task.Delay(duration);

            StopTaste(odorType);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error triggering taste: {ex.Message}");
        }
        
    }
    
    public void StopTaste(OdorType odorType)
    {
        string command = $"{(int)odorType} 0 0 0";
        try
        {
            _serialPortManager.SendCommand(command);
            Debug.Log($"Taste stopped: {command}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error stopping taste: {ex.Message}");
        }
    }
    
    public void ActivateSelectedTastes(List<OdorType> selectedTastes, int duration, int intensity)
    {
        foreach (var taste in selectedTastes)
        {
            TriggerTaste(taste, duration, intensity);
        }
    }
    
    public void DeactivateSelectedTastes(List<OdorType> selectedTastes)
    {
        foreach (var taste in selectedTastes)
        {
            StopTaste(taste);
        }
    }
    
    public void ActivateAllTastes(int duration, int intensity)
    {
        foreach (OdorType odorType in System.Enum.GetValues(typeof(OdorType)))
        {
            TriggerTaste(odorType, duration, intensity);
        }
    }
    
    public void DeactivateAllTastes()
    {
        foreach (OdorType taste in System.Enum.GetValues(typeof(OdorType)))
        {
            StopTaste(taste);
        }
    }
}
