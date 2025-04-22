using System;
using System.Collections;
using System.Collections.Generic;
using LiquidVolumeFX;
using UnityEngine;

public class PouringLiquid : MonoBehaviour
{
    [SerializeField] private LiquidVolume liquidVolume;
    [SerializeField] private float initialLevel = 0.88f; // Mức đầy ban đầu
    
    [Tooltip("Pour data for this cup type")]
    [SerializeField] private PouringData pouringData;

    private float _cupInitRotationX;
    private float _currentLevel;
    private float _targetLevel;
    private float _lastPourTime;
    private float _steadyAngleTime = 0f; // Thời gian giữ ở một góc ổn định

    private void Start()
    {
        _cupInitRotationX = transform.rotation.eulerAngles.x;
        
        // Thiết lập mức ban đầu
        _currentLevel = initialLevel;
        _targetLevel = initialLevel;
        liquidVolume.level = initialLevel;
        
        // Kiểm tra xem có dữ liệu đổ nước không
        if (pouringData == null)
        {
            Debug.LogWarning("Pouring Data not found! Assign a ScriptableObject PouringData to the Inspector.");
        }
    }

    void Update()
    {
        // Nếu không có dữ liệu đổ nước, không xử lý
        if (pouringData == null)
            return;
            
        // Tính góc nghiêng hiện tại
        float currentAngleDeg = Mathf.Abs(transform.rotation.eulerAngles.x - _cupInitRotationX);
        // Chuẩn hóa về 0-90 độ
        if (currentAngleDeg > 180) currentAngleDeg = 360 - currentAngleDeg;
        
        // Giới hạn góc tối đa là 90 độ
        currentAngleDeg = Mathf.Clamp(currentAngleDeg, 0, 90);
        
        // Tìm mức nước mục tiêu dựa trên góc nghiêng
        float targetLevel = GetWaterLevelAtAngle(currentAngleDeg) * initialLevel;
        
        // Tính toán tốc độ đổ dựa trên góc nghiêng
        float pourRate = GetPourRateAtAngle(currentAngleDeg);
        
        // Tính toán thời gian giữ ở góc ổn định
        if (Mathf.Abs(_targetLevel - targetLevel) < 0.01f && currentAngleDeg > pouringData.minPourAngle)
        {
            _steadyAngleTime += Time.deltaTime;
        }
        else
        {
            _steadyAngleTime = 0f;
            _targetLevel = targetLevel;
        }
        
        // Dần dần đổ nước nếu góc nghiêng đủ lớn
        if (currentAngleDeg > pouringData.minPourAngle && 
            Time.time - _lastPourTime > pouringData.pourDelay && 
            _currentLevel > targetLevel)
        {
            // Tốc độ đổ tăng theo thời gian giữ ở một góc ổn định
            float timeMultiplier = Mathf.Lerp(1f, 3f, Mathf.Clamp01(_steadyAngleTime / 2f));
            
            // Giảm mức nước dần dần
            _currentLevel = Mathf.MoveTowards(_currentLevel, targetLevel, 
                                           Time.deltaTime * pourRate * timeMultiplier);
            _lastPourTime = Time.time;
            
            // Cập nhật mức nước
            liquidVolume.level = _currentLevel;
        }
        
        // Nếu góc nhỏ và mục tiêu cao hơn mức hiện tại, không thay đổi mức
        if (targetLevel > _currentLevel)
        {
            _targetLevel = _currentLevel;
        }
    }

    private float GetWaterLevelAtAngle(float angle)
    {
        var angleLevels = pouringData.angleLevels;
        
        // Nếu góc nhỏ hơn hoặc bằng góc đầu tiên, trả về mức nước đầu tiên
        if (angle <= angleLevels[0].angle)
            return angleLevels[0].levelPercent;
        
        // Nếu góc lớn hơn hoặc bằng góc cuối cùng, trả về mức nước cuối cùng
        if (angle >= angleLevels[angleLevels.Length - 1].angle)
            return angleLevels[angleLevels.Length - 1].levelPercent;
        
        // Tìm hai điểm để nội suy
        for (int i = 0; i < angleLevels.Length - 1; i++)
        {
            if (angle >= angleLevels[i].angle && angle <= angleLevels[i + 1].angle)
            {
                // Nội suy tuyến tính giữa hai điểm
                float t = (angle - angleLevels[i].angle) / (angleLevels[i + 1].angle - angleLevels[i].angle);
                return Mathf.Lerp(angleLevels[i].levelPercent, angleLevels[i + 1].levelPercent, t);
            }
        }
        
        // Mặc định trả về 0 (không nên xảy ra)
        return 0;
    }
    
    private float GetPourRateAtAngle(float angle)
    {
        var angleLevels = pouringData.angleLevels;
        
        // Nếu góc nhỏ hơn hoặc bằng góc đầu tiên, trả về tốc độ đầu tiên
        if (angle <= angleLevels[0].angle)
            return angleLevels[0].pourRate;
        
        // Nếu góc lớn hơn hoặc bằng góc cuối cùng, trả về tốc độ cuối cùng
        if (angle >= angleLevels[angleLevels.Length - 1].angle)
            return angleLevels[angleLevels.Length - 1].pourRate;
        
        // Tìm hai điểm để nội suy
        for (int i = 0; i < angleLevels.Length - 1; i++)
        {
            if (angle >= angleLevels[i].angle && angle <= angleLevels[i + 1].angle)
            {
                // Nội suy tuyến tính giữa hai điểm
                float t = (angle - angleLevels[i].angle) / (angleLevels[i + 1].angle - angleLevels[i].angle);
                return Mathf.Lerp(angleLevels[i].pourRate, angleLevels[i + 1].pourRate, t);
            }
        }
        
        // Mặc định trả về tốc độ trung bình
        return 0.5f;
    }
}