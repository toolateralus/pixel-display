
myObject 
{
	function::new()
	{
		outObj = 
		{
		  "string value",
		   1,
		} 	
		return outObj; 
	}
};

object = myObject:new()

print(object.outObj)

