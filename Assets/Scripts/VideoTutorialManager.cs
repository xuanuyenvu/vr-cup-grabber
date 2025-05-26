using UnityEngine;
using UnityEngine.Video;

public class VideoTutorialManager : MonoBehaviour
{
    private VideoPlayer _videoPlayer;

    [SerializeField] private bool isClickPlayVideo = false;
    [SerializeField] private bool _isRewindToLast3Seconds = false;
    [SerializeField] private bool _isSkipForward3Seconds = false;

    private bool _lastPlayState = false;
    void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    void Update()
    {
        TogglePlayPause();
        RewindToLast3Seconds();
        SkipForward3Seconds(); 
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

    public void RewindToLast3Seconds()
    {
        if (!_isRewindToLast3Seconds)
            return;

        if (_videoPlayer.time > 3)
        {
            _videoPlayer.time -= 3;
        }
        else
        {
            _videoPlayer.time = 0;
        }

        _isRewindToLast3Seconds = false;
    }

    public void SkipForward3Seconds()
    {
        if (!_isSkipForward3Seconds)
            return;

        if (_videoPlayer.time < _videoPlayer.length - 3)
        {
            _videoPlayer.time += 3;
        }
        else
        {
            _videoPlayer.time = _videoPlayer.length;
        }

        _isSkipForward3Seconds = false;
    }
}