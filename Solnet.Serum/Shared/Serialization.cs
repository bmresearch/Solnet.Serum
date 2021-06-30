using Solnet.Wallet;
using System;
using System.Buffers.Binary;
using System.Diagnostics;

namespace Solnet.Serum.Shared {

//=================================
// ReadOnlySpan Get/Put extensions
//=================================
public static class Serialization
{
    //-------------------------
    // GET (Unsigned Integers)
    //-------------------------
    public static byte GetU8(this ReadOnlySpan<byte> data, int offset)
    {
        const int len = 1; Debug.Assert(data.Length >= len);
        return data[offset];
    }

    public static ushort GetU16(this ReadOnlySpan<byte> data, int offset)
    {
        const int len = 2; Debug.Assert(data.Length >= len);
        return BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, len));
    }

    public static uint GetU32(this ReadOnlySpan<byte> data, int offset)
    {
        const int len = 4; Debug.Assert(data.Length >= len);
        return BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset, len));
    }

    public static ulong GetU64(this ReadOnlySpan<byte> data, int offset)
    {
        const int len = 8; Debug.Assert(data.Length >= len);
        return BinaryPrimitives.ReadUInt64LittleEndian(data.Slice(offset, len));
    }

    //-----------------------
    // GET (Signed Integers)
    //-----------------------
    public static sbyte GetS8(this ReadOnlySpan<byte> data, int offset)
    {
        const int len = 1; Debug.Assert(data.Length >= len);
        return (sbyte)data[offset];
    }

    public static short GetS16(this ReadOnlySpan<byte> data, int offset)
    {
        const int len = 2; Debug.Assert(data.Length >= len);
        return BinaryPrimitives.ReadInt16LittleEndian(data.Slice(offset, len));
    }

    public static int GetS32(this ReadOnlySpan<byte> data, int offset)
    {
        const int len = 4; Debug.Assert(data.Length >= len);
        return BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset, len));
    }

    public static long GetS64(this ReadOnlySpan<byte> data, int offset)
    {
        const int len = 8; Debug.Assert(data.Length >= len);
        return BinaryPrimitives.ReadInt64LittleEndian(data.Slice(offset, len));
    }

    //--------------------
    // GET (Custom Types)
    //--------------------
    public static PublicKey GetPublicKey(this ReadOnlySpan<byte> data, int offset)
    {
        const int len = 32; Debug.Assert(data.Length >= len);
        return new PublicKey(data.Slice(offset, len).ToArray());
    }

    public static OrderId GetOrderId(this ReadOnlySpan<byte> data, int offset)
    {
        Debug.Assert(data.Length >= OrderId.Length);
        return new OrderId(data.Slice(offset, OrderId.Length).ToArray());
    }
}

} // Namespaces
