using System;
using System.Collections.Generic;
using System.Text;

namespace ACE.Server.Entity
{
    public static class CustomLevelTables
    {
        static List<(int minlevel,int resist)> DotResistTable = new List<(int, int)>();
        static List<(int minlevel, float multiplier)> IncomingNetherDamageMultiplierTable = new List<(int, float)>();

        static CustomLevelTables()
        {
            DotResistTable.Add((0, 5));
            DotResistTable.Add((100, 10));
            DotResistTable.Add((150, 15));
            DotResistTable.Add((200, 30));
            IncomingNetherDamageMultiplierTable.Add((0, 0.95f));
            IncomingNetherDamageMultiplierTable.Add((150, 0.85f));
        }

        public static int GetDotResist(int level, int enlightenment)
        {
            if (enlightenment > 0)
                return DotResistTable[DotResistTable.Count - 1].resist;
            for(int i = DotResistTable.Count - 1; i >= 0; i--)
            {
                var item = DotResistTable[i];
                if (level >= item.minlevel)
                    return item.resist;
            }
            return 0;
        }

        public static float GetNetherResist(int level, int enlightenment)
        {
            if (enlightenment > 0)
                return IncomingNetherDamageMultiplierTable[IncomingNetherDamageMultiplierTable.Count - 1].multiplier;
            for (int i = IncomingNetherDamageMultiplierTable.Count - 1; i >= 0; i--)
            {
                var item = IncomingNetherDamageMultiplierTable[i];
                if (level >= item.minlevel)
                    return item.multiplier;
            }
            return 0;
        }
    }
}
