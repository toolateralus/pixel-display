using Pixel.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    public static class Audio
    {
        static List<AudioInstance> Players;
        static Audio()
        {
            Players = new();
            for (int i = 0; i < 25; ++i)
                Players.Add(new AudioInstance());      
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
                player.Open(new Uri(fileName, UriKind.RelativeOrAbsolute));

            player.SpeedRatio = speed;
            player.Volume = volume;
            player.Play();
        }
        public static MediaPlayer GetFreePlayer()
        {
            var playerCopy = Players.Where(player => !player.locked).FirstOrDefault();
            if (playerCopy is null)
            {
                AudioInstance ai = new AudioInstance();
                Players.Add(ai);
                return ai;
            }
            return playerCopy;
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
        public static void PlayFromPath(string fileName = "", float volume = 0.5f, double speed = 1)
        {
            var playerCopy = GetFreePlayer();

            Play(fileName, volume, speed, playerCopy);
        }
        public static void PlayFromMeta(Metadata metadata, float volume = 0.5f, double speed = 1)
        {
            var playerCopy = GetFreePlayer();
            if (metadata is not null)
                Play(metadata.FullPath, volume, speed, playerCopy);
        }
    }
}
