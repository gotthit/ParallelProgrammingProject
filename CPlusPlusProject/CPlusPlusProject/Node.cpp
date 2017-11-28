#include "Node.h"
#include <map>
#include <omp.h>

Node::Node(Node* parent, char charToParent)
{
    Sons = std::map<char, Node*>();
    Transitions = std::map<char, Node*>();
    IsTerminal = false;
    SuffLink = NULL;
    PressedSuffixLink = NULL;
    Parent = parent;
    CharToParent = charToParent;

    omp_init_lock(&Locker);
}

Node::~Node()
{
    for (auto it = Sons.begin(); it != Sons.end(); ++it)
    {
        delete it->second;
    }

    Sons.clear();
    Transitions.clear();
}

Node* Node::NonLockedGetTransition(char key)
{
    if (Transitions.find(key) == Transitions.end())
    {
        if (Sons.find(key) != Sons.end())
        {
            Transitions[key] = Sons[key];
        }
        else if (Parent == NULL)
        {
            Transitions[key] = this;
        }
        else
        {
            Transitions[key] = NonLockedGetSuffLink()->GetTransition(key);
        }
    }
    return Transitions[key];
}

Node* Node::NonLockedGetSuffLink()
{
    if (SuffLink == NULL)
    {
        if (Parent == NULL)
        {
            SuffLink = this;
        }
        else if (Parent->Parent == NULL)
        {
            SuffLink = Parent;
        }
        else
        {
            SuffLink = Parent->GetSuffLink()->GetTransition(CharToParent);
        }
    }
    return SuffLink;
}

Node* Node::NonLockedGetPressedSuffixLink()
{
    if (PressedSuffixLink == NULL)
    {
        if (NonLockedGetSuffLink()->IsTerminal || NonLockedGetSuffLink()->Parent == NULL)
        {
            PressedSuffixLink = NonLockedGetSuffLink();
        }
        else
        {
            PressedSuffixLink = NonLockedGetSuffLink()->GetPressedSuffixLink();
        }
    }
    return PressedSuffixLink;
}

Node* Node::GetTransition(char key)
{
    omp_set_lock(&Locker);

    Node * result = NonLockedGetTransition(key);

    omp_unset_lock(&Locker);
    return result;
}

Node* Node::GetSuffLink()
{
    omp_set_lock(&Locker);

    Node * result = NonLockedGetSuffLink();

    omp_unset_lock(&Locker);
    return result;
}

Node* Node::GetPressedSuffixLink()
{
    omp_set_lock(&Locker);

    Node * result = NonLockedGetPressedSuffixLink();

    omp_unset_lock(&Locker);
    return result;
}