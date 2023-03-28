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
            property int Length
            {
                int get()
                {
                    return static_cast<int>(std::strlen(frame));
                }
            }
    };
}

