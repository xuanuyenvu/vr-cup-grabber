using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pouring Data", menuName = "Liquid/Pouring Data")]
public class PouringData : ScriptableObject
{
    [Serializable]
    public class AngleLevel
    {
        public float angle; // Góc nghiêng (độ)
        public float levelPercent; // Phần trăm nước còn lại (0-1)
        public float pourRate = 1f; // Tốc độ đổ ở góc này
    }
    
    public string containerName = "Default Cup"; // Tên của loại cốc
    public float minPourAngle = 5f; // Góc tối thiểu để bắt đầu đổ
    public float pourDelay = 0.05f; // Độ trễ khi đổ (giây)
    
    [Tooltip("Thiết lập mức nước ở các góc nghiêng khác nhau")]
    public AngleLevel[] angleLevels = new AngleLevel[] 
    {
        new AngleLevel { angle = 0, levelPercent = 1.0f, pourRate = 1f },
        new AngleLevel { angle = 5, levelPercent = 0.948f, pourRate = 1f },
        new AngleLevel { angle = 10, levelPercent = 0.8f, pourRate = 1f },
        new AngleLevel { angle = 15, levelPercent = 0.6f, pourRate = 1f },
        new AngleLevel { angle = 20, levelPercent = 0.3f, pourRate = 1f },
        new AngleLevel { angle = 25, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 30, levelPercent = 0.0f, pourRate = 1f },
        new AngleLevel { angle = 35, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 40, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 45, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 50, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 55, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 60, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 65, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 70, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 75, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 80, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 85, levelPercent = 0.1f, pourRate = 1f },
        new AngleLevel { angle = 90, levelPercent = 0f, pourRate = 1f },

    };

    // Đảm bảo dữ liệu đã được sắp xếp theo góc tăng dần
    private void OnValidate()
    {
        Array.Sort(angleLevels, (a, b) => a.angle.CompareTo(b.angle));
    }
}