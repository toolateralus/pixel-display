using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.FileIO;
using pixel_renderer.Scripts;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public class Animator : Component, IAnimate
    {
        
        private Animation? animation;
        private Sprite? sprite;

        public void SetAnimation(Animation animation) => this.animation = animation;
        public Animation? GetAnimation() => animation;

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
        }

        public void Reset()
        {
            parent.TryGetComponent(out sprite);
            if (animation is null)
            {
                Runtime.Log("Animation was null.");
                return;
            }
            Awake();
        }

        public override void Awake()
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

            // 60 frame-padding value displays the animation frames at about 1fps (per new frame) (while engine is running at ~@30fps)
            animation = new(anim_metas.ToArray(), 60)
            {
                Metadata = new("Animation", Constants.WorkingRoot + Constants.AssetsDir + Name + Constants.AssetsFileExtension, Constants.AssetsFileExtension)
            };

            animation.Upload();
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


    }
}
