// Copyright (C) Microsoft Corporation. All rights reserved.

namespace BitConverter
{
    /// <summary>
    /// A little-endian BitConverter that converts base data types to an array of bytes, and an array of bytes to base data types. All conversions are in
    /// little-endian format regardless of machine architecture.
    /// </summary>
    internal class LittleEndianBitConverter : EndianBitConverter
    {
        // Instance available from EndianBitConverter.LittleEndian
        internal LittleEndianBitConverter() { }

        public override bool IsLittleEndian { get; } = true;

        public override byte[] GetBytes(short value)
        {
            return new byte[] { (byte)value, (byte)(value >> 8) };
        }

        public override byte[] GetBytes(int value)
        {
            return new byte[] { (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) };
        }

        public override byte[] GetBytes(long value)
        {
            return new byte[] {
                (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24),
                (byte)(value >> 32), (byte)(value >> 40), (byte)(value >> 48), (byte)(value >> 56)
            };
        }

        public override short ToInt16(byte[] value, int startIndex)
        {
            this.CheckArguments(value, startIndex, sizeof(short));

            return (short)((value[startIndex]) | (value[startIndex + 1] << 8));
        }

        public override int ToInt32(byte[] value, int startIndex)
        {
            this.CheckArguments(value, startIndex, sizeof(int));

            return (value[startIndex]) | (value[startIndex + 1] << 8) | (value[startIndex + 2] << 16) | (value[startIndex + 3] << 24);
        }

        public override long ToInt64(byte[] value, int startIndex)
        {
            this.CheckArguments(value, startIndex, sizeof(long));

            int lowBytes = (value[startIndex]) | (value[startIndex + 1] << 8) | (value[startIndex + 2] << 16) | (value[startIndex + 3] << 24);
            int highBytes = (value[startIndex + 4]) | (value[startIndex + 5] << 8) | (value[startIndex + 6] << 16) | (value[startIndex + 7] << 24);
            return ((uint)lowBytes | ((long)highBytes << 32));
        }
    }
}
