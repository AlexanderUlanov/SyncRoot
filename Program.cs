using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncQueue
{
	class Program
	{
        private const int N = 100;
        private static SyncQueue<int> q = new SyncQueue<int>();

        static void Main(string[] args)
		{
            Task.Run(Read);
            // Parallel population
            Parallel.For(0, N, i => q.Push(i));

            // "Enter any number and it will be immediately printed. To quite enter any character (not a number)"
            while (true)
            {
                int i;
                if (int.TryParse(Console.ReadLine(), out i))
                {
                    q.Push(i);
                }
                else
                {
                    break;
                }
            }
        }

        private static async Task Read()
        {
            while (true)
                Console.WriteLine(await q.Pop().ConfigureAwait(false));
        }
    }
}
