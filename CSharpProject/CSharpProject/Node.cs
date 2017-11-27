using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpProject
{
    partial class AhoKorasik
    {
        private class Node
        {
            public Dictionary<char, Node> Sons;
            public Dictionary<char, Node> Transitions;
            public bool IsTerminal;
            public Node SuffLink;
            public Node PressedSuffixLink;
            public Node Parent;
            public char CharToParent;

            public Node(Node parent = null, char charToParent = (char)0)
            {
                Sons = new Dictionary<char, Node>();
                Transitions = new Dictionary<char, Node>();
                IsTerminal = false;
                SuffLink = null;
                Parent = parent;
                CharToParent = charToParent;
            }

            public Node GetTransition(char key)
            {
                if (!Transitions.ContainsKey(key))
                {
                    if (Sons.ContainsKey(key))
                    {
                        Transitions[key] = Sons[key];
                    }
                    else if (Parent == null)
                    {
                        Transitions[key] = this;
                    }
                    else
                    {
                        Transitions[key] = GetSuffLink().GetTransition(key);
                    }
                }
                return Transitions[key];
            }

            public Node GetSuffLink()
            {
                if (SuffLink == null)
                {
                    if (Parent == null)
                    {
                        SuffLink = this;
                    }
                    else if (Parent.Parent == null)
                    {
                        SuffLink = Parent;
                    }
                    else
                    {
                        SuffLink = Parent.GetSuffLink().GetTransition(CharToParent);
                    }
                }
                return SuffLink;
            }

            public Node GetPressedSuffixLink()
            {
                if (PressedSuffixLink == null)
                {
                    if (GetSuffLink().IsTerminal || GetSuffLink().Parent == null)
                    {
                        PressedSuffixLink = GetSuffLink();
                    }
                    else
                    {
                        PressedSuffixLink = GetSuffLink().GetPressedSuffixLink();
                    }
                }
                return PressedSuffixLink;
            }
        }
    }
}
