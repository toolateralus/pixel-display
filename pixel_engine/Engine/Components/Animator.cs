using System;
using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.FileIO;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public class Animator : Component, IAnimate
    {
        private Animation? animation;
        private Sprite? sprite;
        
        public override void Awake()
        {
            test_flame_anim_setup();
        }
        public override void FixedUpdate(float delta)
        {
            if (animation is null || !animation.playing) 
                return;
            Next(animation.padding);
        }
        
        public void Next(int increment = 1)
        {
           sprite.Draw(sprite.size, animation.GetFrame());
        }
        public void Previous(int increment = 1)
        {
            sprite.Draw(new (32, 32), animation?.frames[(animation.frameIndex, animation.frameIndex -= increment)]);
        }
        
        public void Start(float speed = 1, bool looping = true)
        {
            parent.TryGetComponent(out sprite);
            if (animation is null)
            {
                Runtime.Log("Animation was null.");
                return;
            }
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
