//#define OPERATOR_CONCAT
#define REUSE_BUILDER

using System;
using System.Diagnostics;
using System.Linq;
#if !OPERATOR_CONCAT
using System.Text;
#endif

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable SpecifyACultureInStringConversionExplicitly
#if OPERATOR_CONCAT
// ReSharper disable LoopCanBeConvertedToQuery
#pragma warning disable 219
#endif

namespace StringBuilderPerf
{
    internal class Program
    {
        private static long _bytes;

        private static void Main ()
        {
            using (new Benchmark("total overall")) {
                Perf(100000000, 1, "1.{0}: tiny count");
                Perf(1000000, 100, "2.{0}: small count");
                Perf(10000, 10000, "3.{0}: medium count");
                Perf(100, 1000000, "4.{0}: big count");
                Console.WriteLine("Done!");
            }

            GC.Collect();
            Console.WriteLine("Total bytes wasted: {0:n0} :)", _bytes * sizeof(char));
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void Perf (int repeat, int count, string stage)
        {
            using (new Benchmark("total stage")) {
                using (new Benchmark("total stage part")) {
                    Console.WriteLine("Stage {0}, single length", string.Format(stage, 1));
                    foreach (int length in new[] { 1, 2, 5, 10, 20, 50, 100 })
                        Perf(repeat, count, length);
                }
                using (new Benchmark("total stage part")) {
                    Console.WriteLine("Stage {0}, single & double length", string.Format(stage, 2));
                    foreach (int length in new[] { 1, 2, 5, 10, 20, 50 })
                        Perf(repeat, count, length, length * 2);
                }
                using (new Benchmark("total stage part")) {
                    Console.WriteLine("Stage {0}, various length", string.Format(stage, 3));
                    foreach (int length in new[] { 1, 2, 5, 10 })
                        Perf(repeat, count, length, length * 10, length * 2, length * 5);
                }
                Console.WriteLine("Stage done!");
            }
        }

        private static void Perf (int repeat, int count, params int[] lengths)
        {
            //repeat /= 100;
            string[] strs = lengths.Select(i => new string('a', i)).ToArray();
            int totalChars = lengths.Sum() * count;
            _bytes += totalChars * repeat;
            using (new Benchmark(string.Format("append strings with lengths [{0}] {1:n0} times\n          total length {2:n0}, repeat {3:n0} times",
                string.Join(", ", lengths.Select(i => i.ToString()).ToArray()), count, totalChars, repeat))) {
#if OPERATOR_CONCAT
                while (repeat --> 0) {
                    string s = "";
                    int num = count;
                    while (num --> 0)
                        foreach (string str in strs)
                            s += str;
                }
#elif REUSE_BUILDER
                var sb = new StringBuilder();
                while (repeat --> 0) {
                    sb.Length = 0;
                    int num = count;
                    while (num --> 0)
                        foreach (string str in strs)
                            sb.Append(str);
                    sb.ToString();
                }
#else
                while (repeat --> 0) {
                    var sb = new StringBuilder();
                    int num = count;
                    while (num --> 0)
                        foreach (string str in strs)
                            sb.Append(str);
                    sb.ToString();
                }
#endif
            }
        }

        private class Benchmark : IDisposable
        {
            private readonly string _desc;
            private readonly Stopwatch _time = new Stopwatch();

            public Benchmark (string desc)
            {
                _desc = desc;
                _time.Start();
            }

            public void Dispose ()
            {
                _time.Stop();
                // use commented code
                //Console.WriteLine("  Time: {0:n0} ms\n    Task: {1}", _time.ElapsedMilliseconds, _desc);
                // fancy colored formatting
                Console.Write("  Time: ");
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("{0:n0}", _time.ElapsedMilliseconds);
                Console.ForegroundColor = oldColor;
                Console.WriteLine(" ms\n    Task: {0}", _desc);
            }
        }
    }
}