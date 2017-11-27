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

        public static void PrepareDataForTests(int testNumber, int maxChar, int textLength, int wordsLengt, int wordCount, out List<List<string>> words, out List<string> textes)
        {
            words = new List<List<string>>();
            textes = new List<string>();

            for (int i = 0; i < testNumber; ++i)
            {
                words.Add(new List<string>());
                for (int j = 0; j < wordCount; ++j)
                {
                    words[i].Add(GenerateRandomString(wordsLengt, maxChar));
                }
                textes.Add(GenerateRandomString(textLength, maxChar));
            }
        }

        public static List<char> GetAlphabet(int maxChar)
        {
            return Enumerable.Range(0, maxChar).Select(x => (char)x).ToList();
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

            return count != 0 ? sum / count : 0;
        }

        public static List<bool> Test(IAhoKorasik korasik, List<string> textes, List<List<string>> words)
        {
            double averageAddTime = 0;
            double averagePrepareTime = 0;
            double averageFindTime = 0;

            List<double> addTimes = new List<double>();
            List<double> prepareTimes = new List<double>();
            List<double> findTimes = new List<double>();

            List<bool> answers = new List<bool>();
            bool answer;

            for (int i = 0; i < textes.Count; ++i)
            {
                GC.Collect();

                Stopwatch timer = new Stopwatch();
                timer.Start();
                korasik.AddStrings(words[i]);
                timer.Stop();
                addTimes.Add(timer.Elapsed.TotalSeconds);

                if (korasik.IsPreparable)
                {
                    timer.Restart();
                    korasik.PrepareTransitions();
                    timer.Stop();
                    prepareTimes.Add(timer.Elapsed.TotalSeconds);

                    timer.Restart();
                    answer = korasik.IsOneOfStringsInText_Prepared(textes[i]);
                    timer.Stop();
                    findTimes.Add(timer.Elapsed.TotalSeconds);
                }
                else
                {
                    timer.Restart();
                    answer = korasik.IsOneOfStringsInText(textes[i]);
                    timer.Stop();
                    findTimes.Add(timer.Elapsed.TotalSeconds);
                }
                answers.Add(answer);
            }
            averageAddTime = GetTrunclatedAverage(addTimes);
            averagePrepareTime = GetTrunclatedAverage(prepareTimes);
            averageFindTime = GetTrunclatedAverage(findTimes);

            Console.WriteLine();
            Console.WriteLine($"      averageAddTime: {averageAddTime}  | averagePrepareTime: {averagePrepareTime}  | averageFindTime: {averageFindTime} ");
            Console.WriteLine();

            return answers;
        }

        public static bool CheckIfAnswersSame(int answersSize, params List<bool>[] answers)
        {
            bool same = true;
            for (int i = 0; i < answersSize; ++i)
            {
                bool current = answers[0][i];
                for (int j = 1; j < answers.Length; ++j)
                {
                    if (current != answers[j][i])
                    {
                        same = false;
                        break;
                    }
                }
            }
            return same;
        }

        static void Main(string[] args)
        {
            //ThreadPool.SetMaxThreads(3, 3);

            const int testNumber = 1;

            //using (StreamWriter writer = new StreamWriter(@"..\..\..\CSharpTestResults.txt", true))
            {
                for (int alpabetSize = 50; alpabetSize < 1000; alpabetSize *= 10)
                {
                    for (int textLength = 1000000; textLength <= 10000000; textLength *= 10)
                    {
                        for (int wordsLength = 10000; wordsLength <= textLength; wordsLength *= 10)
                        {
                            for (int wordCount = 100; (wordsLength * wordCount) <= 1000000; wordCount *= 10)
                            {
                                //writer.WriteLine();
                                //writer.WriteLine();
                                //writer.WriteLine($" alpabetSize: {alpabetSize}  |  textSize: {textSize}  |  wordsSize: {wordsSize}  | wordCount: {wordCount} ");
                                //writer.WriteLine(" ----------- ");
                                //writer.WriteLine();

                                Console.WriteLine();
                                Console.WriteLine(" ----------- ");
                                Console.WriteLine($"             alpabetSize: {alpabetSize}  |  textSize: {textLength}  |  wordsSize: {wordsLength}  | wordCount: {wordCount} ");
                                Console.WriteLine(" ----------- ");

                                List<List<string>>  words;
                                List<string> textes;

                                PrepareDataForTests(testNumber, alpabetSize, textLength, wordsLength, wordCount, out words, out textes);

                                AhoKorasik simple = new AhoKorasik();
                                List<bool> simpleAnswers = Test(simple, textes, words);

                                AhoKorasik praparable = new AhoKorasik(GetAlphabet(alpabetSize));
                                List<bool> praparableAnswers = Test(praparable, textes, words);

                                AhoKorasik_Parallel parallel = new AhoKorasik_Parallel();
                                List<bool> parallelAnswers = Test(parallel, textes, words);

                                AhoKorasik_Parallel parallelPreparable = new AhoKorasik_Parallel(GetAlphabet(alpabetSize));
                                List<bool> parallelPreparableAnswers = Test(parallelPreparable, textes, words);

                                if (!CheckIfAnswersSame(testNumber, simpleAnswers, praparableAnswers, parallelAnswers, parallelPreparableAnswers))
                                {
                                    Console.WriteLine("!!!!!!!    Mistake found     !!!!!!!!!");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
