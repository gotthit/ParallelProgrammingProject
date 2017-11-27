#include <string>
#include <vector>
#include <map>
#include <queue>

#include "Node.h"
#include "AhoKorasik_Parallel.h"

AhoKorasik_Parallel::AhoKorasik_Parallel(std::vector<char> alphabet = std::vector<char>())
{
    root = AddNode();
    if (!alphabet.empty)
    {
        this->alphabet = alphabet;
        IsPreparable = true;
    }
    else
    {
        IsPreparable = false;
    }
}

int AhoKorasik_Parallel::AddNode(int parent = -1, char charToParent = (char)0)
{
    memory.push_back(Node(parent, charToParent));
    return memory.size() - 1;
}

int AhoKorasik_Parallel::GetTransition(int adress, char key)
{
    if (memory[adress].Transitions.find(key) == memory[adress].Transitions.end())
    {
        if (memory[adress].Sons.find(key) != memory[adress].Sons.end())
        {
            memory[adress].Transitions[key] = memory[adress].Sons[key];
        }
        else if (memory[adress].Parent == -1)
        {
            memory[adress].Transitions[key] = adress;
        }
        else
        {
            memory[adress].Transitions[key] = GetTransition(GetSuffLink(adress), key);
        }
    }
    return memory[adress].Transitions[key];
}

int AhoKorasik_Parallel::GetSuffLink(int adress)
{
    if (memory[adress].SuffLink == -1)
    {
        if (adress == root || memory[adress].Parent == root)
        {
            memory[adress].SuffLink = root;
        }
        else
        {
            memory[adress].SuffLink = GetTransition(GetSuffLink(memory[adress].Parent), memory[adress].CharToParent);
        }
    }
    return memory[adress].SuffLink;
}

int AhoKorasik_Parallel::GetPressedSuffixLink(int adress)
{
    if (memory[adress].PressedSuffixLink == -1)
    {
        if (memory[GetSuffLink(adress)].IsTerminal || memory[GetSuffLink(adress)].Parent == -1)
        {
            memory[adress].PressedSuffixLink = GetSuffLink(adress);
        }
        else
        {
            memory[adress].PressedSuffixLink = GetPressedSuffixLink(GetSuffLink(adress));
        }
    }
    return memory[adress].PressedSuffixLink;
}


void AhoKorasik_Parallel::AddString(std::string str)
{
    int current = root;

    for (int i = 0; i < str.size(); ++i)
    {
        if (memory[current].Sons.find(str[i]) == memory[current].Sons.end())
        {
            memory[current].Sons[str[i]] = AddNode(current, str[i]);
        }
        current = memory[current].Sons[str[i]];
    }
    memory[current].IsTerminal = true;
}

void AhoKorasik_Parallel::AddStrings(std::vector<std::string> strings)
{
    for (int i = 0; i < strings.size(); ++i)
    {
        AddString(strings[i]);
    }
}

void AhoKorasik_Parallel::PrepareTransitions()
{
    if (IsPreparable)
    {
        std::queue<int> queue;
        queue.push(root);

        while (!queue.empty())
        {
            int current = queue.front();
            queue.pop();

            // prepering suffix link
            if (current == root || memory[current].Parent == root)
            {
                memory[current].SuffLink = root;
            }
            else
            {
                memory[current].SuffLink = memory[memory[memory[current].Parent].SuffLink].Transitions[memory[current].CharToParent];
            }

            // preparing pressed suffix link
            if (memory[memory[current].SuffLink].IsTerminal || memory[current].SuffLink == root)
            {
                memory[current].PressedSuffixLink = memory[current].SuffLink;
            }
            else
            {
                memory[current].PressedSuffixLink = memory[memory[current].SuffLink].PressedSuffixLink;
            }

            // prepering suffix transitions
            for (int i = 0; i < alphabet.size(); ++i)
            {
                char letter = alphabet[i];

                if (memory[current].Sons.find(letter) != memory[current].Sons.end())
                {
                    memory[current].Transitions[letter] = memory[current].Sons[letter];
                }
                else if (current == root)
                {
                    memory[current].Transitions[letter] = current;
                }
                else
                {
                    memory[current].Transitions[letter] = memory[memory[current].SuffLink].Transitions[letter];
                }
            }

            for (auto it = memory[current].Sons.begin(); it != memory[current].Sons.end(); ++it)
            {
                queue.push(it->second);
            }
        }
    }
}

bool AhoKorasik_Parallel::IsOneOfStringsInText(std::string text)
{
    int current = root;
    for (int i = 0; i < text.size(); ++i)
    {
        current = GetTransition(current, text[i]);
        if (memory[current].IsTerminal || GetPressedSuffixLink(current) != root)
        {
            return true;
        }
    }
    return false;
}

bool AhoKorasik_Parallel::IsOneOfStringsInText_Prepared(std::string text)
{
    int current = root;
    for (int i = 0; i < text.size(); ++i)
    {
        current = memory[current].Transitions[text[i]];
        if (memory[current].IsTerminal || memory[current].PressedSuffixLink != root)
        {
            return true;
        }
    }
    return false;
}