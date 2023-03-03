﻿
using pixel_renderer.Assets;
using Newtonsoft.Json;
using System;
using System.Windows.Media.TextFormatting;
using System.Windows.Controls;
using System.Numerics;

namespace pixel_renderer
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Component
    {
        [JsonProperty]
        public Node node;
        [JsonProperty]
        public bool IsActive = true;
        [JsonProperty]
        internal Vector2 Position
        {
            get
            {
                return node?.Position ?? default;
            }
            set
            {
                if (node is null)
                    return; 
                node.Position = value;
            }
        }
        public float rotation => node.rotation; 
        public string Name { get; set; } = "";
        [JsonProperty]
        private string _uuid = "";
        public string UUID => _uuid;

        public Component()
        {
            _uuid = pixel_renderer.UUID.NewUUID();
        }

        public virtual void Awake() { }
        public virtual void Update() { }
        public virtual void FixedUpdate(float delta) { }
        public virtual void OnTrigger(Collider other) {  }
        public virtual void OnCollision(Collider collider) {  }

        /// <summary>
        /// Performs a 'Get Component' call on the Parent node object of the component this is called from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns>A component of specified type and parent</returns>
        public T GetComponent<T>(int index = 0) where T : Component => node.GetComponent<T>(index);
        /// <summary>
        /// initializes UUID and other readonly info, should NEVER be called by the user.
        /// </summary>
        internal protected void init_component_internal()
        {
            Name = $"{GetType().Name}";
            Awake();
        }
        internal T GetShallowClone<T>() where T : Component => (T)MemberwiseClone();

        public virtual void OnEditActionClicked(Action action) 
        {
            action?.Invoke();
            Runtime.Log($"{Name} had {nameof(OnEditActionClicked)} called at {DateTime.Now}");
        }
        public virtual void OnDrawShapes()
        {
        }
    }

}
