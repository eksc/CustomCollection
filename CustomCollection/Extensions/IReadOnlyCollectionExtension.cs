using System;
using System.Collections.Generic;

namespace CustomCollection.Extensions
{
    internal static class IReadOnlyCollectionExtension
    {
        /// <summary>
        /// Print in console TItem in collection
        /// </summary>
        /// <typeparam name="TItem">TItem</typeparam>
        /// <param name="items">IReadOnlyCollection with elements</param>
        public static void PrintItemsInCollection<TItem>(this IReadOnlyCollection<TItem> items)
        {
            if (items.Count < 1)
            {
                Console.WriteLine($"Collection without items");
                return;
            }

            foreach(var item in items)
            {
                Console.WriteLine(item.ToString());
            }
        }
    }
}
