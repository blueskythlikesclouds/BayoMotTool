namespace BayoMotTool;

public static class PgHalf
{
    public static float ToSingle(ushort value)
    {
        uint sign = (uint)((value & 0x8000) >> 15);

        int exponent = (value & 0x7E00) >> 9;
        exponent -= 47;

        uint significand = (uint)(value & 0x01FF);

        if (exponent == -47)
        {
            if (significand == 0)
            {
                return sign == 0 ? +0.0f : -0.0f;
            }
            else
            {
                while ((significand & 0x0200) == 0)
                {
                    significand <<= 1;
                    exponent--;
                }
                significand &= 0x01FF;
                exponent++;
            }
        }

        exponent += 127;

        return BitConverter.UInt32BitsToSingle((sign << 31) | ((uint)exponent << 23) | (significand << 14));
    }
}
