#pragma once

#include <map>

struct Node
{
    std::map<char, int> Sons;
    std::map<char, int> Transitions;
    bool IsTerminal;
    int SuffLink;
    int PressedSuffixLink;
    int Parent;
    char CharToParent;


    Node(int parent = -1, char charToParent = (char)0);
};