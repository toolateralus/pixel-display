namespace pixel_renderer
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Collision
    {
        public static SpatialHash hash = new(Constants.screenWidth, Constants.screenHeight, Constants.collisionCellSize);
        public static bool CheckOverlap(this Node nodeA, Node nodeB)
        {
            Vec2 a = nodeA.position;
            Vec2 b = nodeB.position; 
            Vec2 spriteSizeA = nodeA.GetComponent<Sprite>().size;
            Vec2 spriteSizeB = nodeB.GetComponent<Sprite>().size;

            if (spriteSizeA != null && spriteSizeB != null)
            {
                // messy if for box collision; 
                if (a.x < b.x + spriteSizeB.y && 
                    a.x + spriteSizeA.x > b.x && 
                    a.y < b.y + spriteSizeB.y && 
                    spriteSizeA.y + a.y > b.y)
                    return true; 
            }
            return false; 
           
        }
        public static void BroadPhase(Stage stage, List<List<Node>> broadMap)
        {
            broadMap.Clear(); 
            foreach (var node in stage)
            {
                if (!node.TryGetComponent(out Sprite sprite) || !sprite.isCollider)
                {
                    continue; 
                }
                hash.RegisterObject(node);
            }
            foreach (var node in stage)
            {
               List<Node> result = hash.GetNearby(node);
               broadMap.Add(result);
            }
            hash.ClearBuckets();
        }
        public static Dictionary<Node, Node> NarrowPhase(List<List<Node>> collisionMap, Dictionary<Node, Node> narrowMap)
        {
            narrowMap.Clear();

            if (collisionMap.Count <= 0 || collisionMap[0] == null) 
                return narrowMap; 
            
            for(int i = 0; i < collisionMap.Count(); i++)
            {
                var cell = collisionMap[i];
                if(cell.Count <= 0) continue;

                for (int j = 0; j < cell.Count; j++)
                {
                    var nodeA = cell[j];
                    if (nodeA is null) continue; 

                    for (int k = 0; k < cell.Count; k++)
                    {
                        var nodeB = cell[k];
                        if (nodeB is null) continue;

                        // check UUID instead of absolute value of somthing else
                        if (nodeA.UUID.Equals(nodeB.UUID)) continue; 

                        if (nodeA.CheckOverlap(nodeB))
                        {
                            /* with  a 2D loop, each node is compared twice from each perspective
                             and once against itself as well, because we use the entire node list
                            in the stage for both loops*/

                            // continue or remove and proceed?
                            // continue might be cheaper but might also continue to have to 
                            // try and do the alreasdy done or false comparison 

                            if (narrowMap.ContainsKey(nodeA))   continue;
                            if (narrowMap.ContainsKey(nodeB))   continue;
                            if (narrowMap.ContainsValue(nodeA)) continue;
                            if (narrowMap.ContainsValue(nodeB)) continue;

                            narrowMap.Add(nodeA, nodeB);
                        }
                    }
                }
            }
            return narrowMap; 
        }
        public static void ViewportCollision(Node node)
        {
            Sprite sprite = node.GetComponent<Sprite>();
            Rigidbody rb = node.GetComponent<Rigidbody>();
            if (sprite is null || rb is null) return;
            if (sprite.isCollider)
            {
                if (node.position.y > Constants.screenHeight - 4 - sprite.size.y)
                {
                    node.position.y = Constants.screenHeight - 4 - sprite.size.y;
                }
                if (node.position.x > Constants.screenWidth - sprite.size.x)
                {
                    node.position.x = Constants.screenWidth - sprite.size.x;
                    rb.velocity.x = 0;
                }
                if (node.position.x < 0)
                {
                    node.position.x = 0;
                    rb.velocity.x = 0;
                }
            }
        }
        internal static void GetCollision(Dictionary<Node, Node> narrowMap)
        {
            foreach (var collisionPair in narrowMap)
            {
                GetCollisionComponents(collisionPair, out Rigidbody rbA, out Rigidbody rbB);
                GetDominantBody(rbA, rbB, out Rigidbody submissive, out Rigidbody dominant);
                Collide(submissive, dominant);
            }
        }
        private static void Collide(Rigidbody submissive, Rigidbody dominant)
        {
            submissive.parentNode.position += dominant.velocity;
        }
        private static void GetDominantBody(Rigidbody rbA, Rigidbody rbB, out Rigidbody submissive, out Rigidbody dominant)
        {
            
            if (rbA.velocity.Length >= rbB.velocity.Length)
            {
                dominant = rbA;
                submissive = rbB;
            }
            else
            {
                dominant = rbB;
                submissive = rbA;
            }
            if (rbA.usingGravity && !rbB.usingGravity)
            {
                dominant = rbA;
                submissive = rbB;
            }
            else
            {
                dominant = rbB;
                submissive = rbA;
            }
            if (submissive == null || dominant == null)
            {
                submissive = rbB;
                dominant = rbA;
            }
        }
        /// <summary>
        /// Retrieves all relevant Node components to solve an already verified collision between two Nodes. 
        /// </summary>
        /// <param name="colliders"></param>
        /// <param name="rbA"></param>
        /// <param name="rbB"></param>
        /// <param name="submissive"></param>
        /// <param name="dominant"></param>
        private static void GetCollisionComponents(KeyValuePair<Node, Node> colliders, out Rigidbody rbA, out Rigidbody rbB)
        {
            Node a = colliders.Key;
            Node b = colliders.Value;
            if (a.Name == "Floor" || b.Name == "Floor")
            {
                
            }
            Sprite spriteA = a.GetComponent<Sprite>();
            Sprite spriteB = b.GetComponent<Sprite>();

            rbA = a.GetComponent<Rigidbody>();
            rbB = b.GetComponent<Rigidbody>();
            
            Vec2 sizeA = spriteA.size;
            Vec2 sizeB = spriteB.size;

            Vec2 posA = a.position;
            Vec2 posB = b.position;
        }
           
    }

}