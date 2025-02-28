using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using LAV.RedBlackTree;

internal class Program
{
    private static void Main(string[] args)
    {
        //Examples();
        //Console.WriteLine("Examples Done!");

        //Task.WaitAll(AsyncExamples());
        //Console.WriteLine("AsyncExamples Done!");

        RedBlackTreeExamples();
        Console.WriteLine("RedBlackTreeExamples Done!");

        Console.ReadLine();
    }

    private static void RedBlackTreeExamples()
    {
        var rbt = new RedBlackTree<string>();
        rbt.Insert("apple");
        rbt.Insert("banana");
        rbt.Insert("cherry");

        Console.WriteLine(rbt.Search("banana")); // True
        Console.WriteLine(rbt.Search("mango"));  // False

        rbt.Delete("banana");
        Console.WriteLine(rbt.Search("banana")); // False
    }

    static void Examples()
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
            if (t.Search(300))
            {
                t.Delete(300);
                t.Insert(333);
            }

            if (t.Search(500))
            {
                t.Delete(500);
                t.Insert(555);
            }

            if (t.Search(700))
            {
                t.Delete(700);
                t.Insert(777);
            }
        });

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
            if (key < 1000)
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
    }

    static async Task AsyncExamples()
    {
        var tree = new ConcurrentRedBlackTree<int>();

        // Parallel async writes
        var writeTasks = new List<Task>();
        for (int i = 0; i < 1000; i++)
        {
            writeTasks.Add(tree.InsertAsync(i));
        }
        await Task.WhenAll(writeTasks);

        // Concurrent reads
        var readTasks = new List<Task<bool>>();
        for (int i = 0; i < 1000; i++)
        {
            readTasks.Add(tree.SearchAsync(i));
        }
        var results = await Task.WhenAll(readTasks);

        // Mixed sync/async usage
        Console.WriteLine("Sync insert 1001: {0}", tree.Insert(1001)); // Sync insert

        // Async check
        if (await tree.SearchAsync(1001))
            Console.WriteLine("Async check (Search 1001): found.");
        else
            Console.WriteLine("Async check (Search 1001): not found.");

        // Batched processing
#if !NET452
        await foreach (var item in tree)
        {
            Console.WriteLine(item);
        }
#else
        foreach (var item in tree)
        {
            Console.WriteLine(item);
        }
#endif
    }
}