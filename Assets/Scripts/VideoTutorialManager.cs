using UnityEngine;
using UnityEngine.Video;

public class VideoTutorialManager : MonoBehaviour
{
    private VideoPlayer _videoPlayer;

    [SerializeField] private bool isClickPlayVideo = false;
    [SerializeField] private bool _isRewindToLast10Seconds = false;
    [SerializeField] private bool _isSkipForward10Seconds = false;

    private bool _lastPlayState = false;
    void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    void Update()
    {
        TogglePlayPause();
        RewindToLast10Seconds();
        SkipForward10Seconds(); 
    }

    public void TogglePlayPause()
    {
        if (isClickPlayVideo == _lastPlayState)
            return;

        _lastPlayState = isClickPlayVideo;

        if (isClickPlayVideo)
        {
            _videoPlayer.Play();
        }
        else
        {
            _videoPlayer.Pause();
        }
    }

    public void RewindToLast10Seconds()
    {
        if (!_isRewindToLast10Seconds)
            return;

        if (_videoPlayer.time > 10)
        {
            _videoPlayer.time -= 10;
        }
        else
        {
            _videoPlayer.time = 0;
        }

        _isRewindToLast10Seconds = false;
    }

    public void SkipForward10Seconds()
    {
        if (!_isSkipForward10Seconds)
            return;

        if (_videoPlayer.time < _videoPlayer.length - 10)
        {
            _videoPlayer.time += 10;
        }
        else
        {
            _videoPlayer.time = _videoPlayer.length;
        }

        _isSkipForward10Seconds = false;
    }
}