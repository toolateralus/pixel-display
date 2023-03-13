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


## Scripts
Tags: API, Tutorial

---

 This section is written to cover need-to-know information about creaing a derivative of the Script class as a custom runtime script.
 > This is currently the only supported method, which uses C# 9.0.

  1. [Overrides](#method-overrides)
  1. [Instantiation](#component-instantiation)
  1. [Script Debugging (NYI)](#debugging)
  ---
### Method Overrides  
- `public override void Awake()` - Called right before the game enters a running state.
- `public override void Update()`  - Called every rendering frame.
- `public override void FixedUpdate(float delta)` - Called every physics frame (fixed delta)
---
---
### Component Instantiation
Tags : Tutorial

 
#### Instantiate Object
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

















