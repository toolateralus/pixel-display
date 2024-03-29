<?xml version="1.0"?>
<doc>
    <assembly>
        <name>Pixel Core</name>
    </assembly>
    <members>
        <member name="M:Pixel.ExtensionMethods.ToInt(System.String)">
            <summary>
            Takes a string of any format and returns a integer value. If the string contains no number chars, it will return -1.
            </summary>
            <param name="input"></param>
            <returns>an integer of value based on the order and frequency of numbers in the input string.</returns>
        </member>
        <member name="M:Pixel.ExtensionMethods.ToFileNameFormat(System.String)">
            <summary>
            Since the assets system handles the file 
            
            , this format is only relevant to the naming convention used for files.
            </summary>
            <param name="input"></param>
            <returns>A formatted version of the string that will not cause file-saving errors</returns>
        </member>
        <member name="M:Pixel.ExtensionMethods.Flatten``1(``0[0:,0:])">
            <summary>
            
            </summary>
            <param name="colors"></param>
            <returns> A one dimensional array containing all the elements of the two-dimensional array passed in.</returns>
        </member>
        <member name="M:Pixel.Vector2ExtensionMethods.Normalized(System.Numerics.Vector2)">
            <summary>
             TODO: fix possible  'divide by zero'
              Normalize a vector
            </summary>
            <param name="v"></param>
            <returns>A normalized Vector from the length of the current</returns>
        </member>
        <member name="P:Pixel.FileIO.FileBase.Path">
            <summary>
            the absolute path corrected to the machine that's running the program, and WorkingRoot in Constants.
            </summary>
        </member>
        <member name="M:Pixel.Project.Load">
            <summary>
            Runs an import file dialog and when appropriately navigated to a project file, loads it.-
            </summary>
            <returns></returns>
        </member>
        <member name="M:Pixel.Project.#ctor(System.String)">
            <summary>
            use this for new projects and overwrite the default stage data, this prevents lockups
            </summary>
            <param name="name"></param>
        </member>
        <member name="M:Pixel.Assets.Importer.Import(System.Boolean)">
            <summary>
            Enumerates through all files in the Asset Import path and at
            ts to register them to the runtime AssetLibrary instance. 
            </summary>
        </member>
        <member name="T:Pixel.WaveForms">
            <summary>
            must be refactored to not be static or to not use instanced data.
            </summary>
        </member>
        <member name="P:Pixel.WaveForms.Next">
            <summary>
            Samples a random vertex point on a Sine Wave operating within pre-defined parameters.
            </summary>
        </member>
        <member name="M:Pixel.WaveForms.GetPointOnSine(System.Single,System.Single,System.Single,System.Int32)">
            <summary>
            Manually define parameters for a sample from a sine wave.
            </summary>
            <param name="startPosition">the start of the wave</param>
            <param name="endPosition">the end position of the wave</param>
            <param name="Tau">A float within the range of 0 to PI * 2</param>
            <param name="vertexIndex">the individual vertex of the wave which will be returned</param>
            <param name="x">out X of the returned vector</param>
            <param name="y">out Y of the returned vector</param>
            <returns>A Vertex position on the specified wave.</returns>
        </member>
        <member name="M:Pixel.WaveForms.GetPointOnSine">
            <summary>
            Sample a sine wave under the current defined parameters of the static class Sine.
            </summary>
            <returns>A Vertex position at a random point on a sine wave</returns>
        </member>
        <member name="M:Pixel.IAnimate.Start(System.Single,System.Boolean)">
            <summary>
            Starts the animation.
            </summary>
            <param name="speed"></param>
            <param name="looping"></param>
        </member>
        <member name="M:Pixel.IAnimate.Stop(System.Boolean)">
            <summary>
            Stops the animation.
            </summary>
            <param name="reset"></param>
        </member>
        <member name="M:Pixel.IAnimate.Next">
            <summary>
            Gets the next frame in the animation, or skips frames if  an increment of greater than one is provided
            /// </summary>
            <param name="increment"></param>
        </member>
        <member name="M:Pixel.IAnimate.Previous">
            <summary>
            Gets the previous frame in the animation, or skips back multiple frames if an increment of greater than one is provided
            </summary>
            <param name="increment"></param>
        </member>
        <member name="F:Pixel.Animation`1.frameTime">
            <summary>
            this is the number of frames to wait between displaying frames.
            </summary>
        </member>
        <member name="M:Pixel.Animation`1.#ctor(`0[],System.Int32,System.Boolean,System.Boolean)">
            <summary>
            frameTime is exclusive (I think, if you input 16 frames will be 15 cycles long.)
            </summary>
            <param name="data"></param>
            <param name="frameTime"></param>
            <param name="looping"></param>
        </member>
        <member name="T:Pixel.Types.Components.Component">
            <summary>
            The base class for all Components, which are modules added to nodes to exend behavior.
            </summary>
        </member>
        <member name="F:Pixel.Types.Components.Component.node">
            <summary>
            the owner of this component.
            </summary>
        </member>
        <member name="F:Pixel.Types.Components.Component.Enabled">
            <summary>
            will this component be updated or this next frame?
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.Awake">
            <summary>
            Will be called before <see cref="M:Pixel.Types.Components.Component.Update"/>
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.Update">
            <summary>
            Will be called after <see cref="M:Pixel.Types.Components.Component.Awake"/> and each subsequent render frame while running.
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.FixedUpdate(System.Single)">
            <summary>
            Will be called after <see cref="M:Pixel.Types.Components.Component.Awake"/> and each subsequent physics frame while running.
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.OnTrigger(Pixel.Types.Physics.Collision)">
            <summary>
            Will be called in the event that this Component's parent <see cref="T:Pixel.Node"/>'s components participated in a collision where one or more of the colliders were flagged IsTrigger.
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.OnCollision(Pixel.Types.Physics.Collision)">
            <summary>
            Will be called in the event that this Component's parent <see cref="T:Pixel.Node"/>'s components participated in a collision.
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.OnFieldEdited(System.String)">
            <summary>
            Will be called in the event that a field is edited by reflection and the Editor's Component Editor
            </summary>
            <param name="field"> the field that was called to be edited.</param>
        </member>
        <member name="M:Pixel.Types.Components.Component.OnDrawShapes">
            <summary>
            Will be called each frame by the renderer at the time that the shape drawer is collecting a cycle's worth of debug data.
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.on_destroy_internal">
            <summary>
            is called before Destroy to perform Dispose.
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.OnDestroy">
            <summary>
            Destroy's this component.
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.Dispose">
            <summary>
            You must release all references to any components, nodes, or other engine objects as of now, otherwise unexpected behavior is iminent
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.LocalToGlobal(System.Numerics.Vector2)">
            <summary>
            Transforms a vector to this node's Transform.
            </summary>
            <param name="local"></param>
            <returns></returns>
        </member>
        <member name="M:Pixel.Types.Components.Component.GlobalToLocal(System.Numerics.Vector2)">
            <summary>
            Transforms a vector to this nodes Transform
            </summary>
            <param name="global"></param>
            <returns></returns>
        </member>
        <member name="M:Pixel.Types.Components.Component.GetComponent``1(System.Int32)">
            <summary>
            Performs a 'Get Component' call on the Parent node object of the component this is called from.
            </summary>
            <typeparam name="T"></typeparam>
            <param name="index"></param>
            <returns>A component of specified type and parent</returns>
        </member>
        <member name="M:Pixel.Types.Components.Component.init_component_internal">
            <summary>
            initializes UUID and other readonly info, should NEVER be called by the user.
            </summary>
        </member>
        <member name="M:Pixel.Types.Components.Component.RemoveComponent(Pixel.Types.Components.Component)">
            <summary>
            A wrapper for <see cref="M:Pixel.Node.RemoveComponent(Pixel.Types.Components.Component)"/>
            </summary>
            <param name="component"></param>
        </member>
        <member name="T:Pixel.Types.InputAction">
            <summary>
            A container for an action with some args, used to fire RegesterAction input event types.
            </summary>
        </member>
        <member name="T:Pixel.Types.Physics.BoundingBox2DInt">
            <summary>
            A bounding box.
            </summary>
        </member>
        <member name="T:Pixel.Types.Physics.Collision">
            <summary>
            Stores data representing a single collision between two colliders.
            </summary>
        </member>
        <member name="T:Pixel.Types.Physics.Curve">
            <summary>
            A simple Vector2 Animation class with helpers like primitve curves etc.
            </summary>
        </member>
        <member name="T:Pixel.Types.Physics.Polygon">
            <summary>
            A collection of vertices representing a single shape.
            note :our Polygon uses a clockwise winding order
            </summary>
        </member>
        <member name="M:Pixel.Types.Physics.Polygon.CalculateUV">
            <summary>
            Each line will point clockwise.
            Will have one line that starts and ends in the same spot if there's only one vertex in the polygon
            </summary>
            <returns></returns>
        </member>
        <member name="T:Pixel.Types.Physics.Ray">
            <summary>
            A line.
            </summary>
        </member>
        <member name="T:Pixel.Types.Physics.SATCollision">
            <summary>
            A helper class for performing SAT collisions, our supported algorithm.
            </summary>
        </member>
        <member name="M:Pixel.Types.Physics.SATCollision.GetCollisionData(Pixel.Types.Physics.Polygon,Pixel.Types.Physics.Polygon,System.Numerics.Vector2)">
            <summary>
            Finds the smallest vector that will move A out of B
            </summary>
            <param name="polygonA"></param>
            <param name="polygonB"></param>
            <returns>The direction to move A out of B, and the amount it must move.</returns>
        </member>
        <member name="T:Pixel.Types.Physics.SATProjection">
            <summary>
             A single SAT projection.
            </summary>
        </member>
        <member name="T:Pixel.Types.Physics.SATContanctPoint">
            <summary>
            A single SAT contact point, resulting from a projection.
            </summary>
        </member>
        <member name="T:Pixel.Types.Physics.Line">
            <summary>
            A basic shape mostly used for drawing debug shapes.
            </summary>
        </member>
        <member name="T:Pixel.Types.Physics.Circle">
            <summary>
            A basic shape mostly used for drawing debug shapes.
            </summary>
        </member>
        <member name="M:Pixel.Animator.Start">
            <summary>
            this wrapper allows params to be passed in when pressed from InspectorControl.
            </summary>
        </member>
        <member name="F:Pixel.Collider.transformedModel">
            <summary>
            Sets the model to a copy of input.
            Assumes input polygon has already calculated normals
            </summary>
            <param name="polygon"></param>
        </member>
        <member name="M:Pixel.Collider.GetModel">
            <returns>a copy of the colliders' model, not the model itself</returns>
        </member>
        <member name="M:Pixel.Lua.Dispose">
            <summary>
            this should only need to include references to components or nodes.
            </summary>
        </member>
        <member name="F:Pixel.Softbody.deformationRadius">
            <summary>
            the max deformation represented as radius around the vertex
            </summary>
        </member>
        <member name="F:Pixel.Sprite.IsDirty">
            <summary>
            color data is refreshed from source on update if this is true
            </summary>
        </member>
        <member name="F:Pixel.Sprite.texture">
            <summary>
            stores color data
            </summary>
        </member>
        <member name="F:Pixel.Sprite.Type">
            <summary>
            this determines what source the color data will come from
            </summary>
        </member>
        <member name="F:Pixel.Sprite.textureFiltering">
            <summary>
            this dictates how the renderer filters the sprite during drawing
            </summary>
        </member>
        <member name="F:Pixel.Sprite.lit">
            <summary>
            this will toggle participation in lighting
            </summary>
        </member>
        <member name="F:Pixel.Sprite.color">
            <summary>
            this is the color that a solid color sprite will be drawn as
            </summary>
        </member>
        <member name="F:Pixel.Sprite.drawOrder">
            <summary>
            this determines what layer the sprite will be drawn in, ie -1 for bckground and 1 for on top of that.
            </summary>
        </member>
        <member name="F:Pixel.Sprite.IsReadOnly">
            <summary>
            this prevents the image/color data from being overwritten or changed.
            </summary>
        </member>
        <member name="M:Pixel.Sprite.PixelShader(System.Action{Pixel.Color},System.Func{Pixel.Color},System.Func{System.Int32},System.Func{System.Int32})">
            <summary>
            see LightingPerPixel to see an example 
            </summary>
            <param name="colorOut"></param>
            <param name="colorIn"></param>
            <param name="indexerX"></param>
            <param name="indexerY"></param>
            <param name="onIteraton"></param>
        </member>
        <member name="M:Pixel.Texture.SetImage(Pixel.FileIO.Metadata)">
            <summary>
            probably the best way to set image.
            </summary>
            <param name="meta"></param>
        </member>
        <member name="F:Pixel.Text.posCurve">
            <summary>
            the bounding box of the text element
            </summary>
        </member>
        <member name="M:Pixel.UIComponent.Draw(Pixel.RendererBase)">
            <summary>
            re-draws the image *this is always called when marked dirty*
            </summary>
            <exception cref="T:System.NotImplementedException"></exception>
            <summary>
            for any unmanaged resources that need to be disposed of, this is usually unneccesary.
            </summary>
        </member>
        <member name="M:Pixel.UIComponent.DrawImage(Pixel.RendererBase,Pixel.JImage)">
            <summary>
            this is the default method for drawing.
            </summary>
            <param name="renderer"></param>
            <param name="image"></param>
            <exception cref="T:System.NotImplementedException"></exception>
            
        </member>
        <member name="T:Pixel.EditorEventFlags">
            <summary>
            Shortcut flags for EditorEvents to quickly make requests/calls.
            </summary>
        </member>
        <member name="T:Pixel.EditorEvent">
            <summary>
            This is a class used to pass info and requests to the Editor without the context neccesary from being lower in the hierarchy.
            </summary>
        </member>
        <member name="T:Pixel.Hierarchy">
            <summary>
            The container that the Stage uses to store nodes and make queries.
            </summary>
        </member>
        <member name="M:Pixel.Hierarchy.Find(System.String)">
            <summary>
            Finds a node based on its name
            </summary>
            <param name="name"></param>
            <returns></returns>
        </member>
        <member name="M:Pixel.Hierarchy.RootIndex(System.String)">
            <summary>
            Finds the index of a node if it exists as a root in the stage hierarchy based on its name
            </summary>
            <param name="name"></param>
            <returns></returns>
        </member>
        <member name="M:Pixel.Hierarchy.RootSearch(System.Int32)">
            <summary>
            Searches for a node by index in the root nodes.
            </summary>
            <param name="rootIndex"></param>
            <returns></returns>
        </member>
        <member name="M:Pixel.Hierarchy.ChildSearch(System.Int32,System.Int32)">
            <summary>
            searches for a child under the specified root by index
            </summary>
            <param name="rootIndex"></param>
            <param name="child_index"></param>
            <returns></returns>
        </member>
        <member name="T:Pixel.JImage">
            <summary>
            The lowest-level Image type in Pixel, which only contains a byte[] and the neccesary data to process it from various contexts.
            </summary>
        </member>
        <member name="T:Pixel.LUA">
            <summary>
            A mostly static class functioning as a wrapper for KeraLua Lua usage. Also contains all Pixel LuaFunctions aka PixelLua library :D
            </summary>
        </member>
        <member name="T:Pixel.Node">
            <summary>
            The node represents a GameObject, which always includes a Transform (position , scale, rotation).
            Components are modules added to a Node which are routinely updated auto with subscribed events and
            overriden virutal methods.
            </summary>
        </member>
        <member name="F:Pixel.Node.OnCollided">
            <summary>
            Is triggered when this node or any of is components participate in a collision.
            </summary>
        </member>
        <member name="F:Pixel.Node.OnTriggered">
            <summary>
            Is triggered when this node or any of it's components participate in a collision where
            one or more of the colliders had "IsTriggered" set to true.
            </summary>
        </member>
        <member name="F:Pixel.Node.OnDestroyed">
            <summary>
            Is triggered when this node is called to be destroyed, after <see cref="M:Pixel.Node.Dispose"/> is called.
            </summary>
        </member>
        <member name="P:Pixel.Node.Enabled">
            <summary>
            if this is false the node won't be moved or updated, nor visible.
            </summary>
        </member>
        <member name="P:Pixel.Node.UUID">
            <summary>
            a Universally Unique Identifier.
            </summary>
        </member>
        <member name="P:Pixel.Node.Name">
            <summary>
            The name of the node.
            </summary>
        </member>
        <member name="F:Pixel.Node.tag">
            <summary>
            A way to group nodes and query them without looking for components.
            </summary>
        </member>
        <member name="F:Pixel.Node.parent">
            <summary>
            The node directly above this one in the hierarchy.
            </summary>
        </member>
        <member name="F:Pixel.Node.children">
            <summary>
            Every child node of this node.
            </summary>
        </member>
        <member name="F:Pixel.Node.child_offsets">
            <summary>
            A list of the offset between <see cref="!:this"/> <see cref="T:Pixel.Node"/> and it's children to maintain during physics updates.
            </summary>
        </member>
        <member name="F:Pixel.Node.rb">
            <summary>
            A cached <see cref="T:Pixel.Rigidbody"></see> to save <see cref="M:Pixel.Node.GetComponent``1(System.Int32)"></see> calls/allocations.
            </summary>
        </member>
        <member name="F:Pixel.Node.col">
            <summary>
            A cached <see cref="T:Pixel.Collider"></see> to save <see cref="M:Pixel.Node.GetComponent``1(System.Int32)"></see> calls/allocations.
            </summary>
        </member>
        <member name="F:Pixel.Node.sprite">
            <summary>
            A cached <see cref="T:Pixel.Sprite"></see> to save <see cref="M:Pixel.Node.GetComponent``1(System.Int32)"></see> calls/allocations.
            </summary>
        </member>
        <member name="P:Pixel.Node.Components">
            <summary>
            A dictionary of lists of <see cref="T:Pixel.Types.Components.Component"/> stored by <see cref="T:System.Type"/>
            </summary>
        </member>
        <member name="F:Pixel.Node.Transform">
            <summary>
            A transform matrix updated with Position,Rotation,Scale frequently.
            </summary>
        </member>
        <member name="M:Pixel.Node.Child(Pixel.Node)">
            <summary>
            The method used to insert a node as child of this one.
            </summary>
            <param name="child"></param>
        </member>
        <member name="M:Pixel.Node.TryRemoveChild(Pixel.Node)">
            <summary>
            A way to remove children if they exist under this one.
            </summary>
            <param name="child"></param>
            <returns></returns>
        </member>
        <member name="M:Pixel.Node.ContainsCycle(Pixel.Node)">
            <summary>
            Used to check whether theres a cyclic inclusion in the hierarchy above and below this node.
            </summary>
            <param name="newNode"></param>
            <returns></returns>
        </member>
        <member name="M:Pixel.Node.SubscribeToEngine(System.Boolean,Pixel.Stage)">
            <summary>
            A way for a node to subscribe/unsubscribe from events like Update/FixedUpdate/Awake/OnCollision etc. this does not need to be called by the user.
            </summary>
            <param name="v"></param>
            <param name="stage"></param>
        </member>
        <member name="M:Pixel.Node.Destroy">
            <summary>
            Destroys this node and all of it's components.
            </summary>
        </member>
        <member name="M:Pixel.Node.Dispose">
            <summary>
            Cleans up any managed resources referring to Node/Component as they don't have a great disposal system yet.
            </summary>
        </member>
        <member name="M:Pixel.Node.AddComponent``1">
            <summary>
            Used to add a component of type <see cref="!:T"/>
            </summary>
            <typeparam name="T"></typeparam>
            <returns></returns>
        </member>
        <member name="M:Pixel.Node.RemoveComponent(Pixel.Types.Components.Component)">
            <summary>
            Used to remove the specified instance of a component from this node.
            </summary>
            <param name="component"></param>
        </member>
        <member name="M:Pixel.Node.HasComponent``1">
            <summary>
            Check whether a node does or doesn't have a certain type of component.
            </summary>
            <typeparam name="T"></typeparam>
            <returns>True if exists, false if doesn't.</returns>
        </member>
        <member name="M:Pixel.Node.TryGetComponent``1(``0@,System.Int32)">
            <summary>
            Fetches a component only if it exists in this node.
            </summary>
            <typeparam name="T"></typeparam>
            <param name="component"></param>
            <param name="index"></param>
            <returns>False and null if component didn't exist, else true and component will be the component found.</returns>
        </member>
        <member name="M:Pixel.Node.GetComponents``1">
            <summary>
            </summary>
            <typeparam name="T"></typeparam>
            <returns>A list of components matching type T</returns>
        </member>
        <member name="M:Pixel.Node.GetComponent``1(System.Int32)">
            <summary>
            Gets a component by type.
            </summary>
            <typeparam name="T"></typeparam>
            <param name="index"></param>
            <returns>A component of type T if exists, else null.</returns>
        </member>
        <member name="T:Pixel.Color">
            <summary>
            A basic color structure used by Pixel. implicitly converts with System.Drawing.Color.
            </summary>
        </member>
        <member name="P:Pixel.Color.Random">
            <summary>
            shortcut to random pixel
            </summary>
        </member>
        <member name="F:Pixel.Color.Clear">
            <summary>
            it's not actually clear, just ARGB : 1,1,1,1
            </summary>
        </member>
        <member name="T:Pixel.RendererBase">
            <summary>
            an abstract starter class to base all Renderers on, which are utilized by the RenderHost.
            </summary>
        </member>
        <member name="F:Pixel.RendererBase.baseImage">
            <summary>
            The stage's background cached.
            </summary>
        </member>
        <member name="F:Pixel.RendererBase.frame">
            <summary>
            the frame that will be drawn to each cycle.
            </summary>
        </member>
        <member name="P:Pixel.RendererBase.Frame">
            <summary>
            the frame that is sent out after each cycle, cached.
            </summary>
        </member>
        <member name="P:Pixel.RendererBase.Stride">
            <summary>
            the stride of the render output image.
            </summary>
        </member>
        <member name="P:Pixel.RendererBase.Resolution">
            <summary>
            the cached resolution of the screen.
            </summary>
        </member>
        <member name="F:Pixel.RendererBase.baseImageDirty">
            <summary>
            dictates whether to redraw the background or not on the next cycle.
            </summary>
        </member>
        <member name="M:Pixel.RendererBase.Render(System.Windows.Controls.Image)">
            <summary>
            Sends the last rendered frame up to the UI.
            </summary>
            <param name="output"></param>
        </member>
        <member name="M:Pixel.RendererBase.Draw(Pixel.StageRenderInfo)">
            <summary>
            Takes all the prepared renderInfo from the stage and constructs an image starting with the base image.
            </summary>
            <param name="info"></param>
        </member>
        <member name="M:Pixel.RendererBase.Dispose">
            <summary>
            cleans up any managed/unmanaged resources from last cycle.
            </summary>
        </member>
        <member name="M:Pixel.RendererBase.WriteColorToFrame(Pixel.Color@,System.Numerics.Vector2@)">
            <summary>
            Places a pixel on the render texture of this cycle at the desired position.
            </summary>
            <param name="color"></param>
            <param name="framePos"></param>
        </member>
        <member name="M:Pixel.RendererBase.WriteColorToFrame(Pixel.Color@,System.Int32,System.Int32)">
            <summary>
            Places a pixel on the render texture of this cycle at the desired position.
            </summary>
            <param name="color"></param>
            <param name="framePos"></param>
        </member>
        <member name="M:Pixel.RendererBase.MarkDirty">
            <summary>
            calling this will redraw the background next frame
            </summary>
        </member>
        <member name="M:Pixel.RendererBase.ReadColorFromFrame(System.Numerics.Vector2)">
            <summary>
            Reads a color from the current render texture.
            </summary>
            <param name="vector2"></param>
            <returns></returns>
        </member>
        <member name="T:Pixel.RenderInfo">
            <summary>
            A container that stores info about rendering such as metrics.
            </summary>
        </member>
        <member name="T:Pixel.Stage">
            <summary>
            The container that represents a 2D Scene.
            </summary>
        </member>
        <member name="F:Pixel.Stage.background">
            <summary>
            The base image/ skybox that everything else will be drawn on/behind.
            </summary>
        </member>
        <member name="F:Pixel.Stage.nodes">
            <summary>
            the collection of nodes that belong to this stage.
            </summary>
        </member>
        <member name="P:Pixel.Stage.StageRenderInfo">
            <summary>
            A collection of data used to render this stage's renderables.
            </summary>
        </member>
        <member name="P:Pixel.Stage.DefaultBackgroundMetadata">
            <summary>
            Backup background image data.
            </summary>
        </member>
        <member name="M:Pixel.Stage.#ctor(System.String,Pixel.FileIO.Metadata,Pixel.Hierarchy,System.String)">
            <summary>
            Memberwise copy constructor
            </summary>
            <param name="name"></param>
            <param name="backgroundMeta"></param>
            <param name="nodes"></param>
            <param name="existingUUID"></param>
        </member>
        <member name="T:Pixel.StageRenderInfo">
            <summary>
            A container that represents a render cycle worth of any renderable data that exists in the current stage.
            </summary>
        </member>
        <member name="T:FieldAttribute">
            <summary>
            An attribute for serializing fields to the InspectorControl.
            </summary>
        </member>
        <member name="M:Pixel_Core.Types.Attributes.HideFromEditorAttribute.#ctor">
            <summary>
            This allows you to prevent a type from being added to AddComponent menu atm.
            </summary>
        </member>
    </members>
</doc>
