using static Pixel.Input;
using Key = System.Windows.Input.Key;
using Pixel.Assets;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using Pixel.Types.Components;
using Pixel.Statics;
using Pixel.Types.Physics;
using System.Collections.Generic;
using System.Threading.Tasks;
using PixelLang.Tools;

namespace Pixel
{
    /// <summary>
    ///  base class for all actors/ entities
    /// </summary>
    public class Entity : Component
    {
        const int startHp = 100;
        int health = startHp;
        bool dead = false;


        internal List<Entity> hit_list = new();

        public override void OnTrigger(Collision collision)
        {
            if (collision.collider.TryGetComponent<Entity>(out var ent) && !hit_list.Contains(ent) && ent != this)
                hit_list.Add(ent);
        }

        public void Damage(int value)
        {
            if (dead)
                return;
            health -= value;
            if (health <= 0) 
                Die();

        }

        private void Die()
        {
            health = 0;
            dead = true; 
        }

        public override void Dispose()
        {
             
        }
    }

    public class Player : Entity
    {
        [Field][JsonProperty] public float speed = 0.1f;
        [Field][JsonProperty] private float jumpSpeed = 0.25f;
        [Field][JsonProperty] public bool takingInput = true;
        [Field][JsonProperty] private bool followPlayer;

        private bool isGrounded;
        private Vector2 moveVector = default;
        private int haltIterations = 16;
        private int song_handle = 0; 
        private bool hit_cooldown;
        Inventory inventory = new();

        Sprite sprite;
        Rigidbody rb;
        Animator anim;
        Camera cam;
        
        public override void Dispose()
        {
            Audio.FreePlayer(song_handle);
            sprite = null;
            rb = null;
            anim = null;
            cam = null; 

        }

        private void Fire()
        {
            if (!hit_cooldown)
                if (hit_list.Count > 0)
                {
                    (int dmg, int speed) weapon = inventory.GetWeaponDamageAndSpeed();
                    Entity entity = hit_list.First();
                    Runtime.Log($"dealing {weapon.dmg} damage to {entity.node}");
                    entity.Damage(weapon.dmg);
                    hit_cooldown = true;
                    
                    Task cooldown = new Task(async delegate {
                        var delay = weapon.speed / 1000;
                        await Task.Delay(delay);
                        hit_cooldown = false; 
                    });

                    cooldown.Start();
                }
        }
        public override void Awake()
        {
            node.TryGetComponent(out rb);

            Item weapon = new("default weapon", ItemType.Weapon, 0, new Dictionary<string, int>
            {
                {Item.MinWeaponDamageProperty, 8},
                {Item.MaxWeaponDamageProperty, 22},
                {Item.HitSpeedValueProperty, 1000},
                {Item.StrengthValueProperty, 1},
                {Item.AgilityValueProperty, 2},
                {Item.AnimationIDProperty, 0},
                {Item.ModelIDProperty, 0},
                {Item.AudioIDProperty, 0}
            });

            inventory.Add(weapon);

            var meta = Library.FetchMeta("KingCrimsonRequiem");

            if (meta != null)
                song_handle = Audio.PlayFromPath(meta.Path, 0.45f);

            if (node.TryGetComponent(out sprite))
                sprite.Type = ImageType.Image;

            if (node.TryGetComponent(out anim))
                anim.Next();

            RegisterActions();
        }
        public override void FixedUpdate(float delta)
        {
            cam ??= Camera.First;

            if (!takingInput)
                return;

            if (CMouse.LeftPressedThisFrame)
            {
                Fire();
                InputProcessor.TryCallLine("=> clear;");
                Runtime.Log("Fired weapon.");
            }

            Move(moveVector);

            if (isGrounded)
                isGrounded = false;
            
            bool? playing = anim?.GetAnimation()?.playing;

            moveAnimation(playing);

            if (moveVector != Vector2.Zero)
                moveVector = Vector2.Zero;
            else rb?.ApplyImpulse((-rb.velocity / haltIterations));

            if (cam != null && followPlayer)
                cam.Position = Position;

            void moveAnimation(bool? playing)
            {
                if (rb is not null && playing.HasValue)
                    if (rb.velocity.Length() < 0.05f)
                    {
                        anim?.Stop();
                        sprite?.texture.SetImageRelative("\\Assets\\Animations\\Dog walking\\dog_standing.bmp");
                    }
                    else if (playing.HasValue && !playing.Value)
                        anim?.Start();
            }
        }
        public override void OnCollision(Collision collider)
        {
            isGrounded = true;
        }

        private void RegisterActions()
        {
            RegisterAction(this, Jump, Key.Up);
            RegisterAction(this, Down, Key.Down);
            RegisterAction(this, Left, Key.Left);
            RegisterAction(this, Right, Key.Right);
        }
        private void Move(Vector2 moveVector)
        {
            if (sprite is not null)
            {
                if (moveVector.X < 0)
                    sprite.Scale = new(-1, 1);
                if (moveVector.X > 0)
                    sprite.Scale = new(1, 1);
            }
            if (isGrounded)
                rb?.ApplyImpulse(moveVector.WithValue(y: speed * moveVector.Y) * speed);
            else rb?.ApplyImpulse(moveVector * speed);
        }
        
        #region Input 
        private void Jump()
        {
            if (isGrounded)
            {
                var meta = Library.FetchMeta("dog_barking");
                
                if(meta != null)
                    Audio.PlayFromMeta(meta, 0.45f);

                rb?.ApplyImpulse(-Vector2.UnitY * jumpSpeed * (1f + rb.velocity.Length()));
            }

        }
        private void Down()
        {
            moveVector.Y = 1 * speed;
        }
        private void Left()
        {
            moveVector.X = -1 * speed;
        }
        private void Right()
        {
            moveVector.X = 1 * speed;
        }
        #endregion

        public static Node Standard()
        {
            Node playerNode = Animator.Standard();
            playerNode.AddComponent<Interpreter>();
            playerNode.tag = "Player";
            playerNode.Position = new Vector2(-15, -20);
            playerNode.AddComponent<Player>().takingInput = true;
            playerNode.Scale = Constants.DefaultNodeScale;
            return playerNode;
        }
    }
}
