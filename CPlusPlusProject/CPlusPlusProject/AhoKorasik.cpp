#include <string>
#include <vector>
#include <map>
#include <queue>

#include "Node.h"
#include "AhoKorasik.h"

AhoKorasik::AhoKorasik(std::vector<char> alphabet)
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
}

void AhoKorasik::Clear()
{
    alphabet.clear();
    delete root;
}

void AhoKorasik::AddStrings(std::vector<std::string> strings)
{
    for (int i = 0; i < strings.size(); ++i)
    {
        std::string str = strings[i];
        Node* current = root;

        for (int i = 0; i < str.size(); ++i)
        {
            if (current->Sons.find(str[i]) == current->Sons.end())
            {
                current->Sons[str[i]] = new Node(current, str[i]);
            }
            current = current->Sons[str[i]];
        }
        current->IsTerminal = true;
    }
}

void AhoKorasik::PrepareTransitions()
{
    if (IsPreparable)
    {
        std::queue<Node*> queue;
        queue.push(root);

        while (!queue.empty())
        {
            Node* current = queue.front();
            queue.pop();

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

            for (auto it = current->Sons.begin(); it != current->Sons.end(); ++it)
            {
                queue.push(it->second);
            }
        }
    }
}

bool AhoKorasik::IsOneOfStringsInText(std::string text)
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

bool AhoKorasik::IsOneOfStringsInText_Prepared(std::string text)
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