#pragma once

using namespace System;

namespace PixelNative {
	public ref class Native
	{
		char* frame;
		
		public:
		void SetPixel(int position, int color)
		{
			frame[position] = static_cast<char>(color); 
		};

		char GetPixel(int position)
		{
			return frame[position];
		}


	};
}
