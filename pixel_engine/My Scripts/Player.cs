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
            Action<object[]> up = (e) =>    moveVector += new Vec2(0, 1);
            Action<object[]> down = (e) =>  moveVector += new Vec2(0, -1);
            Action<object[]> left = (e) =>  moveVector += new Vec2(1, 0);
            Action<object[]> right = (e) => moveVector += new Vec2(1, 1);

            InputAction player_move_up = new(false, up, null, Key.W);
            InputAction player_move_down = new(false, down, null, Key.W);
            InputAction player_move_left = new(false, left, null, Key.W);
            InputAction player_move_right = new(false, right, null, Key.W);

            RegisterAction(player_move_up, InputEventType.DOWN);
            RegisterAction(player_move_down, InputEventType.DOWN);
            RegisterAction(player_move_left, InputEventType.DOWN);
            RegisterAction(player_move_right, InputEventType.DOWN);
        }

        public override void FixedUpdate(float delta)
        {

            if (!takingInput) 
                return;
            Runtime.Log(moveVector.AsString());

            //Runtime.Log(log);
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