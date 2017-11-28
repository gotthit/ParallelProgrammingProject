#pragma once

#include <string>
#include <vector>
#include <map>

#include "Node.h"

struct AhoKorasik
{
    Node* root;
    std::vector<char> alphabet;
    bool IsPreparable;

    AhoKorasik(std::vector<char> alphabet = std::vector<char>());

    void AddStrings(std::vector<std::string> strings);

    void PrepareTransitions();

    bool IsOneOfStringsInText(std::string text);

    bool IsOneOfStringsInText_Prepared(std::string text);

    void Clear();
};