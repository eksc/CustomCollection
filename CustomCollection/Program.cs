using CustomCollection.CustomCollection;
using CustomCollection.Extensions;
using CustomCollection.Models;
using System;
using System.Collections.Generic;

namespace CustomCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomCollection<UserType, string, double> testCollection = new CustomCollection<UserType, string, double>();

            var idKey = new UserType(Guid.NewGuid(), "Иванов Петр Николаевич", new DateTime(2000, 1, 1));
            var nameKey = "SecondKey";
            var value = 20.0;

            Console.WriteLine($"Add new item with idKey - «{idKey}», nameKey - «{nameKey}» and value «{value}»");

            testCollection.Add(idKey, nameKey, value);

            Console.WriteLine($"GetById items with key - «{idKey}»");

            testCollection.GetById(idKey)
                          .PrintItemsInCollection();

            var newValue = 30.0;

            Console.WriteLine($"Change on new value «{newValue}»");

            testCollection[idKey, nameKey] = newValue;

            Console.WriteLine($"GetByName items with key - «{nameKey}»");

            testCollection.GetByName(nameKey)
                          .PrintItemsInCollection();

            Console.WriteLine($"Remove item from collection");

            var removedItem = new KeyValuePair<CustomKeyPair<UserType, string>, double>
            (
                new CustomKeyPair<UserType, string>(idKey, nameKey),
                testCollection[idKey, nameKey]
            );

            if (testCollection.Remove(removedItem))
                Console.WriteLine("Remove Success");
            else
                Console.WriteLine("Remove Failed");

            Console.ReadKey();
        }
    }
}
