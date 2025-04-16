using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum OdorType
{
    Odor1,
    Odor2,
    Odor3,
    Odor4,
    Odor5,
    Odor6,
}

public class SmellAPI
{
    private readonly SerialPortManager _serialPortManager;

    public SmellAPI(SerialPortManager serialPortManager)
    {
        _serialPortManager = serialPortManager;
    }

    public async void TriggerSmell(OdorType odorType, int duration)
    {
        string command = $"{(int)odorType} 1 {duration}";
        try
        {
            _serialPortManager.SendCommand(command);
            Debug.Log($"Smell triggered: {command}");

            await Task.Delay(duration);

            StopSmell(odorType);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error triggering smell: {ex.Message}");
        }
    }

    public void StopSmell(OdorType odorType)
    {
        string command = $"{(int)odorType} 0 0";
        try
        {
            _serialPortManager.SendCommand(command);
            Debug.Log($"Smell stopped: {command}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error stopping smell: {ex.Message}");
        }
    }
    
    public void ActivateSelectedSmells(List<OdorType> selectedSmells, int duration)
    {
        foreach (var smell in selectedSmells)
        {
            TriggerSmell(smell, duration);
        }
    }
    
    public void DeactivateSelectedSmells(List<OdorType> selectedSmells)
    {
        foreach (var smell in selectedSmells)
        {
            StopSmell(smell);
        }
    }
    
    public void ActivateAllSmells(int duration)
    {
        foreach (OdorType odorType in System.Enum.GetValues(typeof(OdorType)))
        {
            TriggerSmell(odorType, duration);
        }
    }
    
    public void DeactivateAllSmells()
    {
        foreach (OdorType smell in System.Enum.GetValues(typeof(OdorType)))
        {
            StopSmell(smell);
        }
    }
}