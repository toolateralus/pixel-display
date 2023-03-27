#pragma once

using namespace System;
#include <cstring>

namespace PixelNative {
    public ref class Native
    {
        char* frame;

    public:
        Native(int length)
        {
            frame = new char[length];
        }

        ~Native()
        {
            delete[] frame;
        }

        void SetData(char* data)
        {
            frame = data;
        }

        char* GetData()
        {
            return frame;
        }

        void SetPixel(int position, int color)
        {
            frame[position] = static_cast<char>(color);
        }

        char GetPixel(int position)
        {
            return frame[position];
        }

        property int Length
        {
            int get()
            {
                return static_cast<int>(std::strlen(frame));
            }
        }
    };
}

