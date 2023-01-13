using pixel_renderer;
using static pixel_renderer.Input;
using System;
using Key = System.Windows.Input.Key;

namespace pixel_renderer.Scripts
{

    public class Player : Component
    {
        [Field] public bool takingInput = true;
        [Field] public int speed = 2;
        [Field] Sprite sprite = new();
        [Field] Rigidbody rb = new();
        [Field] public Vec2 moveVector;
        public override void Awake()
        {
            parent.TryGetComponent(out sprite);
            parent.TryGetComponent(out rb);
            CreateInputEvents();

        }

        private void CreateInputEvents()
        {

            void up(object[] e) => moveVector = new Vec2(0, -1);
            void down(object[] e) => moveVector = new Vec2(0, 1);
            void left(object[] e) => moveVector = new Vec2(-1, 0);
            void right(object[] e) => moveVector = new Vec2(1, 0);
          
            RegisterAction(false, up, null, Key.W, InputEventType.DOWN);
            RegisterAction(false, down, null, Key.D, InputEventType.DOWN);
            RegisterAction(false, left, null, Key.A, InputEventType.DOWN);
            RegisterAction(false, right, null, Key.R, InputEventType.DOWN);
        }

        public override void FixedUpdate(float delta)
        {
            if (!takingInput) 
                return;
            Jump(moveVector);
            Move(moveVector);
            moveVector = Vec2.zero; 
        }
        public override void OnTrigger(Rigidbody other) { }
        public override void OnCollision(Rigidbody collider) => sprite.Randomize();
        private void Move(Vec2 moveVector)
        {
            rb.velocity.x += moveVector.x * speed;
        }
        private void Jump(Vec2 moveVector)
        {
            var jumpVel = speed * 2;
            rb.velocity.y = moveVector.y * jumpVel;
        }
    }

}