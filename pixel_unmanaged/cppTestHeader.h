#pragma once

using namespace System::Collections::Generic;
using namespace System; 

public ref class cppClass 
{
	public:
		static int val;

		property static int integers 
		{
			int get() { return val; };
			void set(int value) { val = value; }
		}
};
