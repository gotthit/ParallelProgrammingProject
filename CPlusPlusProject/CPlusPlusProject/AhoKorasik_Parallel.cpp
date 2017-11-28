#include <string>
#include <vector>
#include <map>
#include <queue>
#include <omp.h>
#include <iostream>
#include "math.h"
#include <algorithm>

#include "Node.h"
#include "AhoKorasik_Parallel.h"

AhoKorasik_Parallel::AhoKorasik_Parallel(std::vector<char> alphabet)
{
    root = new Node();
    if (!alphabet.empty())
    {
        this->alphabet = alphabet;
        IsPreparable = true;
    }
    else
    {
        IsPreparable = false;
    }
    MaxWordLength = 0;
}

void AhoKorasik_Parallel::Clear()
{
    alphabet.clear();
    delete root;
}

void AhoKorasik_Parallel::AddStrings(std::vector<std::string> strings)
{
    #pragma omp parallel for
    for (int i = 0; i < strings.size(); ++i)
    {
        std::string str = strings[i];

        if (MaxWordLength < str.size())
        {
            #pragma omp critical (fillingMaxWordLength)
            {
                MaxWordLength = std::max(MaxWordLength, str.size());
            }
        }

        Node* current = root;

        for (int i = 0; i < str.size(); ++i)
        {
            omp_lock_t * Locker = &(current->Locker); // start critical
            omp_set_lock(Locker);

            if (current->Sons.find(str[i]) == current->Sons.end())
            {
                current->Sons[str[i]] = new Node(current, str[i]);
            }
            current = current->Sons[str[i]];

            omp_unset_lock(Locker);                   // end critical
        }
        current->IsTerminal = true;
    }
}

void AhoKorasik_Parallel::PrepareTransitions()
{
    if (IsPreparable)
    {
        std::vector<Node*> layer;
        std::vector<Node*> nextLayer;
        layer.push_back(root);

        while (!layer.empty())
        {
            #pragma omp parallel for
            for (int i = 0; i < layer.size(); ++i)
            {
                Node* current = layer[i];

                // prepering suffix link
                if (current == root || current->Parent == root)
                {
                    current->SuffLink = root;
                }
                else
                {
                    current->SuffLink = current->Parent->SuffLink->Transitions[current->CharToParent];
                }

                // preparing pressed suffix link
                if (current->SuffLink->IsTerminal || current->SuffLink == root)
                {
                    current->PressedSuffixLink = current->SuffLink;
                }
                else
                {
                    current->PressedSuffixLink = current->SuffLink->PressedSuffixLink;
                }

                // prepering suffix transitions
                for (int i = 0; i < alphabet.size(); ++i)
                {
                    char letter = alphabet[i];

                    if (current->Sons.find(letter) != current->Sons.end())
                    {
                        current->Transitions[letter] = current->Sons[letter];

                        #pragma omp critical (fillingNextLayer) 
                        {
                            nextLayer.push_back(current->Sons[letter]);
                        }
                    }
                    else if (current == root)
                    {
                        current->Transitions[letter] = current;
                    }
                    else
                    {
                        current->Transitions[letter] = current->SuffLink->Transitions[letter];
                    }
                }
            }
            layer.clear();
            std::swap(layer, nextLayer);
        }
    }
}

// almost same as IsOneOfStringsInText_Prepared
bool AhoKorasik_Parallel::IsOneOfStringsInText(std::string text)
{
    int possibleFullParts = (int)std::ceil((double)text.size() / MaxWordLength);
    int maxThreadNumber = std::sqrt(possibleFullParts);

    if (possibleFullParts >= 3 && maxThreadNumber >= 2)
    {
        int divideToParts = std::min(possibleFullParts - 1, maxThreadNumber);
        int subpartsInEachPart = (int)std::ceil((double)(possibleFullParts + divideToParts - 1) / divideToParts);

        bool result = false;

        #pragma omp parallel for
        for (int i = 0; i < divideToParts; ++i)
        {
            size_t begin = i * (subpartsInEachPart - 1) * MaxWordLength;
            size_t end = std::min(text.size(), (i + 1) * (subpartsInEachPart - 1) * MaxWordLength + MaxWordLength);

            Node* current = root;
            for (size_t i = begin; i < end; ++i)
            {
                current = current->GetTransition(text[i]);
                if (result || current->IsTerminal || current->GetPressedSuffixLink() != root)
                {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }
    else
    {
        Node* current = root;
        for (int i = 0; i < text.size(); ++i)
        {
            current = current->GetTransition(text[i]);
            if (current->IsTerminal || current->GetPressedSuffixLink() != root)
            {
                return true;
            }
        }
        return false;
    }
}

// almost same as IsOneOfStringsInText
bool AhoKorasik_Parallel::IsOneOfStringsInText_Prepared(std::string text)
{
    int possibleFullParts = (int)std::ceil((double)text.size() / MaxWordLength);
    int maxThreadNumber = std::sqrt(possibleFullParts);

    if (possibleFullParts >= 3 && maxThreadNumber >= 2)
    {
        int divideToParts = std::min(possibleFullParts - 1, maxThreadNumber);
        int subpartsInEachPart = (int)std::ceil((double)(possibleFullParts + divideToParts - 1) / divideToParts);

        bool result = false;

        #pragma omp parallel for
        for (int i = 0; i < divideToParts; ++i)
        {
            size_t begin = i * (subpartsInEachPart - 1) * MaxWordLength;
            size_t end = std::min(text.size(), (i + 1) * (subpartsInEachPart - 1) * MaxWordLength + MaxWordLength);

            Node* current = root;
            for (size_t i = begin; i < end; ++i)
            {
                current = current->Transitions[text[i]];
                if (result || current->IsTerminal || current->PressedSuffixLink != root)
                {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }
    else
    {
        Node* current = root;
        for (int i = 0; i < text.size(); ++i)
        {
            current = current->Transitions[text[i]];
            if (current->IsTerminal || current->PressedSuffixLink != root)
            {
                return true;
            }
        }
        return false;
    }
}