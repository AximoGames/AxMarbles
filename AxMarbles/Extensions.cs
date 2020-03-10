// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Aximo.Marbles
{

    public static class Extensions
    {

        public static List<MarbleColor> GetRegularColors(this MarbleColor marbleColors)
        {
            var list = GetEnumFlags<MarbleColor>(marbleColors);
            list.Remove(MarbleColor.None);
            list.Remove(MarbleColor.ColorJoker);
            list.Remove(MarbleColor.BombJoker);
            list.Remove(MarbleColor.BombColor);
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