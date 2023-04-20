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

        [JsonProperty]
        [Field] public float speed = 2f;

        [JsonProperty]
        private Animation? animation;

        [JsonProperty]
        private Sprite? sprite;
       
        [Field][JsonProperty]
        public string[] frameNames = new string[]
        {
            "dog_walking_1",
            "dog_walking_2",
            "dog_walking_3",
            "dog_walking_4",
            "dog_walking_5",
            "dog_walking_6",
            "dog_walking_7",
            "dog_walking_8",
        };

        [JsonProperty][Field]
        public int padding = 12;

        [JsonProperty]
        [Field]
        public bool looping = true; 

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
                if (i < max)
                    newArray[i++] = str;

            frameNames = newArray;
        }
        
        [Method]
        internal void RefreshAnimationWithFrameNames()
        {
            List<Metadata> metas = new();

            for (int i = 0; i < frameNames.Length; i++)
            {
                string? name = frameNames[i];
                if (AssetLibrary.FetchMeta(name) is Metadata meta)
                    metas.Add(meta);
            }
            
            if (metas.Count == 0) 
                return;
               
            Animation anim = new(metas.ToArray(), padding)
            {
                looping = true
            };

            animation = anim;
        }
        /// <summary>
        /// this wrapper allows params to be passed in when pressed from inspector.
        /// </summary>
        [Method]
        void Start() => Start(1, true);

        public override void Awake()
        {

        }
        public override void FixedUpdate(float delta)
        {
            if (animation is null || !animation.playing)
                return;
            Next();
        }

        public void Next()
        {
            if (animation is null)
                return;

            if (animation.speed != speed)
                animation.speed = speed; 

            var img = animation?.GetFrame(true);
            sprite?.SetImage(img);
        }
        public void Previous()
        {
            if (animation is null)
                return;
            animation.frameIndex -= 2;

            var img = animation?.GetFrame(true);

            sprite?.SetImage(img);
        }

        public void Start(float speed = 1, bool looping = true)
        {
            node.TryGetComponent(out sprite);
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
            node.TryGetComponent(out sprite);
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
            anim.looping = true; 
            
            if (node.TryGetComponent(out Sprite sprite))
                sprite.IsReadOnly = false; 

            anim.RefreshAnimationWithFrameNames();
            anim.Start();
            node.Scale = Constants.DefaultNodeScale;
            return node;
        }

        internal JImage? TryGetFrame(int index)
        {
            if (animation?.frames is null)
                return null;
            if (animation?.frames.ElementAt(index) is null)
                return null;
            return animation?.frames.ElementAt(index).Value;
        }
    }
}
