using System;
using System.Diagnostics;

namespace Solnet.Serum.Shared {

//==========================================================
// Bits/Flag Types. Derive from these to create a Bits/Flag
// class with specific interpretations for each bit
//==========================================================
public class BitsU8
{
    public byte Bits;
                               public BitsU8( byte  bits) { Bits = bits; }
    [Obsolete("Invalid type")] public BitsU8(sbyte  bits){}
    [Obsolete("Invalid type")] public BitsU8(ushort bits){}
    [Obsolete("Invalid type")] public BitsU8( short bits){}
    [Obsolete("Invalid type")] public BitsU8(uint   bits){}
    [Obsolete("Invalid type")] public BitsU8( int   bits){}
    [Obsolete("Invalid type")] public BitsU8(ulong  bits){}
    [Obsolete("Invalid type")] public BitsU8( long  bits){}
    public static implicit operator   BitsU8(byte bits) => new(bits);
    public bool Bit(int num) { Debug.Assert(num>=0 && num<8); return (Bits & (1U<<num)) != 0; }
    public bool Bit0 => (Bits & 0x01) != 0;
    public bool Bit1 => (Bits & 0x02) != 0;
    public bool Bit2 => (Bits & 0x04) != 0;
    public bool Bit3 => (Bits & 0x08) != 0;
    public bool Bit4 => (Bits & 0x10) != 0;
    public bool Bit5 => (Bits & 0x20) != 0;
    public bool Bit6 => (Bits & 0x40) != 0;
    public bool Bit7 => (Bits & 0x80) != 0;
}

public class BitsU16
{
    protected ushort Bits;
    [Obsolete("Invalid type")] public BitsU16( byte  bits){}
    [Obsolete("Invalid type")] public BitsU16(sbyte  bits){}
                               public BitsU16(ushort bits) { Bits = bits; }
    [Obsolete("Invalid type")] public BitsU16( short bits){}
    [Obsolete("Invalid type")] public BitsU16(uint   bits){}
    [Obsolete("Invalid type")] public BitsU16( int   bits){}
    [Obsolete("Invalid type")] public BitsU16(ulong  bits){}
    [Obsolete("Invalid type")] public BitsU16( long  bits){}
    public static implicit operator   BitsU16(ushort bits) => new(bits);
    public bool Bit(int num) { Debug.Assert(num>=0 && num<16); return (Bits & (1U<<num)) != 0; }
    public bool Bit00 => (Bits & 0x0001) != 0;    public bool Bit08 => (Bits & 0x0100) != 0;
    public bool Bit01 => (Bits & 0x0002) != 0;    public bool Bit09 => (Bits & 0x0200) != 0;
    public bool Bit02 => (Bits & 0x0004) != 0;    public bool Bit10 => (Bits & 0x0400) != 0;
    public bool Bit03 => (Bits & 0x0008) != 0;    public bool Bit11 => (Bits & 0x0800) != 0;
    public bool Bit04 => (Bits & 0x0010) != 0;    public bool Bit12 => (Bits & 0x1000) != 0;
    public bool Bit05 => (Bits & 0x0020) != 0;    public bool Bit13 => (Bits & 0x2000) != 0;
    public bool Bit06 => (Bits & 0x0040) != 0;    public bool Bit14 => (Bits & 0x4000) != 0;
    public bool Bit07 => (Bits & 0x0080) != 0;    public bool Bit15 => (Bits & 0x8000) != 0;
}

public class BitsU32
{
    protected uint Bits;
    [Obsolete("Invalid type")] public BitsU32( byte  bits){}
    [Obsolete("Invalid type")] public BitsU32(sbyte  bits){}
    [Obsolete("Invalid type")] public BitsU32(ushort bits){}
    [Obsolete("Invalid type")] public BitsU32( short bits){}
                               public BitsU32(uint   bits) { Bits = bits; }
    [Obsolete("Invalid type")] public BitsU32( int   bits){}
    [Obsolete("Invalid type")] public BitsU32(ulong  bits){}
    [Obsolete("Invalid type")] public BitsU32( long  bits){}
    public static implicit operator   BitsU32(uint bits) => new(bits);
    public bool Bit(int num) { return (Bits & (1U<<num)) != 0; }
    public bool Bit00 => (Bits & 0x00000001) != 0;    public bool Bit16 => (Bits & 0x00010000) != 0;
    public bool Bit01 => (Bits & 0x00000002) != 0;    public bool Bit17 => (Bits & 0x00020000) != 0;
    public bool Bit02 => (Bits & 0x00000004) != 0;    public bool Bit18 => (Bits & 0x00040000) != 0;
    public bool Bit03 => (Bits & 0x00000008) != 0;    public bool Bit19 => (Bits & 0x00080000) != 0;
    public bool Bit04 => (Bits & 0x00000010) != 0;    public bool Bit20 => (Bits & 0x00100000) != 0;
    public bool Bit05 => (Bits & 0x00000020) != 0;    public bool Bit21 => (Bits & 0x00200000) != 0;
    public bool Bit06 => (Bits & 0x00000040) != 0;    public bool Bit22 => (Bits & 0x00400000) != 0;
    public bool Bit07 => (Bits & 0x00000080) != 0;    public bool Bit23 => (Bits & 0x00800000) != 0;

