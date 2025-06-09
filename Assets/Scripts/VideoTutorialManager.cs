using System;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class VideoTutorialManager
{
    [SerializeField] private VideoPlayer _videoPlayer;

    public bool isClickPlayVideo = false;
    public bool _isRewindToLast3Seconds = false;
    public bool _isSkipForward3Seconds = false;

    private bool _lastPlayState = false;

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