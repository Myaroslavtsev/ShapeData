using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    class GeneralMethods
    {
        public static bool RemoveListItems<T>(List<T> list, Predicate<T> condition)
        {
            var oddItems = list.Find(condition);
            if (oddItems != null)
            {
                list.Remove(oddItems);
                return true;
            }
            return false;
        }
    }


    

    class Dog : Animal
    {
        public void Bark()
        {
            Console.WriteLine("Woof!");
        }
    }

    class Animal
    {
        public string Name { get; set; }
        public void MakeNoise()
        {
            Console.WriteLine("Generic animal noise");
        }
    }
}
