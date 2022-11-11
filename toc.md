# **pixel-engine**
---
---

## *Table of Contents*
1. [Scripting Interface](#scripting-interface)
1. [Stage](#stage)
1. [Node](#node)
1. [Component](#component)

---
---


## Scripting Interface
Tags: API, Tutorial

---

 This section is written to cover need-to-know information about creaing a derivative of the Script class as a custom runtime script.
 > This is currently the only supported method, which uses C# 9.0.

  1. [Overrides](#method-overrides)
  1. [Instantiation](#component-instantiation)
  1. [Script Debugging (NYI)](#debugging)
  ---
### Method Overrides  
- `public override void Awake()` - Called right after Application Startup.
- `public override void Update()`  - Called every rendering frame.
- `public override void FixedUpdate(float delta)` - Called every physics frame (fixed delta)
---
---
### Component Instantiation
Tags : Tutorial
 >`Node.AddComponent()` does not instantiate or return an `object`, instead requiring the user to do a few extras before adding a component to a node.

 >`Component` and `Script` instantiation are almost identical operations, hence the documentation for both leading to *this page*.

 For this example, we insantiate a generic `Component` object `'component'`.
 This is done for learning purposes, but the `abstract class`  `Component` *cannot be instantiated*.
 
 Steps in this tutorial :
 - instantiating the object.
 - adding the object to a node.
#### Instantiate Object
 We begin by creating an `object` of the `Type` of `Component` that we want to add. (ie. `Text`, `Rigidbody`). The `Type` must derive from `Component` (or derivative of) or `Script`.
 >The following code shows the instantiation of an object using the recent `new(){ }` syntax, but any method is acceptable.
 ```.cs
 Tutorial.cs (CSharp)
 public class Tutorial : Script
 {
	 public override void Awake()
	 {
		base.Awake(); 
		// base should always be called to 
		// register UUID and ComponentName.

		Vec2 position = Vec2.one * 13f;
		

		Component component = new()
		{
			isComponent = true,
			isSentientAI = true,
			growWeed = true,
			spawnPosition = position
		};
	 } 
}
 ```
 ---
#### Add Component
 ---
 After we've created an `instance` of the type of `Component` or `Script` that we are adding, 
 we simply add the `Component` reference to the `Node`. In this case, we use the `node.parentNode` assuming
 the `Script` is already attached to an `instance` of a `Node`, however, there are various ways to get a reference to a `Node`, as mentioned [Here](#referencing-nodes)
 ```.cs
 Tutorial.cs (CSharp) (Continued)
 public class Tutorial : Script
 {
		... (public override void Awake()) cont.
		Component component = new()
		{
			isComponent = true,
			isSentientAI = true,
			growWeed = true,
			spawnPosition = position
		};
		Node node = this.parentNode;
		node.AddComponent(component);
	 } 
}

 ```
  
 - [Finding a Reference to a Node](#referencing-nodes)
 - [Finding a Reference to a Component](#referencing-components)
---
## Stage
Tags: API

The `Stage` class represents the highest-level container in the hierarchy, equivalent to a 'Scene'.

---
### Methods
#### -Public 
`public Node FindNode(string name)`
 
 - Arguments :  `string` `name` - the name of the desired node.
 - Returns : the first `Node` found with the specified `name`.

 `public void RefreshStageDictionary()` 

 >This method should never be called by the user.
 - Arguments : `NULL`
 - Returns : `void`
 ---
### Properties 
#### -Public 
 `public Bitmap Background`

The background image that's rendering behind the sprites in the Stage.
Could be thought of as a Skybox.
#### -Static
 `public static Stage New`
		 
Shorthand for a new stage with no nodes.

 `public static Stage Empty`

Shorthand for a new, empty stage, but with missing data.

---
---
## Node
---
Tags: API

The `Node` is the base entity `object`, could be compared to an `Actor` or `Game Object`.

A `Node` :

- must belong to a `Stage`.
- may have any number of `Component`s.
- may have multiple of a single `Type` or `instance` of `Component`, which then have to be accessed by index.
    for more on Stacking Components, see [Here](#component-stacking)
### Methods

#### -Public

`public Component? GetComponent<T>(int? index)`

Gets and returns either the first found `Component` of `Type` `<T>` , or of type at specified `int?` `index`. 
- Type Arguments : `<T> where T : Component`
- Arguments: `int? index` 
- Returns: a `Component` as typeof `<T>`
- Exceptions: `MissingComponentException()`

			
>For most calls, `GetComponent<T>(int? index)`'s nullable integer argument 'index' is not neccesary, unless you are [Stacking Components](#component-stacking) 

>Note:  When a ? follows a type declaration, ex. `Component? GetComponent()`, this means the type is [Nullable](#nullable-type-references)

`public void AddComponent(Component component)`
 
 Adds an instance of a component to the node and registers all events/callbacks.
 - Arguments: `Component` `component`
 - Returns : `void`


 `public bool TryGetComponent<T>(out Component? result) `
 
 - Type Arguments: `<T> where T : Component`
 - Out Arguments: `Component?` `result`
 - Returns: a `bool` representing the success of the query, and if true, an 
 [`out variable`](#out-variables)containing the `Component` that was searched for.

 ## Referencing Nodes
 Tags: Tutorial, API

 ### Queries:
 ### By `Name`
 Tags: API, Tutorial

 For this query type, you must first [get a reference to the currently loaded `Stage`](#referencing-stages). 
 
 Once obtained, the reference to the `Stage`, `stage` can be used to call the method `stage`.`FindNode(string name)`
 ### By `UUID`
 Tags: API, Experimental

 This query type is use-specific as it requires instanced data from the `node` that's being searched for.
 However, it is a simple method to verify an instance's most recent state.

 To query for a `Node` by `UUID`, you can make a request from the `static class` `pixel_engine.UUID`
 Method `Query<T>(string UUID)`
 ```.cs 
 public override void Awake()
 {
	string uuid = parentNode.UUID; 
	Node node = UUID.Query<Node>(uuid);

	if(node is not null) 
		System.Diagnostics.Debug.Log($"Node {node.Name} found.");

	// using system diagnostics log because a proper 
	// debug system is not implemented.
	// this is a windows application anyway.
 }
 ```
 > This type of query can be used to fetch any runtime instanced data, currently existent or not. 
 >> This also means the reference can easily be null, wrong, or invalid.

 ### By `ComponentArray`
 Tags : API, Experimental

 This query type is experimental, and could be useless. Given the provided parameter of type `ComponentArray`, return the
 first node that meets the minimum or maximum criteria. 
 
 To understand this more, let's look at what a `ComponentArray` `object`.

 ``` componentArray.cs 
public class ComponentArray
{
    public Dictionary<Type, Component?> Components = new();
    public ComponentArray(Type[] types, Component[]? values)
    {
        int i = 0; 
        foreach (var type in types)
        {
            Components.Add(type, values[i]);
            i++;
        }
    }
 }
 ```


---

## Component
### Referencing Components
 This section is written to cover the need-to-know about where, when, and how to get a `Component` reference.
### Component Stacking
 This section is written to cover the need-to-know about how to use the Component Stacking system, and some
 examples of where it could be applicable. 

















