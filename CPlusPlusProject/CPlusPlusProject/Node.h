#pragma once

#include <map>
#include <omp.h>

struct Node
{
    std::map<char, Node*> Sons;
    std::map<char, Node*> Transitions;
    bool IsTerminal;
    Node* SuffLink;
    Node* PressedSuffixLink;
    Node* Parent;
    char CharToParent;

    omp_lock_t Locker;

    Node(Node* parent = NULL, char charToParent = (char)0);

    ~Node();

    Node* NonLockedGetTransition(char key);
    Node* NonLockedGetSuffLink();
    Node* NonLockedGetPressedSuffixLink();

    Node* GetTransition(char key);
    Node* GetSuffLink();
    Node* GetPressedSuffixLink();
};