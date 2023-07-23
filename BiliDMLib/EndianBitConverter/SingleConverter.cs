// Copyright (C) Microsoft Corporation. All rights reserved.

using System.Runtime.InteropServices;

namespace BitConverter
{
    // Converts between Single (float) and Int32 (int), as System.BitConverter does not have a method to do this in all .NET versions.
    // A union is used instead of an unsafe pointer cast so we don't have to worry about the trusted environment implications.
    [StructLayout(LayoutKind.Explicit)]
    internal struct SingleConverter
    {
        // map int value to offset zero
        [FieldOffset(0)] private readonly int intValue;

        // map float value to offset zero - intValue and floatValue now take the same position in memory
        [FieldOffset(0)] private readonly float floatValue;

        internal SingleConverter(int intValue)
        {
            floatValue = 0;
            this.intValue = intValue;
        }

        internal SingleConverter(float floatValue)
        {
            intValue = 0;
            this.floatValue = floatValue;
        }

        internal int GetIntValue()
        {
            return intValue;
        }

        internal float GetFloatValue()
        {
            return floatValue;
        }
    }
}