#pragma once
#include <vector>
using namespace std; 

namespace CPP
{
 class TestClassCPP
	{
		public:
		  int value;
		  vector<int> values; 

		  int GetValue() { return value; };
		  void SetValue(int newValue)
		  {
			  value = newValue;
		  }
		  void DoAction() 
		  {
			  for (int i = 0; i < values.size(); ++i)
			  {
				  auto val = GetValue();
				  values.push_back(val * i);
				  SetValue(val);
			  }
		  }

	};
}