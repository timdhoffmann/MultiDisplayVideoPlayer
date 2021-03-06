﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
//using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    [SerializeField]
    private Text debugText;
    [SerializeField]
    private Text video1StateDisplay;
    [SerializeField]
    private Text video2StateDisplay;
    [SerializeField]
    private AudioSource audioSource;

    private VideoPlayer videoPlayer1;
    private VideoPlayer videoPlayer2;
    private DisplayManager displayManager;

    private bool playbackIsStarted = false;
    private double maxTimingError = 0d;
    private long framesDropped = 0;

    #region PROPERTIES
    public string[] VideoFilePaths { get; set; }
    #endregion

    // Use this for initialization
    void Start ()
    {
        InitializeReferences();
        InitializeVideoPlayer();
    }

    private void InitializeVideoPlayer ()
    {
        // We want to play from a URL.
        // Set the source mode FIRST, before changing audio settings;
        // as setting the source mode will reset those.
        videoPlayer1.source = VideoSource.Url;

        videoPlayer1.audioOutputMode = VideoAudioOutputMode.AudioSource;

        // We want to control one audio track with the video player
        videoPlayer1.controlledAudioTrackCount = 1;

        // We enable the first track, which has the id zero
        videoPlayer1.EnableAudioTrack(0, true);

        // ...and we set the audio source for this track
        videoPlayer1.SetTargetAudioSource(0, audioSource);

        // External reference clock the VideoPlayer observes to detect and correct drift.
        videoPlayer1.timeReference = VideoTimeReference.InternalTime;
        videoPlayer2.timeReference = videoPlayer1.timeReference;

        Debug.Log("video1 timeReference: " + videoPlayer1.timeReference);
        Debug.Log("video2 timeReference: " + videoPlayer2.timeReference);

        //video1debugText.enabled = true;
        //video2debugText.enabled = true;
        video2StateDisplay.text = "Preparing video for playback.";
    }

    private void InitializeReferences ()
    {
        videoPlayer1 = Camera.main.GetComponent<VideoPlayer>();
        Assert.IsNotNull(videoPlayer1);

        videoPlayer2 = GameObject.FindGameObjectWithTag("Display2Camera").GetComponent<VideoPlayer>();
        Assert.IsNotNull(videoPlayer2);

        displayManager = FindObjectOfType<DisplayManager>();
        Assert.IsNotNull(displayManager);

        Assert.IsNotNull(debugText);
        Assert.IsNotNull(video1StateDisplay);
        Assert.IsNotNull(video2StateDisplay);

        Assert.IsNotNull(audioSource);
    }

    private void Update ()
    {
        if (!playbackIsStarted)
        {
            UpdateVideoPlayerState();
        }

        //if (Input.GetKeyDown("space") && videoPlayer1.isPrepared && videoPlayer2.isPrepared)
        //{
        //    TogglePlayback();
        //}

        if (playbackIsStarted)
        {
            // Reference time of the external clock the VideoPlayer uses to correct its drift.
            // Only relevant when VideoPlayer.timeReference is set to VideoTimeReference.ExternalTime.
            //videoPlayer2.externalReferenceTime = videoPlayer1.time;

            long frameError = System.Math.Abs(videoPlayer1.frame - videoPlayer2.frame);

            double timingError = System.Math.Abs(videoPlayer1.time - videoPlayer2.time);

            if (timingError > maxTimingError)
            {
                maxTimingError = timingError;
            }

            if (frameError > 0)
            {
                framesDropped += frameError;
            }
        }
    }

    private void UpdateVideoPlayerState ()
    {
        if (!videoPlayer1.isPrepared)
        {
            video1StateDisplay.text = "Preparing video for playback.";
            debugText.color = Color.red;
        }

        if (!videoPlayer2.isPrepared)
        {
            video2StateDisplay.text = "Preparing video for playback.";
            debugText.color = Color.red;
        }
    }

    private void OnVideoPlayer1Prepared (VideoPlayer vPlayer)
    {
        video1StateDisplay.text = "Video is ready for playback.";
        video1StateDisplay.color = Color.green;
    }

    private void OnVideoPlayer2Prepared (VideoPlayer vPlayer)
    {
        video2StateDisplay.text = "Video is ready for playback.";
        video2StateDisplay.color = Color.green;
    }

    #region PUBLIC METHODS
    public void SetVideoUrl (string filePath, int targetDisplay)
    {
        if (targetDisplay == 1)
        {
            videoPlayer1.url = filePath;

            videoPlayer1.Prepare();
            videoPlayer1.prepareCompleted += OnVideoPlayer1Prepared;

        }
        else if (targetDisplay == 2)
        {
            videoPlayer2.url = filePath;

            videoPlayer2.Prepare();
            videoPlayer2.prepareCompleted += OnVideoPlayer2Prepared;
        }
    }

    public void TogglePlayback ()
    {
        if (!videoPlayer1.isPlaying)
        {
            videoPlayer1.Play();
            videoPlayer2.Play();
        }
        else
        {
            videoPlayer1.Stop();
            videoPlayer2.Stop();

            debugText.text += "\n max. Timing Error: " + maxTimingError + "\n Frames dropped: " + framesDropped;

            videoPlayer1.Prepare();
            videoPlayer2.Prepare();
        }
        playbackIsStarted = !playbackIsStarted;
    } 
    #endregion
}
