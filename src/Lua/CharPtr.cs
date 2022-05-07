using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Lua;

public struct CharPtr
{
    readonly IntPtr Value;

    public CharPtr(IntPtr value)
    {
        this = new();
        Value = value;
    }

    public static implicit operator CharPtr(IntPtr ptr) => new(ptr);

    public static string StringFromNativeUtf8(IntPtr nativeUtf8, int len = 0)
    {
        if(len == 0)
            while(Marshal.ReadByte(nativeUtf8, len) != 0)
                ++len;
        if(len == 0)
            return string.Empty;
        var numArray = new byte[len];
        Marshal.Copy(nativeUtf8, numArray, 0, numArray.Length);
        return Encoding.UTF8.GetString(numArray, 0, len);
    }

    static string PointerToString(IntPtr ptr) => StringFromNativeUtf8(ptr);

    static string PointerToString(IntPtr ptr, int length) => StringFromNativeUtf8(ptr, length);

    static byte[] PointerToBuffer(IntPtr ptr, int length)
    {
        var destination = new byte[length];
        Marshal.Copy(ptr, destination, 0, length);
        return destination;
    }

    public override string ToString()
    {
        if(Value == IntPtr.Zero)
            return "";
        return PointerToString(Value);
    }

    public string ToString(int length)
    {
        if(Value == IntPtr.Zero)
            return "";
        var buffer = PointerToBuffer(Value, length);
        if(length <= 3 || buffer[0] != 27 || buffer[1] != 76 || buffer[2] != 117 || buffer[3] != 97)
            return Encoding.UTF8.GetString(buffer);
        var stringBuilder = new StringBuilder(length);
        foreach(var num in buffer)
            stringBuilder.Append((char)num);
        return stringBuilder.ToString();
    }
}