using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using Pixel = System.Drawing.Color;

namespace pixel_renderer
{
    public class Animator : Component, IAnimate
    {
        private Animation? animation;
        private Sprite? sprite;
        [Field]
        [JsonProperty]
        public string[] frameNames = new string[] 
        { 
            "List Empty", 
            "Use Buttons To", 
            "Add Elements That", 
            "Point to names of", 
            "Files in the", 
            "Assets Directory", 
        };

        [JsonProperty]
        [Field]
        public int padding = 24;
        
        [JsonProperty]
        [Field]
        public bool looping;

        [Method]
        void InsertFrame()
        {
            var newArray = new string[frameNames.Length + 1];
            int i = 0; 

            foreach (var str in frameNames)
                newArray[i++] = str;

            frameNames = newArray;
        }
        [Method]
        void RemoveFrame()
        {
            int max = frameNames.Length - 1;
            int i = 0;
            
            if (max <= 1) 
                return;

            var newArray = new string[max];

            foreach (var str in frameNames)
                if(i < max)
                newArray[i++] = str;

            frameNames = newArray;
        }
        [Method]
        void RefreshAnimationWithFrameNames()
        {
            List<Metadata> metas = new();
            foreach (var name in frameNames)
            {
                if(AssetLibrary.FetchMeta(name) is Metadata meta)
                metas.Add(meta);
            }
            if (metas.Count == 0)
                return;
            Animation anim = new(metas.ToArray(), padding);
            anim.looping = true;
            animation = anim; 
        }
        [Method]
        void Start() => Start(1, true);

        public override void Awake()
        {

        }
        public override void FixedUpdate(float delta)
        {
            if (animation is null || !animation.playing) 
                return;
            Next(animation.padding);
        }
        public void Next(int increment = 1)
        {
            if (animation is null)
                return;

            var img  = animation?.GetFrame(true);
            sprite?.Draw(img);
        }
        public void Previous(int increment = 1)
        {
            if (animation is null)
                return; 
            animation.frameIndex -= 2;

            var img = animation?.GetFrame(true);
         
            sprite?.Draw(img);
        }
        public void Start(float speed = 1, bool looping = true)
        {
            parent.TryGetComponent(out sprite);
            if (animation is null)
            {
                Runtime.Log("Animation was null.");
                return;
            }
            animation.frameIndex = animation.startIndex;
            animation.playing = true;
        }
        public void Stop(bool reset = false)
        {
            parent.TryGetComponent(out sprite);
            if (animation is null)
            {
                Runtime.Log("Animation was null.");
                return;
            }
            animation.playing = false;

            if (reset)
                animation.frameIndex = animation.startIndex;
        }
        
        public void SetAnimation(Animation animation) => this.animation = animation;
        public Animation? GetAnimation() => animation;
        public static Node Standard()
        {
            var node = Rigidbody.Standard();
            var anim = node.AddComponent<Animator>();
            anim.test_flame_anim_setup();
            anim.Start();
            return node;
        }
        
        private void test_flame_anim_setup()
        {
            List<Metadata> anim_metas = new()
            {
                Player.test_animation_data(1),
                Player.test_animation_data(2),
                Player.test_animation_data(3),
                Player.test_animation_data(4),
                Player.test_animation_data(5),
                Player.test_animation_data(6),
            };

            animation = new(anim_metas.ToArray(), 10);
            animation.Name = "Animation_Test";
            animation.Metadata = new("Animation_Test", Constants.WorkingRoot + Constants.AssetsDir + "Animation_Test" + Constants.AssetsFileExtension, Constants.AssetsFileExtension);
            animation.Upload();
        }
    }
}
