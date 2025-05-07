using Oculus.Interaction.HandGrab.Recorder;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class HandPoseRecorder : MonoBehaviour
{
    [SerializeField] private HandGrabPoseLiveRecorderCustom handGrabPoseLiveRecorderCustom;
    [SerializeField] private GameObject cup;
    public bool recording = false;
    public bool createPrefab = false;
    public string userId = null;
    
    void Update()
    {
        if (recording)
        {
            handGrabPoseLiveRecorderCustom.Record();
            recording = false;
        }
        if (createPrefab)
        {
            string localPath = "Assets/CupPrefabs/Cup" + userId + ".prefab";
            CreateNewPrefab(cup, localPath);
            createPrefab = false;
        }
    }

#if UNITY_EDITOR
    private void CreateNewPrefab(GameObject obj, string localPath)
    {
        UnityEngine.Object prefab = PrefabUtility.SaveAsPrefabAsset(obj, localPath);
    }
#endif
}