    public bool Bit08 => (Bits & 0x00000100) != 0;    public bool Bit24 => (Bits & 0x01000000) != 0;
    public bool Bit09 => (Bits & 0x00000200) != 0;    public bool Bit25 => (Bits & 0x02000000) != 0;
    public bool Bit10 => (Bits & 0x00000400) != 0;    public bool Bit26 => (Bits & 0x04000000) != 0;
    public bool Bit11 => (Bits & 0x00000800) != 0;    public bool Bit27 => (Bits & 0x08000000) != 0;
    public bool Bit12 => (Bits & 0x00001000) != 0;    public bool Bit28 => (Bits & 0x10000000) != 0;
    public bool Bit13 => (Bits & 0x00002000) != 0;    public bool Bit29 => (Bits & 0x20000000) != 0;
    public bool Bit14 => (Bits & 0x00004000) != 0;    public bool Bit30 => (Bits & 0x40000000) != 0;
    public bool Bit15 => (Bits & 0x00008000) != 0;    public bool Bit31 => (Bits & 0x80000000) != 0;
}

public class BitsU64
{
    protected ulong Bits;
    [Obsolete("Invalid type")] public BitsU64( byte  bits){}
    [Obsolete("Invalid type")] public BitsU64(sbyte  bits){}
    [Obsolete("Invalid type")] public BitsU64(ushort bits){}
    [Obsolete("Invalid type")] public BitsU64( short bits){}
    [Obsolete("Invalid type")] public BitsU64(uint   bits){}
    [Obsolete("Invalid type")] public BitsU64( int   bits){}
                               public BitsU64(ulong  bits) { Bits = bits; }
    [Obsolete("Invalid type")] public BitsU64( long  bits){}
    public static implicit operator   BitsU64(ulong bits) => new(bits);
    public bool Bit(int num) { return (Bits & (1UL<<num)) != 0; }
    public bool Bit00 => (Bits & 0x00000001) != 0;    public bool Bit16 => (Bits & 0x00010000) != 0;
    public bool Bit01 => (Bits & 0x00000002) != 0;    public bool Bit17 => (Bits & 0x00020000) != 0;
    public bool Bit02 => (Bits & 0x00000004) != 0;    public bool Bit18 => (Bits & 0x00040000) != 0;
    public bool Bit03 => (Bits & 0x00000008) != 0;    public bool Bit19 => (Bits & 0x00080000) != 0;
    public bool Bit04 => (Bits & 0x00000010) != 0;    public bool Bit20 => (Bits & 0x00100000) != 0;
    public bool Bit05 => (Bits & 0x00000020) != 0;    public bool Bit21 => (Bits & 0x00200000) != 0;
    public bool Bit06 => (Bits & 0x00000040) != 0;    public bool Bit22 => (Bits & 0x00400000) != 0;
    public bool Bit07 => (Bits & 0x00000080) != 0;    public bool Bit23 => (Bits & 0x00800000) != 0;

    public bool Bit08 => (Bits & 0x00000100) != 0;    public bool Bit24 => (Bits & 0x01000000) != 0;
    public bool Bit09 => (Bits & 0x00000200) != 0;    public bool Bit25 => (Bits & 0x02000000) != 0;
    public bool Bit10 => (Bits & 0x00000400) != 0;    public bool Bit26 => (Bits & 0x04000000) != 0;
    public bool Bit11 => (Bits & 0x00000800) != 0;    public bool Bit27 => (Bits & 0x08000000) != 0;
    public bool Bit12 => (Bits & 0x00001000) != 0;    public bool Bit28 => (Bits & 0x10000000) != 0;
    public bool Bit13 => (Bits & 0x00002000) != 0;    public bool Bit29 => (Bits & 0x20000000) != 0;
    public bool Bit14 => (Bits & 0x00004000) != 0;    public bool Bit30 => (Bits & 0x40000000) != 0;
    public bool Bit15 => (Bits & 0x00008000) != 0;    public bool Bit31 => (Bits & 0x80000000) != 0;
}

} // Namespaces
