﻿using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EnsoMusicPlayer
{
    public class Enso_MusicPlayerTests
    {
        MusicPlayer musicPlayer;
        Speaker module;
        Speaker module2;
        AudioSource speaker1;
        AudioSource speaker2;
        AudioSource speaker3;
        AudioSource speaker4;

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator Enso_SpeakerHasAudioSource()
        {
            SetUpMusicPlayer();

            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;

            Assert.IsNotNull(speaker1.GetComponent<AudioSource>());
            Assert.IsNotNull(speaker2.GetComponent<AudioSource>()); 
        }

        [UnityTest]
        public IEnumerator Enso_FadeOutTrack()
        {
            SetUpMusicPlayer();

            yield return null;

            MusicTrack track = new MusicTrack
            {
                Track = AudioClip.Create("test", 2000, 1, 1000, false)
            };
            track.CreateAndCacheClips();
            module.Play(track, EnsoConstants.PlayEndlessly);

            float originalVolume = speaker1.volume;

            yield return null;

            module.FadeOut(2);

            Assert.AreEqual(Speaker.VolumeStatuses.FadingOut, module.VolumeStatus);

            yield return null;

            Assert.AreNotEqual(speaker1.volume, originalVolume, "The volume isn't changing when fading out.");

            yield return new WaitForSecondsRealtime(2);

            Assert.AreEqual(Speaker.VolumeStatuses.Static, module.VolumeStatus);
            Assert.AreEqual(speaker1.volume, 0f, "Speaker volume doesn't equal 0 when fading out is complete.");
        }

        [UnityTest]
        public IEnumerator Enso_PlayAfterFadeOut()
        {
            SetUpMusicPlayer();

            yield return null;

            MusicTrack track = new MusicTrack
            {
                Track = AudioClip.Create("test", 2000, 1, 1000, false)
            };
            track.CreateAndCacheClips();

            musicPlayer.Play(track);

            yield return null;

            musicPlayer.FadeOut();

            yield return new WaitForSeconds(2);

            Assert.IsTrue(speaker1.volume <= 0f, "Speaker should be muted after fadeout.");

            musicPlayer.Play("MusicTest");

            yield return null;

            Assert.IsTrue(speaker1.volume == musicPlayer.Volume, "Speaker1 should be back at player volume after PlayTrack() is called.");
            Assert.IsTrue(speaker2.volume == musicPlayer.Volume, "Speaker2 should be back at player volume after PlayTrack() is called.");
            Assert.IsTrue(speaker3.volume == musicPlayer.Volume, "Speaker3 should be back at player volume after PlayTrack() is called.");
            Assert.IsTrue(speaker4.volume == musicPlayer.Volume, "Speaker4 should be back at player volume after PlayTrack() is called.");
        }

        [UnityTest]
        public IEnumerator Enso_PlayWhileFadingOut()
        {
            SetUpMusicPlayer();

            yield return null;

            MusicTrack track = new MusicTrack
            {
                Track = AudioClip.Create("test", 2000, 1, 1000, false)
            };
            track.CreateAndCacheClips();

            musicPlayer.Play(track);

            yield return null;

            musicPlayer.FadeOut();

            musicPlayer.Play("MusicTest");

            yield return new WaitForSeconds(2);

            Assert.IsTrue(speaker1.volume == musicPlayer.Volume, "Speaker1 should be back at player volume after PlayTrack() is called.");
            Assert.IsTrue(speaker2.volume == musicPlayer.Volume, "Speaker2 should be back at player volume after PlayTrack() is called.");
            Assert.IsTrue(speaker3.volume == musicPlayer.Volume, "Speaker3 should be back at player volume after PlayTrack() is called.");
            Assert.IsTrue(speaker4.volume == musicPlayer.Volume, "Speaker4 should be back at player volume after PlayTrack() is called.");
        }

        [UnityTest]
        public IEnumerator Enso_SetVolume()
        {
            SetUpMusicPlayer();

            yield return null;

            musicPlayer.SetVolume(.5f);

            yield return null;

            Assert.AreEqual(speaker1.volume, musicPlayer.Volume);
            Assert.AreEqual(speaker2.volume, musicPlayer.Volume);
        }

        [UnityTest]
        public IEnumerator Enso_FadeTo()
        {
            SetUpMusicPlayer();
            musicPlayer.CrossFadeTime = .5f;

            yield return null;

            musicPlayer.FadeTo(.5f);

            yield return new WaitForSeconds(musicPlayer.CrossFadeTime);

            Assert.AreEqual(.5f, module.Volume);

            musicPlayer.FadeTo(.75f);

            yield return new WaitForSeconds(musicPlayer.CrossFadeTime);

            Assert.AreEqual(.75f, module.Volume);
        }

        [UnityTest]
        public IEnumerator Enso_FadeInTrack()
        {
            SetUpMusicPlayer();

            yield return null;

            musicPlayer.CrossFadeTime = 1f;
            musicPlayer.SetVolume(.75f);
            MusicTrack track = new MusicTrack
            {
                Track = AudioClip.Create("test", 2000, 1, 1000, false)
            };
            track.CreateAndCacheClips();
            module.Play(track, EnsoConstants.PlayEndlessly);
            module.SetSpeakerVolume(0f);

            yield return null;

            module.FadeIn(musicPlayer.CrossFadeTime);
            float originalVolume = speaker1.volume;

            Assert.AreEqual(Speaker.VolumeStatuses.FadingIn, module.VolumeStatus);

            yield return null;

            Assert.AreNotEqual(speaker1.volume, originalVolume, "The volume isn't changing when fading in.");

            yield return new WaitForSeconds(2);

            Assert.AreEqual(Speaker.VolumeStatuses.Static, module.VolumeStatus);
            Assert.AreEqual(musicPlayer.Volume, speaker1.volume, "Speaker volume doesn't equal 1 when fading in is complete.");
        }

        [UnityTest]
        public IEnumerator Enso_DontChangeVolumeWhileFading()
        {
            SetUpMusicPlayer();

            yield return null;

            MusicTrack track = new MusicTrack
            {
                Track = AudioClip.Create("test", 4000, 1, 1000, false),
                LoopStart = 1000,
                LoopLength = 1000
            };
            track.CreateAndCacheClips();
            musicPlayer.Play(track);

            yield return null;

            musicPlayer.FadeIn();
            float originalVolume = speaker1.volume;

            musicPlayer.SetVolume(.5f);

            Assert.AreNotEqual(speaker1.volume, .5f, "Volume should not be changeable while fading in.");

            yield return new WaitForSeconds(2);

            musicPlayer.FadeOut();
            musicPlayer.SetVolume(.5f);

            Assert.AreNotEqual(speaker1.volume, .5f, "Volume should not be changeable while fading out.");
        }

        [UnityTest]
        public IEnumerator Enso_NullLoopPointsShouldNotThrowException()
        {
            SetUpMusicPlayer();

            yield return null;

            MusicTrack track = new MusicTrack
            {
                Track = AudioClip.Create("test", 10000, 1, 1000, false),
            };
            track.CreateAndCacheClips();
        }

        [UnityTest]
        public IEnumerator Enso_ScrubbingToEndsOfTrackShouldNotThrowError()
        {
            SetUpMusicPlayer();

            yield return null;

            MusicTrack track = new MusicTrack
            {
                Track = AudioClip.Create("test", 13803264, 2, 44100, false),
                LoopStart = 11130,
                LoopLength = 6477145
            };
            track.CreateAndCacheClips();

            yield return null;

            musicPlayer.Play(track);

            yield return null;

            musicPlayer.Scrub(152.098f);

            yield return null;

            musicPlayer.ScrubAsPercentage(.97f);
        }

        [UnityTest]
        public IEnumerator Enso_PlayShouldSetCurrentTrack()
        {
            // Arrange
            SetUpMusicPlayer();

            // Play(string) test
            // Act
            musicPlayer.Play("MusicTest");

            yield return null;

            // Assert
            Assert.IsNotNull(musicPlayer.PlayingTrack);

            // Play(MusicTrack) test
            // Arrange
            MusicTrack track = new MusicTrack
            {
                Track = AudioClip.Create("test", 1000000, 1, 1000, false)
            };
            track.CreateAndCacheClips();

            // Act
            musicPlayer.Stop();
            musicPlayer.Play(track);

            yield return null;

            // Assert
            Assert.IsNotNull(musicPlayer.PlayingTrack);
        }

        [UnityTest]
        public IEnumerator Enso_CrossfadeShouldSetCurrentTrack()
        {
            // Arrange
            SetUpMusicPlayer();
            MusicTrack track = new MusicTrack
            {
                Name = "test",
                Track = AudioClip.Create("test", 1000000, 1, 1000, false)
            };
            track.CreateAndCacheClips();

            musicPlayer.Tracks.Add(track);

            // Act
            musicPlayer.Play("MusicTest");

            yield return null;

            musicPlayer.CrossFadeTo("test");

            yield return null;

            // Assert
            Assert.AreNotSame(musicPlayer.PlayingTrack, musicPlayer.Tracks.Where(x => x.Name == "MusicTest").First());
        }

        [UnityTest]
        public IEnumerator Enso_FadeInAtPoint()
        {
            // Arrange
            SetUpMusicPlayer();
            float fadeInPoint = 3f;

            // Act
            musicPlayer.FadeInAtPoint("MusicTest", fadeInPoint);

            yield return null;

            // Assert
            Assert.IsTrue(musicPlayer.CurrentTime >= fadeInPoint);
            Assert.AreEqual(Speaker.VolumeStatuses.FadingIn, module.VolumeStatus);
        }

        [UnityTest]
        public IEnumerator Enso_VolumeShouldBeSetProperlyBeforeDuringAndAfterCrossfade()
        {
            // Arrange
            SetUpMusicPlayer();
            musicPlayer.CrossFadeTime = .5f;
            musicPlayer.SetVolume(.25f);

            // Act
            musicPlayer.Play("MusicTest");

            float originalVolume = module.LoopSource.volume;

            yield return null;

            musicPlayer.CrossFadeTo("MusicTest");

            Assert.AreEqual(originalVolume, module.Volume, "Volume is not the same for speaker 1 before crossfade");
            Assert.AreEqual(0f, module2.Volume, "Speaker 2 is not muted before crossfade");

            yield return new WaitForSeconds(musicPlayer.CrossFadeTime / 2f);

            Assert.AreNotEqual(0f, module2.Volume, "Speaker 2 isn't fading in during crossfade");

            yield return new WaitForSeconds(musicPlayer.CrossFadeTime / 2f);

            // Assert
            Assert.AreEqual(originalVolume, module2.LoopSource.volume, "Speaker 2 is not at the original volume after crossfade");
        }

        [UnityTest]
        public IEnumerator Enso_VolumeShouldBeSetProperlyAfterDoubleCrossfade()
        {
            // Arrange
            SetUpMusicPlayer();
            musicPlayer.CrossFadeTime = .5f;
            musicPlayer.SetVolume(.25f);

            // Act
            musicPlayer.Play("MusicTest");

            float originalVolume = module.LoopSource.volume;

            yield return null;

            musicPlayer.CrossFadeTo("MusicTest");

            yield return new WaitForSeconds(musicPlayer.CrossFadeTime);

            musicPlayer.CrossFadeTo("MusicTest");

            yield return new WaitForSeconds(musicPlayer.CrossFadeTime);

            // Assert
            Assert.AreNotEqual(0f, module.Volume, "Speaker 1 is not at the original volume after a double crossfade");
        }

        [UnityTest]
        public IEnumerator Enso_VolumeShouldNotBeDifferentAfterCrossfadeAtPoint()
        {
            // Arrange
            SetUpMusicPlayer();
            musicPlayer.CrossFadeTime = .5f;
            musicPlayer.SetVolume(.25f);

            // Act
            musicPlayer.Play("MusicTest");

            float originalVolume = module.LoopSource.volume;

            yield return null;

            musicPlayer.CrossFadeAtPoint("MusicTest", .1f);

            yield return new WaitForSeconds(musicPlayer.CrossFadeTime);

            // Assert
            Assert.AreEqual(originalVolume, module2.LoopSource.volume);
        }

        [UnityTest]
        public IEnumerator Enso_CrossFadeAtPoint()
        {
            // Arrange
            SetUpMusicPlayer();
            float fadeInPoint = 3f;

            // Act
            musicPlayer.Play("MusicTest");

            yield return null;

            musicPlayer.CrossFadeAtPoint("MusicTest", fadeInPoint);

            yield return null;

            // Assert
            Assert.IsTrue(musicPlayer.CurrentTime >= fadeInPoint);
            Assert.AreEqual(Speaker.VolumeStatuses.FadingOut, module.VolumeStatus);
            Assert.AreEqual(Speaker.VolumeStatuses.FadingIn, module2.VolumeStatus);
        }

        [UnityTest]
        public IEnumerator Enso_Scrub()
        {
            // Arrange
            SetUpMusicPlayer();
            float scrubPoint = 3f;

            // Act
            musicPlayer.Play("MusicTest");

            yield return null;

            musicPlayer.Pause();

            musicPlayer.Scrub(scrubPoint);

            musicPlayer.Unpause();

            yield return null;

            // Assert
            Assert.IsTrue(musicPlayer.CurrentTime >= scrubPoint,
                string.Format("Current time is less than scrub point: {0} < {1}",
                    musicPlayer.CurrentTime,
                    scrubPoint));
        }

        [UnityTest]
        public IEnumerator Enso_ScrubbingShouldNotPlayTrackAfterPause()
        {
            // Arrange
            SetUpMusicPlayer();
            float scrubPoint = 3f;

            // Act
            musicPlayer.Play("MusicTest");

            yield return null;

            musicPlayer.Pause();

            yield return null;

            musicPlayer.Scrub(scrubPoint);

            yield return null;

            // Assert
            Assert.IsFalse(module2.IsPlaying);
            Assert.IsFalse(module2.IsPlaying);
        }

        [UnityTest]
        public IEnumerator Enso_ScrubbingShouldNotPlayTrackAfterStop()
        {
            // Arrange
            SetUpMusicPlayer();
            float scrubPoint = 3f;

            // Act
            musicPlayer.Play("MusicTest");

            yield return null;

            musicPlayer.Stop();

            yield return null;

            musicPlayer.Scrub(scrubPoint);

            yield return null;

            // Assert
            Assert.IsFalse(module2.IsPlaying);
            Assert.IsFalse(module2.IsPlaying);
        }

        [UnityTest]
        public IEnumerator Enso_PlayFinitely()
        {
            // Arrange
            SetUpMusicPlayer();
            musicPlayer.TrackEndOrLoop += PlayFinitely_TrackEndOrLoop;

            // Act
            musicPlayer.Play("QuickTest", 3);

            yield return new WaitForSeconds(.1f);

            // Assert
            Assert.AreEqual(3, timesPlayed);
            Assert.IsFalse(module.IsPlaying);
            Assert.IsFalse(module2.IsPlaying);
        }

        [UnityTest]
        public IEnumerator Enso_ScrubShouldNotAffectFinitePlays()
        {
            // Arrange
            SetUpMusicPlayer();
            musicPlayer.TrackLoop += PlayFinitely_TrackEndOrLoop;

            // Act
            musicPlayer.Play("QuickTest", 3);

            yield return new WaitForSeconds(.051f);

            musicPlayer.ScrubAsPercentage(.5f);

            yield return new WaitForSeconds(.2f);

            // Assert
            Assert.AreEqual(2, timesPlayed);
        }

        [UnityTest]
        public IEnumerator Enso_ScrubAfterFinitePlayEndsShouldNotPlayAgain()
        {
            // Arrange
            SetUpMusicPlayer();
            musicPlayer.TrackEnd += Enso_ScrubAfterFinitePlayEndsShouldNotPlayAgainCallback;

            // Act
            musicPlayer.Play("QuickTest", 3);

            // Assert is in callback
            yield return new WaitForSeconds(.7f);
        }

        private void Enso_ScrubAfterFinitePlayEndsShouldNotPlayAgainCallback(MusicPlayerEventArgs e)
        {
            // Act
            musicPlayer.ScrubAsPercentage(.5f);

            // Assert
            Assert.IsFalse(module.IsPlaying);
            Assert.IsFalse(module2.IsPlaying);
        }

        [UnityTest]
        public IEnumerator Enso_PauseAndUnpauseWithTrackNotSetShouldNotThrowError()
        {
            // Arrange
            SetUpMusicPlayer();

            // Act
            musicPlayer.Pause();
            musicPlayer.Unpause();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Enso_PauseAndUnpauseWithTrackSetShouldNotThrowError()
        {
            // Arrange
            SetUpMusicPlayer();

            // Act
            musicPlayer.Play("MusicTest");
            musicPlayer.Stop();
            musicPlayer.Pause();
            musicPlayer.Unpause();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Enso_PauseUnpauseThenScrubShouldPlayAtPoint()
        {
            // Arrange
            SetUpMusicPlayer();

            // Act
            musicPlayer.Play("MusicTest");
            musicPlayer.Stop();
            musicPlayer.Pause();
            musicPlayer.Unpause();

            musicPlayer.Scrub(3f);

            yield return null;

            // Assert
            Assert.IsTrue(musicPlayer.CurrentTime >= 3f, "Current time is actually " + musicPlayer.CurrentTime);
        }

        [UnityTest]
        public IEnumerator Enso_FadeOutThenCrossfadeShouldNotChangeVolumeOfSpeaker()
        {
            // Arrange
            SetUpMusicPlayer();

            // Act
            musicPlayer.Play("MusicTest");
            musicPlayer.FadeOut();

            yield return new WaitForSeconds(2);

            musicPlayer.CrossFadeAtPoint("MusicTest", 0f);

            yield return null;

            // Assert
            Assert.IsTrue(speaker1.volume <= 0f, "Speaker volume is at " + speaker1.volume);
        }

        [UnityTest]
        public IEnumerator Enso_FadeInAfterFadeInShouldDoNothing()
        {
            // Arrange
            SetUpMusicPlayer();

            // Act
            musicPlayer.Play("MusicTest");
            musicPlayer.SetVolume(.75f);

            yield return null;

            musicPlayer.FadeIn();

            yield return new WaitForSeconds(2);

            musicPlayer.FadeIn();

            yield return null;

            // Assert
            Assert.IsTrue(speaker1.volume >= .75f, "Speaker volume is at " + speaker1.volume);
        }

        [UnityTest]
        public IEnumerator Enso_FadeOutAfterFadeOutShouldDoNothing()
        {
            // Arrange
            SetUpMusicPlayer();

            // Act
            musicPlayer.Play("MusicTest");

            yield return null;

            musicPlayer.FadeOut();

            yield return new WaitForSeconds(2);

            musicPlayer.FadeOut();

            yield return null;

            // Assert
            Assert.IsTrue(speaker1.volume <= 0f, "Speaker volume is at " + speaker1.volume);
        }

        #region Setup

        private void SetUpMusicPlayer()
        {
            GameObject player = new GameObject();
            player.AddComponent<AudioListener>();
            musicPlayer = player.AddComponent<MusicPlayer>();

            musicPlayer.Tracks = new List<MusicTrack>
            {
                new MusicTrack
                {
                    Name = "MusicTest",
                    Track = AudioClip.Create("MusicTest", 10000, 1, 1000, false),
                    loopPoints = new MusicTrack.LoopPoints
                    {
                        sampleLoopStart = 2000,
                        sampleLoopLength = 3000
                    }
                },
                new MusicTrack
                {
                    Name = "QuickTest",
                    Track = AudioClip.Create("QuickTest", 5000, 1, 100000, false), // .05 seconds long
                    loopPoints = new MusicTrack.LoopPoints
                    {
                        sampleLoopStart = 4000,
                        sampleLoopLength = 1000 // Loops are .01 seconds long
                    }
                }
            };

            foreach (MusicTrack track in musicPlayer.Tracks)
            {
                track.CreateAndCacheClips();
            }

            Speaker[] modules = player.GetComponents<Speaker>();

            module = modules[0];
            module2 = modules[1];

            speaker1 = module.IntroSource;
            speaker2 = module.LoopSource;
            speaker3 = module2.IntroSource;
            speaker4 = module2.LoopSource;
        }

        [TearDown]
        public void CleanUp()
        {
            DestroyIfItExists(module);
            DestroyIfItExists(musicPlayer);
            timesPlayed = 0;
        }

        private MusicTrack CreateMockMusicTrack()
        {
            return new MusicTrack
            {
                Name = "MusicTest",
                Track = AudioClip.Create("MusicTest", 5, 1, 1, false),
                loopPoints = new MusicTrack.LoopPoints
                {
                    sampleLoopStart = 1,
                    sampleLoopLength = 3
                }
            };
        }

        private void DestroyIfItExists(MonoBehaviour obj)
        {
            if (obj)
            {
                UnityEngine.Object.Destroy(obj.gameObject);
            }
        }

        int timesPlayed = 0;
        private void PlayFinitely_TrackEndOrLoop(MusicPlayerEventArgs e)
        {
            timesPlayed++;
        }

        #endregion
    }
}