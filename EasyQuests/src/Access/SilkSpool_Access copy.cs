using HarmonyLib;
using System.Collections.Generic;

namespace owd.EasyQuests.Access
{
    public static class SilkSpool_Access
    {
        public static readonly AccessTools.FieldRef<SilkSpool, List<SilkChunk>> silkChunksRef =
        AccessTools.FieldRefAccess<SilkSpool, List<SilkChunk>>("silkChunks");
        
        public static List<SilkChunk>? GetChunks()
        {
            SilkSpool spool = SilkSpool.Instance;
            if (spool == null)
                return null;

            return silkChunksRef(spool);
        }
    }
}
