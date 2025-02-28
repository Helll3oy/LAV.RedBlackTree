using System;
using System.Linq;
using System.Threading.Tasks;
using LAV.RedBlackTree;

internal class Program
{
    private static void Main(string[] args)
    {
        var tree = new ConcurrentRedBlackTree<int>();

        // Concurrent writes
        Parallel.Invoke(
            () => tree.Insert(300),
            () => tree.Insert(500),
            () => tree.Insert(700),
            () => tree.GetSnapshot()
        );


        // Atomic read-modify-write
        tree.BatchOperation(t =>
        {
            if (t.Search(500))
            {
                t.Delete(500);
                t.Insert(555);
            }
        });

        ////Thread-safe enumeration
        //var snapshot = tree.ReadOperation(t => t.ToList());

        //// Parallel population
        tree.ParallelInsert(Enumerable.Range(1, 100));

        //// Concurrent queries
        //var results = tree.ParallelSearch(Enumerable.Range(50000, 60000));

        //// Bulk parallel deletion
        //tree.ParallelDelete(key => key % 2 == 0); // Delete all even numbers

        var sorted = tree!.ToList();

        // Parallel processing
        tree.ParallelTraverse(key =>
        {
            if(key < 1000)
                Console.WriteLine(key);
        });

        // Safe enumeration with explicit disposal
        using (var enumerator = tree.GetLazyEnumerator())
        {
            while (enumerator.MoveNext())
            {
                Console.WriteLine(enumerator.Current);
            }
        }

        // LINQ integration (automatic disposal)
        var firstTen = tree.Take(10).ToList();

        Console.WriteLine("Done!");

        Console.ReadLine();
    }
}