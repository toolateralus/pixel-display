using Pixel.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Pixel
{

    public class AudioInstance : MediaPlayer
    {
        public bool locked; 
        public AudioInstance()
        {
            Volume = 0.5f;
            SpeedRatio = 1;
            MediaOpened += Player_MediaOpened;
            MediaFailed += Player_MediaFailed;
            MediaEnded += Player_MediaEnded;
        }
        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            if (sender is not AudioInstance ai)
                return;
            ai.locked = false; 
        }

        private void Player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            if (sender is not AudioInstance ai)
                return;
            Runtime.Log($"Failed to play media on {ai}");
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            if (sender is not AudioInstance ai)
                return;
            ai.locked = true;
        }
        public void ForceRelease()
        {
            locked = false;
            Stop();
        }
    }

    public static class Audio
    {
        static Dictionary<int, AudioInstance> Players;
        static Audio()
        {
            Players = new();
            Application.Current.Dispatcher.Invoke(() => { 
                lock (Players)
                    for (int i = 0; i < 8; ++i)
                    {
                        AudioInstance audioInstance = new AudioInstance();
                        Players.Add(audioInstance.GetHashCode(), audioInstance);
                    }
            });
        }

        /// <summary>
        /// this allocates an entirely new instance of mediaplayer, it seems to have a gigantic delay. PlayFromPath/PlayFromMeta methods use a cheaper way of cloning the MediaPlayer.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="volume"></param>
        /// <param name="speed"></param>
        public static void OneShot(string fileName = "", float volume = 0.5f, double speed = 1)
        {
            var player = new MediaPlayer();

            if (fileName != "")
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    player.Open(new Uri(fileName, UriKind.RelativeOrAbsolute));
                    player.SpeedRatio = speed;
                    player.Volume = volume;
                    player.Play();
                });
        }
        private static (int handle, AudioInstance player) GetFreePlayer()
        {
            var playerCopy = Players.Where(player => !player.Value.locked);
            AudioInstance player = playerCopy.FirstOrDefault().Value;
            int handle = playerCopy.FirstOrDefault().Key;
            return (handle, player);
        }
        private static void Play(string fileName, float volume, double speed, MediaPlayer playerCopy)
        {
            if (fileName != "")
                Application.Current?.Dispatcher.Invoke(() => 
                {
                    playerCopy.Open(new Uri(fileName, UriKind.RelativeOrAbsolute)); 
                    playerCopy.SpeedRatio = speed;
                    playerCopy.Volume = volume;
                    playerCopy.Play();
                });

        }
        public static int PlayFromPath(string fileName = "", float volume = 0.5f, double speed = 1)
        {
            var audio = GetFreePlayer();
            Play(fileName, volume, speed, audio.player);
            return audio.handle;
        }
        public static int PlayFromMeta(Metadata metadata, float volume = 0.5f, double speed = 1)
        {
            var audio = GetFreePlayer();
            Play(metadata.Path, volume, speed, audio.player);
            return audio.handle;
        }

        internal static void FreePlayer(int song_handle)
        {
            if (Players.ContainsKey(song_handle))
                Application.Current?.Dispatcher.Invoke(() => Players[song_handle].ForceRelease());
        }
    }
}
