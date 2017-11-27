using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpProject
{
    partial class AhoKorasik_Parallel
    {
        private class Node_Parallel
        {
            public ConcurrentDictionary<char, Node_Parallel> Sons;
            public ConcurrentDictionary<char, Node_Parallel> Transitions;
            public bool IsTerminal;
            public Node_Parallel SuffLink;
            public Node_Parallel PressedSuffixLink;
            public Node_Parallel Parent;
            public char CharToParent;

            public object Locker;

            public Node_Parallel(Node_Parallel parent = null, char charToParent = (char)0)
            {
                Sons = new ConcurrentDictionary<char, Node_Parallel>();
                Transitions = new ConcurrentDictionary<char, Node_Parallel>();
                IsTerminal = false;
                SuffLink = null;
                Parent = parent;
                CharToParent = charToParent;

                Locker = new object();
            }

            public Node_Parallel GetTransition(char key)
            {
                lock (Locker)
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
            }

            public Node_Parallel GetSuffLink()
            {
                lock (Locker)
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
            }

            public Node_Parallel GetPressedSuffixLink()
            {
                lock (Locker)
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
}
