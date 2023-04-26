using pixel_core.FileIO;
using System;
using System.Windows.Media;

namespace pixel_core
{
    public static class Audio
    {
        static MediaPlayer player;
        public static void OneShot(string fileName = "", float volume = 0.5f)
        {
            var player = new MediaPlayer();

            if (fileName != "")
                player.Open(new Uri(fileName, UriKind.RelativeOrAbsolute));

            player.Volume = volume;
            player.Play();
        }
        /// <summary>
        /// A highly inefficient way of playing audio. note: path may be relative or absolute, but relative meaning app root, not <see cref="T:Constants.WorkingRoot"/>
        /// </summary>
        /// <param name="fileName"></param>
        public static void Play(string fileName = "", float volume = 0.5f)
        {
            player ??= new MediaPlayer();

            player.Stop();

            if(fileName != "")
                player.Open(new Uri(fileName, UriKind.RelativeOrAbsolute));

            player.Volume = volume;
            player.Play() ; 
        }
        /// <summary>
        /// A highly inefficient way of playing audio. note: path may be relative or absolute, but relative meaning app root, not <see cref="T:Constants.WorkingRoot"/>
        /// </summary>
        /// <param name="fileName"></param>
        public static void Play(Metadata metadata, float volume = 0.5f)
        {
            player ??= new MediaPlayer();

            player.Stop();

            if (metadata.Path != "")
                player.Open(new Uri(metadata.Path, UriKind.Absolute));

            player.Volume = volume;
            player.Play();
        }
    }
}
