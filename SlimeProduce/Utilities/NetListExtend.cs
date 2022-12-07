using Netcode;
using System.Linq;

namespace SlimeProduce
{
    public static class NetListExtend
    {
        public static int CountIgnoreNull<T>(this NetObjectList<T> list) where T : class, INetObject<INetSerializable>
        {
            return list.Count(item => item != null);
        }
    }
}
