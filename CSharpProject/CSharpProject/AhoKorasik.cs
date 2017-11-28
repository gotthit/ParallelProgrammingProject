using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpProject
{
    partial class AhoKorasik : IAhoKorasik
    {
        private Node root;
        private List<char> alphabet;
        public bool IsPreparable { get; private set; }

        public AhoKorasik(List<char> alphabet = null)
        {
            root = new Node();
            this.alphabet = alphabet;
            IsPreparable = alphabet != null && alphabet.Count != 0;
        }

        public void AddString(string str)
        {
            Node current = root;

            for (int i = 0; i < str.Length; ++i)
            {
                if (!current.Sons.ContainsKey(str[i]))
                {
                    current.Sons[str[i]] = new Node(current, str[i]);
                }
                current = current.Sons[str[i]];
            }
            current.IsTerminal = true;
        }

        public void AddStrings(List<string> strings)
        {
            foreach (string str in strings)
            {
                AddString(str);
            }
        }

        public void PrepareTransitions()
        {
            if (IsPreparable)
            {
                Queue<Node> queue = new Queue<Node>();
                queue.Enqueue(root);

                while (queue.Count != 0)
                {
                    Node current = queue.Dequeue();

                    // prepering suffix link
                    if (current == root || current.Parent == root)
                    {
                        current.SuffLink = root;
                    }
                    else
                    {
                        current.SuffLink = current.Parent.SuffLink.Transitions[current.CharToParent];
                    }

                    // preparing pressed suffix link
                    if (current.SuffLink.IsTerminal || current.SuffLink == root)
                    {
                        current.PressedSuffixLink = current.SuffLink;
                    }
                    else
                    {
                        current.PressedSuffixLink = current.SuffLink.PressedSuffixLink;
                    }

                    // prepering suffix transitions
                    foreach (char letter in alphabet)
                    {
                        if (current.Sons.ContainsKey(letter))
                        {
                            current.Transitions[letter] = current.Sons[letter];
                        }
                        else if (current == root)
                        {
                            current.Transitions[letter] = current;
                        }
                        else
                        {
                            current.Transitions[letter] = current.SuffLink.Transitions[letter];
                        }
                    }

                    foreach (Node node in current.Sons.Values)
                    {
                        queue.Enqueue(node);
                    }
                }
            }
        }

        public bool IsOneOfStringsInText(string text)
        {
            Node current = root;
            for (int i = 0; i < text.Length; ++i)
            {
                current = current.GetTransition(text[i]);
                if (current.IsTerminal || current.GetPressedSuffixLink() != root)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsOneOfStringsInText_Prepared(string text)
        {
            Node current = root;
            for (int i = 0; i < text.Length; ++i)
            {
                current = current.Transitions[text[i]];
                if (current.IsTerminal || current.PressedSuffixLink != root)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
