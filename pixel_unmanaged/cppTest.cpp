#include "cppTestHeader.h"

using namespace CPP; 

int main(int argc, char* argv[]) {

	TestClassCPP* myClass = new TestClassCPP();
	
	while (myClass->value > 1) 
	{
		myClass->DoAction();
		
		_sleep(1); 
	}
	delete myClass; 
}

void Test()
{
	
	
}
