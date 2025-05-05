using System.Collections.Generic;

public static class ListShuffleExtensions
{
    private static System.Random rng = new System.Random();

    //public static void Shuffle<T>(this IList<T> list)
    //{
    //    int n = list.Count;
    //    while (n > 1)
    //    {
    //        n--;
    //        int k = rng.Next(n + 1);
    //        T value = list[k];
    //        list[k] = list[n];
    //        list[n] = value;
    //    }
    //}

    // Usage:
    // List<int> myList = new List<int> {1, 2, 3, 4, 5};
    // myList.Shuffle();
}