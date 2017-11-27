using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpProject
{
    class Program
    {
        public static Random random = new Random(42);

        public static string GenerateRandomString(int length, int upperCharIndex)
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < length; ++i)
            {
                str.Append((char)random.Next(0, upperCharIndex));
            }
            return str.ToString();
        }

        public static double GetTrunclatedAverage(List<double> numbers)
        {
            double sum = 0;
            double count = 0;
            numbers.Sort();

            for (int i = numbers.Count / 5; i < numbers.Count - numbers.Count / 5; ++i)
            {
                sum += numbers[i];
                ++count;
            }

            return sum / count;
        }

        static void Main(string[] args)
        {
            const int numberOfTests = 50;

            using (StreamWriter writer = new StreamWriter(@"..\..\..\CSharpTestResults.txt", true))
            {
                for (int alpabetSize = 10; alpabetSize < 1000; alpabetSize *= 10)
                {
                    for (int textSize = 10000; textSize < 1000000; textSize *= 10)
                    {
                        for (int wordsSize = 100; wordsSize <= textSize; wordsSize *= 10)
                        {
                            for (int wordCount = 10; wordCount <= 1000; wordCount *= 10)
                            {
                                writer.WriteLine();
                                writer.WriteLine();
                                writer.WriteLine($" alpabetSize: {alpabetSize}  |  textSize: {textSize}  |  wordsSize: {wordsSize}  | wordCount: {wordCount} ");
                                writer.WriteLine(" ----------- ");
                                writer.WriteLine();

                                Console.WriteLine();
                                Console.WriteLine();
                                Console.WriteLine($" alpabetSize: {alpabetSize}  |  textSize: {textSize}  |  wordsSize: {wordsSize}  | wordCount: {wordCount} ");
                                Console.WriteLine(" ----------- ");
                                Console.WriteLine();

                                List<List<string>> worsdsForTest = new List<List<string>>();
                                List<string> textForTest = new List<string>();

                                List<bool> answerOnTests = new List<bool>();
                                List<double> addtimes = new List<double>();
                                List<double> preparetimes = new List<double>();
                                List<double> findtimes = new List<double>();

                                for (int i = 0; i < numberOfTests; ++i)
                                {
                                    worsdsForTest.Add(new List<string>());
                                    for (int j = 0; j < wordCount; ++j)
                                    {
                                        worsdsForTest[i].Add(GenerateRandomString(wordsSize, alpabetSize));
                                    }
                                    textForTest.Add(GenerateRandomString(textSize, alpabetSize));
                                }


                                // notprepapable_noparallel karasik
                                writer.WriteLine(" NOT prepapable NOT parallel ");
                                Console.WriteLine(" NOT prepapable NOT parallel ");

                                for (int i = 0; i < numberOfTests; ++i)   // add
                                {
                                    AhoKorasik notprepapable_notparallel = new AhoKorasik();

                                    GC.Collect();

                                    Stopwatch timer = new Stopwatch();
                                    timer.Start();
                                    notprepapable_notparallel.AddStrings(worsdsForTest[i]);
                                    timer.Stop();
                                    addtimes.Add(timer.Elapsed.TotalSeconds);

                                    GC.Collect();

                                    timer.Restart();
                                    bool answer = notprepapable_notparallel.IsOneOfStringsInText(textForTest[i]);
                                    timer.Stop();
                                    findtimes.Add(timer.Elapsed.TotalSeconds);

                                    answerOnTests.Add(answer);
                                }

                                writer.WriteLine(" ----------- ");
                                writer.WriteLine($" addtime: {GetTrunclatedAverage(addtimes)}      findtime: {GetTrunclatedAverage(findtimes)}  ");
                                writer.WriteLine(" ----------- ");
                                Console.WriteLine(" ----------- ");
                                Console.WriteLine($" addtime: {GetTrunclatedAverage(addtimes)}      findtime: {GetTrunclatedAverage(findtimes)}  ");
                                Console.WriteLine(" ----------- ");

                                addtimes.Clear();
                                preparetimes.Clear();
                                findtimes.Clear();







                                // prepapable_noparallel karasik
                                writer.WriteLine(" prepapable NOT parallel ");
                                Console.WriteLine(" prepapable NOT parallel ");

                                for (int i = 0; i < numberOfTests; ++i)   // add
                                {
                                    AhoKorasik prepapable_notparallel = new AhoKorasik(Enumerable.Range(0, alpabetSize).Select(x => (char)x).ToList());

                                    GC.Collect();

                                    Stopwatch timer = new Stopwatch();
                                    timer.Start();
                                    prepapable_notparallel.AddStrings(worsdsForTest[i]);
                                    timer.Stop();
                                    addtimes.Add(timer.Elapsed.TotalSeconds);

                                    GC.Collect();

                                    timer.Restart();
                                    prepapable_notparallel.PrepareTransitions();
                                    timer.Stop();
                                    preparetimes.Add(timer.Elapsed.TotalSeconds);

                                    GC.Collect();

                                    timer.Restart();
                                    bool answer = prepapable_notparallel.IsOneOfStringsInText_Prepared(textForTest[i]);
                                    timer.Stop();
                                    findtimes.Add(timer.Elapsed.TotalSeconds);

                                    if (answerOnTests[i] != answer)
                                    {
                                        Console.WriteLine("FUCK FUCK FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK");
                                        Console.WriteLine("FUCK FUCK FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK");
                                    }
                                }
                                writer.WriteLine(" ----------- ");
                                writer.WriteLine($" addtime: {GetTrunclatedAverage(addtimes)}     preparetime: {GetTrunclatedAverage(preparetimes)}     findtime: {GetTrunclatedAverage(findtimes)}  ");
                                writer.WriteLine(" ----------- ");
                                Console.WriteLine(" ----------- ");
                                Console.WriteLine($" addtime: {GetTrunclatedAverage(addtimes)}     preparetime: {GetTrunclatedAverage(preparetimes)}     findtime: {GetTrunclatedAverage(findtimes)}  ");
                                Console.WriteLine(" ----------- ");

                                addtimes.Clear();
                                preparetimes.Clear();
                                findtimes.Clear();





                                // nonprepapable_parallel karasik
                                writer.WriteLine(" NOT prepapable parallel ");
                                Console.WriteLine(" NOT prepapable parallel ");

                                for (int i = 0; i < numberOfTests; ++i)   // add
                                {
                                    AhoKorasik_Parallel notprepapable_parallel = new AhoKorasik_Parallel();

                                    GC.Collect();

                                    Stopwatch timer = new Stopwatch();
                                    timer.Start();
                                    notprepapable_parallel.AddStrings(worsdsForTest[i]);
                                    timer.Stop();
                                    addtimes.Add(timer.Elapsed.TotalSeconds);

                                    GC.Collect();

                                    timer.Restart();
                                    bool answer = notprepapable_parallel.IsOneOfStringsInText(textForTest[i]);
                                    timer.Stop();
                                    findtimes.Add(timer.Elapsed.TotalSeconds);

                                    if (answerOnTests[i] != answer)
                                    {
                                        Console.WriteLine("FUCK FUCK FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK");
                                        Console.WriteLine("FUCK FUCK FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK");
                                    }
                                }
                                writer.WriteLine(" ----------- ");
                                writer.WriteLine($" addtime: {GetTrunclatedAverage(addtimes)}        findtime: {GetTrunclatedAverage(findtimes)}  ");
                                writer.WriteLine(" ----------- ");
                                Console.WriteLine(" ----------- ");
                                Console.WriteLine($" addtime: {GetTrunclatedAverage(addtimes)}        findtime: {GetTrunclatedAverage(findtimes)}  ");
                                Console.WriteLine(" ----------- ");

                                addtimes.Clear();
                                preparetimes.Clear();
                                findtimes.Clear();






                                // prepapable_noparallel karasik
                                writer.WriteLine(" prepapable parallel ");
                                Console.WriteLine(" prepapable parallel ");

                                for (int i = 0; i < numberOfTests; ++i)   // add
                                {
                                    AhoKorasik_Parallel prepapable_parallel = new AhoKorasik_Parallel(Enumerable.Range(0, alpabetSize).Select(x => (char)x).ToList());

                                    GC.Collect();

                                    Stopwatch timer = new Stopwatch();
                                    timer.Start();
                                    prepapable_parallel.AddStrings(worsdsForTest[i]);
                                    timer.Stop();
                                    addtimes.Add(timer.Elapsed.TotalSeconds);

                                    GC.Collect();

                                    timer.Restart();
                                    prepapable_parallel.PrepareTransitions();
                                    timer.Stop();
                                    preparetimes.Add(timer.Elapsed.TotalSeconds);

                                    GC.Collect();

                                    timer.Restart();
                                    bool answer = prepapable_parallel.IsOneOfStringsInText_Prepared(textForTest[i]);
                                    timer.Stop();
                                    findtimes.Add(timer.Elapsed.TotalSeconds);

                                    if (answerOnTests[i] != answer)
                                    {
                                        Console.WriteLine("FUCK FUCK FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK");
                                        Console.WriteLine("FUCK FUCK FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK  FUCK");
                                    }
                                }
                                writer.WriteLine(" ----------- ");
                                writer.WriteLine($" addtime: {GetTrunclatedAverage(addtimes)}     preparetime: {GetTrunclatedAverage(preparetimes)}     findtime: {GetTrunclatedAverage(findtimes)}  ");
                                writer.WriteLine(" ----------- ");
                                Console.WriteLine(" ----------- ");
                                Console.WriteLine($" addtime: {GetTrunclatedAverage(addtimes)}     preparetime: {GetTrunclatedAverage(preparetimes)}     findtime: {GetTrunclatedAverage(findtimes)}  ");
                                Console.WriteLine(" ----------- ");

                                addtimes.Clear();
                                preparetimes.Clear();
                                findtimes.Clear();
                            }
                        }
                    }
                }
            }
        }
    }
}
