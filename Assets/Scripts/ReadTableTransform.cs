using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class ReadTableTransform : MonoBehaviour
{
    [SerializeField] private GameObject Corner1;
    [SerializeField] private GameObject Corner2;
    [SerializeField] private GameObject Corner3;
    [SerializeField] private GameObject Corner4;
    [SerializeField] private GameObject centerMarker;
    [SerializeField] private GameObject vfxPointCloud;
    [SerializeField] private GameObject tableAxis;

    private List<Vector3> markerCorners = new List<Vector3>();

    void Start()
    {
        string cornersFilePath = Path.Combine(Application.streamingAssetsPath, "aruco_4_corners_final.csv");

        List<Dictionary<string, string>> ReadCSVWithHeader(string path)
        {
            var result = new List<Dictionary<string, string>>();

            using (StreamReader reader = new StreamReader(path))
            {
                string headerLine = reader.ReadLine();
                string[] headers = headerLine.Split(',');

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');

                    var entry = new Dictionary<string, string>();
                    for (int i = 0; i < headers.Length && i < values.Length; i++)
                    {
                        entry[headers[i]] = values[i];
                    }

                    result.Add(entry);
                }
            }

            return result;
        }

        List<Dictionary<string, string>> cornersData = ReadCSVWithHeader(cornersFilePath);
        if (cornersData.Count > 0)
        {
            if (cornersData[0].TryGetValue("Corner1_X", out string corner1X) && cornersData[0].TryGetValue("Corner1_Y", out string corner1Y) && cornersData[0].TryGetValue("Corner1_Z", out string corner1Z))
            {
                Corner1.transform.position = new Vector3(
                    float.Parse(corner1X, CultureInfo.InvariantCulture) * 0.001f,
                    float.Parse(corner1Y, CultureInfo.InvariantCulture) * -0.001f,
                    float.Parse(corner1Z, CultureInfo.InvariantCulture) * 0.001f
                );
                Debug.Log($"Corner1 X: {corner1X}, Y: {corner1Y}, Z: {corner1Z}");
                markerCorners.Add(Corner1.transform.position);
            }
            if (cornersData[0].TryGetValue("Corner2_X", out string corner2X) && cornersData[0].TryGetValue("Corner2_Y", out string corner2Y) && cornersData[0].TryGetValue("Corner2_Z", out string corner2Z))
            {
                Corner2.transform.position = new Vector3(
                    float.Parse(corner2X, CultureInfo.InvariantCulture) * 0.001f,
                    float.Parse(corner2Y, CultureInfo.InvariantCulture) * -0.001f,
                    float.Parse(corner2Z, CultureInfo.InvariantCulture) * 0.001f
                );
                markerCorners.Add(Corner2.transform.position);
            }
            if (cornersData[0].TryGetValue("Corner3_X", out string corner3X) && cornersData[0].TryGetValue("Corner3_Y", out string corner3Y) && cornersData[0].TryGetValue("Corner3_Z", out string corner3Z))
            {
                Corner3.transform.position = new Vector3(
                    float.Parse(corner3X, CultureInfo.InvariantCulture) * 0.001f,
                    float.Parse(corner3Y, CultureInfo.InvariantCulture) * -0.001f,
                    float.Parse(corner3Z, CultureInfo.InvariantCulture) * 0.001f
                );
                markerCorners.Add(Corner3.transform.position);
            }
            if (cornersData[0].TryGetValue("Corner4_X", out string corner4X) && cornersData[0].TryGetValue("Corner4_Y", out string corner4Y) && cornersData[0].TryGetValue("Corner4_Z", out string corner4Z))
            {
                Corner4.transform.position = new Vector3(
                    float.Parse(corner4X, CultureInfo.InvariantCulture) * 0.001f,
                    float.Parse(corner4Y, CultureInfo.InvariantCulture) * -0.001f,
                    float.Parse(corner4Z, CultureInfo.InvariantCulture) * 0.001f
                );
                markerCorners.Add(Corner4.transform.position);
            }
        }

        if (markerCorners.Count == 4)
        {
            vfxPointCloud.transform.position = new Vector3(0, 0, 0);
            vfxPointCloud.transform.rotation = Quaternion.Euler(0, 0, 0);
            vfxPointCloud.transform.localScale = new Vector3(-1, 1, 1);

            Vector3 center = (markerCorners[0] + markerCorners[1] + markerCorners[2] + markerCorners[3]) / 4f;
            centerMarker.transform.position = center;

            Vector3 vectorX = markerCorners[3] - markerCorners[0];
            Vector3 vectorZ = markerCorners[1] - markerCorners[0];
            Quaternion rotation = Quaternion.LookRotation(vectorZ, vectorX);
            centerMarker.transform.rotation = rotation * Quaternion.Euler(0, 0, 90);

            Vector3 localPosOfKinectInCenterMarker = centerMarker.transform.InverseTransformPoint(vfxPointCloud.transform.position);
            Quaternion localRotationOfKinectInCenterMarker = Quaternion.Inverse(centerMarker.transform.rotation) * vfxPointCloud.transform.rotation;

            Vector3 localPosOfCorner1InCenterMarker = centerMarker.transform.InverseTransformPoint(Corner1.transform.position);
            Vector3 localPosOfCorner2InCenterMarker = centerMarker.transform.InverseTransformPoint(Corner2.transform.position);
            Vector3 localPosOfCorner3InCenterMarker = centerMarker.transform.InverseTransformPoint(Corner3.transform.position);
            Vector3 localPosOfCorner4InCenterMarker = centerMarker.transform.InverseTransformPoint(Corner4.transform.position);


            centerMarker.transform.position = tableAxis.transform.position;
            centerMarker.transform.rotation = tableAxis.transform.rotation;
            vfxPointCloud.transform.position = centerMarker.transform.TransformPoint(localPosOfKinectInCenterMarker);
            vfxPointCloud.transform.rotation = centerMarker.transform.rotation * localRotationOfKinectInCenterMarker;

            Corner1.transform.position = centerMarker.transform.TransformPoint(localPosOfCorner1InCenterMarker);
            Corner2.transform.position = centerMarker.transform.TransformPoint(localPosOfCorner2InCenterMarker);
            Corner3.transform.position = centerMarker.transform.TransformPoint(localPosOfCorner3InCenterMarker);
            Corner4.transform.position = centerMarker.transform.TransformPoint(localPosOfCorner4InCenterMarker);
        }
    }
}
