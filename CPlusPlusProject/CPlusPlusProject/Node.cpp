#include "Node.h"
#include <map>

Node::Node(int parent = -1, char charToParent = (char)0)
{
    Sons = std::map<char, int>();
    Transitions = std::map<char, int>();
    IsTerminal = false;
    SuffLink = -1;
    PressedSuffixLink = -1;
    Parent = parent;
    CharToParent = charToParent;
}
