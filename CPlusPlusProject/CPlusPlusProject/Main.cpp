#include <iostream>
#include <algorithm>

#include "AhoKorasik_Parallel.h"
#include "AhoKorasik.h"

std::vector<char> GetAlphabet(int maxChar)
{
    std::vector<char> alphabet;
    for (int i = 0; i < maxChar; ++i)
    {
        alphabet.push_back((char)i);
    }
    return alphabet;
}

std::string GenerateRandomString(int length, int upperCharIndex)
{
    std::string str;
    for (int i = 0; i < length; ++i)
    {
        str.push_back((char)(rand() % upperCharIndex));
    }
    return str;
}

void PrepareDataForTests(int testNumber, int maxChar, int textLength, int wordsLengt, int wordCount, 
                         std::vector<std::vector<std::string>> & words, std::vector<std::string> & textes)
{
    for (int i = 0; i < testNumber; ++i)
    {
        words.push_back(std::vector<std::string>());
        for (int j = 0; j < wordCount; ++j)
        {
            words[i].push_back(GenerateRandomString(wordsLengt, maxChar));
        }
        textes.push_back(GenerateRandomString(textLength, maxChar));
    }
}

double GetTrunclatedAverage(std::vector<double> numbers)
{
    double sum = 0;
    double count = 0;
    std::sort(numbers.begin(), numbers.end());

    for (int i = numbers.size() / 5; i < numbers.size() - numbers.size() / 5; ++i)
    {
        sum += numbers[i];
        ++count;
    }
    return count != 0 ? sum / count : 0;
}

bool CheckIfAnswersSame(int answersSize, std::vector<std::vector<bool>> answers)
{
    bool same = true;
    for (int i = 0; i < answersSize; ++i)
    {
        bool current = answers[0][i];
        for (int j = 1; j < answers.size(); ++j)
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

template <typename AhoKorasikT>
std::vector<bool> Test(std::vector<char> alphabet, std::vector<std::string> textes, std::vector<std::vector<std::string>> words)
{
    double averageAddTime = 0;
    double averagePrepareTime = 0;
    double averageFindTime = 0;

    double startTime;
    double finishTime;

    std::vector<double> addTimes;
    std::vector<double> prepareTimes;
    std::vector<double> findTimes;

    std::vector<bool> answers;
    bool answer;

    for (int i = 0; i < textes.size(); ++i)
    {
        AhoKorasikT korasik(alphabet);

        startTime = omp_get_wtime();
        korasik.AddStrings(words[i]);
        finishTime = omp_get_wtime();
        addTimes.push_back(finishTime - startTime);

        if (korasik.IsPreparable)
        {
            startTime = omp_get_wtime();
            korasik.PrepareTransitions();
            finishTime = omp_get_wtime();
            prepareTimes.push_back(finishTime - startTime);


            startTime = omp_get_wtime();
            answer = korasik.IsOneOfStringsInText_Prepared(textes[i]);
            finishTime = omp_get_wtime();
            findTimes.push_back(finishTime - startTime);
        }
        else
        {
            startTime = omp_get_wtime();
            answer = korasik.IsOneOfStringsInText(textes[i]);
            finishTime = omp_get_wtime();
            findTimes.push_back(finishTime - startTime);
        }
        answers.push_back(answer);

        korasik.Clear();
    }
    averageAddTime = GetTrunclatedAverage(addTimes);
    averagePrepareTime = GetTrunclatedAverage(prepareTimes);
    averageFindTime = GetTrunclatedAverage(findTimes);

    std::cout << std::endl << "      averageAddTime: " << averageAddTime 
                           << "  | averagePrepareTime: " << averagePrepareTime
                           << "  | averageFindTime: "<< averageFindTime << std::endl << std::endl;

    return answers;
}

int main()
{
    #ifdef _OPENMP
        std::cout << "OPEN_MP - ON" << std::endl;
    #else
        std::cout << "OPEN_MP - OFF" << std::endl;
    #endif


    const int testNumber = 10;

    for (int alpabetSize = 26; alpabetSize < 1000; alpabetSize *= 10)
    {
        for (int textLength = 10000000; textLength <= 10000000; textLength *= 10)
        {
            for (int wordsLength = 50; wordsLength <= textLength; wordsLength *= 10)
            {
                for (int wordCount = 10000; (wordsLength * wordCount) <= 1000000; wordCount *= 10)
                {
                    std::cout << std::endl << std::endl << " alpabetSize: " << alpabetSize 
                                                        << "  |  textLength: " << textLength
                                                        << "  |  wordsLength: " << wordsLength
                                                        << "  | wordCount: " << wordCount << std::endl << std::endl;

                    std::vector<std::vector<std::string>> words;
                    std::vector<std::string> textes;

                    std::vector<char> alphabet = GetAlphabet(alpabetSize);
                    std::vector<char> emptyAlphabet;

                    std::vector<std::vector<bool>> answers;

                    PrepareDataForTests(testNumber, alpabetSize, textLength, wordsLength, wordCount, words, textes);

                    // simple

                    answers.push_back(Test<AhoKorasik>(emptyAlphabet, textes, words));

                    answers.push_back(Test<AhoKorasik>(alphabet, textes, words));

                    // parallel

                    answers.push_back(Test<AhoKorasik_Parallel>(emptyAlphabet, textes, words));

                    answers.push_back(Test<AhoKorasik_Parallel>(alphabet, textes, words));

                    if (!CheckIfAnswersSame(testNumber, answers))
                    {
                        std::cout << "!!!!!!!    Mistake found     !!!!!!!!!" << std::endl;
                    }

                    system("pause");
                    return 0;
                }
            }
        }
    }





    system("pause");
    return 0;
}
