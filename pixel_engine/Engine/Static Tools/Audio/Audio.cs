using System;
using System.Windows.Media;

namespace pixel_renderer
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
        /// A highly inefficient way of playing audio.
        /// </summary>
        /// <param name="fileName"></param>
        public static void Play(string fileName = "", float volume = 0.5f)
        {
            player ??= new MediaPlayer();

            if(fileName != "")
                player.Open(new Uri(fileName, UriKind.RelativeOrAbsolute));

            player.Volume = volume;
            player.Play() ; 
        }
    }
}
