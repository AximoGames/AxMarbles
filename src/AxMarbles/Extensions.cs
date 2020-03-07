using System;
using System.Collections.Generic;

namespace AxEngine
{

    public static class Extensions
    {

        public static List<MarbleColor> GetEnumFlags(this MarbleColor marbleColors)
        {
            var list = GetEnumFlags<MarbleColor>(marbleColors);
            list.Remove(MarbleColor.Joker);
            return list;
        }

        public static List<T> GetEnumFlags<T>(this T role)
            where T : Enum
        {
            var result = new List<T>();
            foreach (T r in Enum.GetValues(typeof(T)))
            {
                if (((int)(object)role & (int)(object)r) != 0)
                    result.Add(r);
            }
            return result;
        }

    }

}