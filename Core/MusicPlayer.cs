﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EnsoMusicPlayer
{
    [AddComponentMenu("Audio/Ensō Music Player")]
    public class MusicPlayer : MonoBehaviour
    {
        [Header("Volume Settings")]
        [Range(0f, 1f)]
        public float Volume = 1f; // Should be considered readonly outside the player's scope.
        public float CrossFadeTime = 2f;

        [Header("TrackSettings")]
        public List<MusicTrack> Tracks;

        public MusicTrack PlayingTrack { get; private set; }

        private Speaker PrimarySpeaker;
        private Speaker SecondarySpeaker;
        private Speaker CurrentSpeaker;

        // Use this for initialization
        void Awake()
        {
            PrimarySpeaker = gameObject.AddComponent<Speaker>();
            SecondarySpeaker = gameObject.AddComponent<Speaker>();
            PrimarySpeaker.SetPlayerAndInitializeVolume(this);
            SecondarySpeaker.SetPlayerAndInitializeVolume(this);
            CurrentSpeaker = PrimarySpeaker;

            // Cache all the clips before we play them for maximum performance when starting playback.
            if (Tracks != null)
            {
                foreach (MusicTrack track in Tracks)
                {
                    track.CreateAndCacheClips();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
        }

        #region PublicAPI

        /// <summary>
        /// The event handler that is called when FadeOut completes.
        /// </summary>
        public event MusicPlayerEventHandler FadeOutComplete;

        /// <summary>
        /// The event handler that is called when FadeIn completes.
        /// </summary>
        public event MusicPlayerEventHandler FadeInComplete;

        /// <summary>
        /// The event handler that is called when a track ends or loops.
        /// </summary>
        public event MusicPlayerEventHandler TrackEndOrLoop;

        /// <summary>
        /// The event handler that is called when a track ends from finite play.
        /// </summary>
        public event MusicPlayerEventHandler TrackEnd;

        /// <summary>
        /// The event handler that is called when a track loops.
        /// </summary>
        public event MusicPlayerEventHandler TrackLoop;

        /// <summary>
        /// The time of the current track.
        /// </summary>
        public float CurrentTime
        {
            get
            {
                return CurrentSpeaker.CurrentTime;
            }
        }

        /// <summary>
        /// The length of the current track.
        /// </summary>
        public float CurrentLength
        {
            get
            {
                return CurrentSpeaker.CurrentLength;
            }
        }

        /// <summary>
        /// Play a music track.
        /// </summary>
        /// <param name="name">The name of the track</param>
        /// <param name="timesToLoop">The number of times to loop the track. Set to 0 for endless play.</param>
        public void Play(string name, int timesToLoop = 0)
        {
            Play(GetTrackByName(name), timesToLoop);
        }

        /// <summary>
        /// Play a music track.
        /// </summary>
        /// <param name="track">The track to play</param>
        /// <param name="timesToLoop">The number of times to loop the track. Set to 0 for endless play.</param>
        public void Play(MusicTrack track, int timesToLoop = 0)
        {
            PlayingTrack = track;
            PlayAtPoint(track, 0f, timesToLoop);
        }

        /// <summary>
        /// Scrubs the currently-playing track to a specific point in its timeline.
        /// </summary>
        /// <param name="time">How far along to scrub the track, in seconds</param>
        public void Scrub(float time)
        {
            if (PlayingTrack != null)
            {
                time = Mathf.Min(time, PlayingTrack.LengthInSeconds);
                CurrentSpeaker.SetPosition(PlayingTrack, PlayingTrack.SecondsToSamples(time));
            }
        }

        /// <summary>
        /// Scrubs the currently-playing track to a specific point in its timeline.
        /// </summary>
        /// <param name="percentage">How far along to scrub the track, as a percentage of the track's length</param>
        public void ScrubAsPercentage(float percentage)
        {
            if (PlayingTrack != null)
            {
                Scrub(percentage * PlayingTrack.LengthInSeconds);
            }
        }

        /// <summary>
        /// Plays the track starting at the given point on its timeline.
        /// </summary>
        /// <param name="track">The track to play</param>
        /// <param name="time">The time to play the track at, in seconds</param>
        /// <param name="timesToLoop">The number of times to loop the track. Set to 0 for endless play.</param>
        public void PlayAtPoint(MusicTrack track, float time, int timesToLoop = 0)
        {
            CurrentSpeaker.SetVolume(Volume);
            CurrentSpeaker.PlayAtPoint(track, time, timesToLoop);
        }

        /// <summary>
        /// Fades in the track with the given name starting at the given point on its timeline.
        /// </summary>
        /// <param name="name">The name of the track</param>
        /// <param name="time">The time to fade the track in at, in seconds</param>
        /// <param name="timesToLoop">The number of times to loop the track. Set to 0 for endless play.</param>
        public void FadeInAtPoint(string name, float time, int timesToLoop = 0)
        {
            FadeInAtPoint(GetTrackByName(name), time, timesToLoop);
        }

        /// <summary>
        /// Fades in the given track starting at the given point on its timeline.
        /// </summary>
        /// <param name="track">The track to play</param>
        /// <param name="time">The playback time for the next track, in seconds</param>
        /// <param name="timesToLoop">The number of times to loop the track. Set to 0 for endless play.</param>
        public void FadeInAtPoint(MusicTrack track, float time, int timesToLoop = 0)
        {
            PlayingTrack = track;
            CurrentSpeaker.PlayAtPoint(track, time, timesToLoop);
            CurrentSpeaker.FadeIn(CrossFadeTime);
        }

        /// <summary>
        /// Crossfades to the track with the given name at the given point on its timeline.
        /// </summary>
        /// <param name="name">The name of the track</param>
        /// <param name="time">The playback time for the next track, in seconds</param>
        /// <param name="timesToLoop">The number of times to loop the track. Set to 0 for endless play.</param>
        public void CrossFadeAtPoint(string name, float time, int timesToLoop = 0)
        {
            CrossFadeAtPoint(GetTrackByName(name), time, timesToLoop);
        }

        /// <summary>
        /// Crossfades to the given track at the given point on its timeline.
        /// </summary>
        /// <param name="track">The track to crossfade to</param>
        /// <param name="time">The playback time for the next track, in seconds</param>
        /// <param name="timesToLoop">The number of times to loop the track. Set to 0 for endless play.</param>
        public void CrossFadeAtPoint(MusicTrack track, float time, int timesToLoop = 0)
        {
            CrossFadeTo(track, timesToLoop);
            Scrub(time);
            CurrentSpeaker.FadeIn(CrossFadeTime);
        }

        /// <summary>
        /// Crossfades to a track.
        /// </summary>
        /// <param name="name">The name of the track to play</param>
        /// <param name="timesToLoop">The number of times to loop the track. Set to 0 for endless play.</param>
        public void CrossFadeTo(string name, int timesToLoop = 0)
        {
            CrossFadeTo(GetTrackByName(name), timesToLoop);
        }

        /// <summary>
        /// Crossfades to a track.
        /// </summary>
        /// <param name="track">The track to play</param>
        public void CrossFadeTo(MusicTrack track, int timesToLoop = 0)
        {
            CurrentSpeaker.FadeOut(CrossFadeTime, true);
            SwitchSpeakers();
            PlayingTrack = track;
            CurrentSpeaker.Play(PlayingTrack, timesToLoop);
            CurrentSpeaker.FadeIn(CrossFadeTime);
        }

        /// <summary>
        /// Pauses the current track.
        /// </summary>
        public void Pause()
        {
            CurrentSpeaker.Pause();
        }

        /// <summary>
        /// Unpauses the current track.
        /// </summary>
        public void Unpause()
        {
            CurrentSpeaker.UnPause();
        }

        /// <summary>
        /// Fades the currently-playing track in.
        /// </summary>
        public void FadeIn()
        {
            CurrentSpeaker.FadeIn(CrossFadeTime);
        }

        /// <summary>
        /// Fades the currently-playing track out.
        /// </summary>
        public void FadeOut()
        {
            CurrentSpeaker.FadeOut(CrossFadeTime);
        }

        /// <summary>
        /// Sets the volume of the music player. Will do nothing if the player is in the middle of fading.
        /// </summary>
        /// <param name="volume">The volume level, from 0.0 to 1.0.</param>
        public void SetVolume(float volume)
        {
            if (!PrimarySpeaker.IsFading && !SecondarySpeaker.IsFading)
            {
                Volume = volume;
                RefreshSpeakerVolume();
            }
        }

        /// <summary>
        /// Stops the player.
        /// </summary>
        public void Stop()
        {
            PrimarySpeaker.Stop();
            SecondarySpeaker.Stop();
        }

        /// <summary>
        /// Gets a track by its playlist name.
        /// </summary>
        /// <param name="name">The name of the track</param>
        /// <returns>The track with the given playlist name</returns>
        public MusicTrack GetTrackByName(string name)
        {
            MusicTrack track
                = (from t in Tracks
                   where t.Name == name
                   select t).FirstOrDefault();

            if (track == null)
            {
                throw new KeyNotFoundException(string.Format(@"A song with the name ""{0}"" could not be found.", name));
            }

            return track;
        }

        #endregion

        /// <summary>
        /// Called by a speaker that completes a fadeout.
        /// </summary>
        public void OnFadeOutComplete()
        {
            OnFadeOutComplete(new MusicPlayerEventArgs());
        }

        /// <summary>
        /// Called by a speaker that completes a fade in.
        /// </summary>
        public void OnFadeInComplete()
        {
            OnFadeInComplete(new MusicPlayerEventArgs());
        }

        /// <summary>
        /// Called by a speaker that has a track loop or end.
        /// </summary>
        public void OnTrackEndOrLoop()
        {
            OnTrackEndOrLoop(new MusicPlayerEventArgs());
        }

        /// <summary>
        /// Called by a speaker that has a track end finite play.
        /// </summary>
        public void OnTrackEnd()
        {
            OnTrackEnd(new MusicPlayerEventArgs());
        }

        /// <summary>
        /// Called by a speaker that has a track loop. Does not get called if it ended finite play.
        /// </summary>
        public void OnTrackLoop()
        {
            OnTrackLoop(new MusicPlayerEventArgs());
        }

        /// <summary>
        /// Called when a speaker completes a fadeout.
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void OnFadeOutComplete(MusicPlayerEventArgs e)
        {
            if (FadeOutComplete != null)
            {
                FadeOutComplete(e);
            }
        }

        /// <summary>
        /// Called when a speaker completes a fade in.
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void OnFadeInComplete(MusicPlayerEventArgs e)
        {
            if (FadeInComplete != null)
            {
                FadeInComplete(e);
            }
        }

        /// <summary>
        /// Called when a track on the current speaker loops or ends.
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void OnTrackEndOrLoop(MusicPlayerEventArgs e)
        {
            if (TrackEndOrLoop != null)
            {
                TrackEndOrLoop(e);
            }
        }

        /// <summary>
        /// Called when a track on the current speaker ends in finite play.
        /// </summary>
        /// <param name="e"></param>
        private void OnTrackEnd(MusicPlayerEventArgs e)
        {
            if (TrackEnd != null)
            {
                TrackEnd(e);
            }
        }

        /// <summary>
        /// Called when a track on the current speaker loops.
        /// </summary>
        /// <param name="e"></param>
        private void OnTrackLoop(MusicPlayerEventArgs e)
        {
            if (TrackLoop != null)
            {
                TrackLoop(e);
            }
        }

        /// <summary>
        /// Switches the primary speaker to the secondary one and vice versa.. Useful while crossfading.
        /// </summary>
        private void SwitchSpeakers()
        {
            if (CurrentSpeaker == PrimarySpeaker)
            {
                CurrentSpeaker = SecondarySpeaker;
            }
            else
            {
                CurrentSpeaker = PrimarySpeaker;
            }
        }

        /// <summary>
        /// Updates the modules' volume to match the music player's.
        /// </summary>
        private void RefreshSpeakerVolume()
        {
            PrimarySpeaker.SetVolume(Volume);
            SecondarySpeaker.SetVolume(Volume);
        }
    }
}