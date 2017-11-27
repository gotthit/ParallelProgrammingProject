#pragma once

#include <string>
#include <vector>
#include <map>

#include "Node.h"

struct AhoKorasik
{
    std::vector<Node> memory;

    int root;
    std::vector<char> alphabet;
    bool IsPreparable;

    AhoKorasik(std::vector<char> alphabet = std::vector<char>());

    int AddNode(int parent = -1, char charToParent = (char)0);

    int GetTransition(int adress, char key);

    int GetSuffLink(int adress);

    int GetPressedSuffixLink(int adress);


    void AddString(std::string str);

    void AddStrings(std::vector<std::string> strings);

    void PrepareTransitions();

    bool IsOneOfStringsInText(std::string text);

    bool IsOneOfStringsInText_Prepared(std::string text);
};