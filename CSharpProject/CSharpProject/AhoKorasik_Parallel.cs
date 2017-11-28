using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace CSharpProject
{
    partial class AhoKorasik_Parallel : IAhoKorasik
    {
        private Node_Parallel root;
        private List<char> alphabet;

        public bool IsPreparable { get; private set; }

        private int maxWordLength;

        public AhoKorasik_Parallel(List<char> alphabet = null)
        {
            root = new Node_Parallel();
            this.alphabet = alphabet;
            maxWordLength = 0;
            IsPreparable = alphabet != null && alphabet.Count != 0;
        }

        #region adding

        public Task AddString(string str)
        {
            maxWordLength = Math.Max(maxWordLength, str.Length);

            return Task.Run(() =>
            {
                Node_Parallel current = root;

                for (int i = 0; i < str.Length; ++i)
                {
                    lock (current.Locker)
                    {
                        if (!current.Sons.ContainsKey(str[i]))
                        {
                            current.Sons[str[i]] = new Node_Parallel(current, str[i]);
                        }
                        current = current.Sons[str[i]];
                    }
                }
                current.IsTerminal = true;
            });
        }

        public void AddStrings(List<string> strings)
        {
            Task[] t = new Task[strings.Count];
            for (int i = 0; i < strings.Count; ++i)
            {
                t[i] = AddString(strings[i]);
            }
            Task.WaitAll(t);
        }

        #endregion adding

        #region preparation

        private Task PrepareTransition(Node_Parallel current)
        {
            return Task.Run(() =>
            {
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
                    //Console.WriteLine("ForEach    " + Thread.CurrentThread.ManagedThreadId);

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
                };
            });
        }

        public void PrepareTransitions()
        {
            if (IsPreparable)
            {
                List<Node_Parallel> layer = new List<Node_Parallel>() { root };

                while (layer.Count != 0)
                {
                    Task[] t = new Task[layer.Count];
                    for (int i = 0; i < layer.Count; ++i)
                    {
                        t[i] = PrepareTransition(layer[i]);
                    }

                    layer = layer.SelectMany(node => node.Sons.Values).ToList();

                    Task.WaitAll(t);
                }
            }
        }

        #endregion preparation

        #region searchInNotPrepared

        // almost same as FindInParfOfText_Prepared
        private Task<bool> FindInParfOfText(string text, int begin, int end)
        {
            return Task.Run(() =>
            {
                Node_Parallel current = root;
                for (int i = begin; i < end; ++i)
                {
                    current = current.GetTransition(text[i]);
                    if (current.IsTerminal || current.GetPressedSuffixLink() != root)
                    {
                        return true;
                    }
                }
                return false;
            });
        }

        // almost same as IsOneOfStringsInText_Prepared
        public bool IsOneOfStringsInText(string text)
        {
            int maxThreadNumber = 3;
            int possibleFullParts = (int)Math.Ceiling((double)text.Length / maxWordLength);

            if (possibleFullParts >= 3)
            {
                int divideToParts = Math.Min(possibleFullParts - 1, maxThreadNumber);
                int subpartsInEachPart = (int)Math.Ceiling((double)(possibleFullParts + divideToParts - 1) / divideToParts);

                Task<bool>[] t = new Task<bool>[divideToParts];

                for (int i = 0; i < divideToParts; ++i)
                {
                    t[i] = FindInParfOfText(
                        text,
                        i * (subpartsInEachPart - 1) * maxWordLength,
                        Math.Min(text.Length, (i + 1) * (subpartsInEachPart - 1) * maxWordLength + maxWordLength)
                    );
                }
                Task.WaitAll(t);
                return t.Any(x => x.Result == true);
            }
            else
            {
                Node_Parallel current = root;
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
        }

        #endregion searchInNotPrepared

        #region searchInPrepared

        // almost same as FindInParfOfText
        private Task<bool> FindInParfOfText_Prepared(string text, int begin, int end)
        {
            return Task.Run(() =>
            {
                Node_Parallel current = root;
                for (int i = begin; i < end; ++i)
                {
                    current = current.Transitions[text[i]];
                    if (current.IsTerminal || current.PressedSuffixLink != root)
                    {
                        return true;
                    }
                }
                return false;
            });
        }

        // almost same as IsOneOfStringsInText
        public bool IsOneOfStringsInText_Prepared(string text)
        {
            int maxThreadNumber = 3;
            int possibleFullParts = (int)Math.Ceiling((double)text.Length / maxWordLength);

            if (possibleFullParts >= 3)
            {
                int divideToParts = Math.Min(possibleFullParts - 1, maxThreadNumber);
                int subpartsInEachPart = (int)Math.Ceiling((double)(possibleFullParts + divideToParts - 1) / divideToParts);

                Task<bool>[] t = new Task<bool>[divideToParts];

                for (int i = 0; i < divideToParts; ++i)
                {
                    t[i] = FindInParfOfText_Prepared(
                        text, 
                        i * (subpartsInEachPart - 1) * maxWordLength, 
                        Math.Min(text.Length, (i + 1) * (subpartsInEachPart - 1) * maxWordLength + maxWordLength)
                    );
                }
                Task.WaitAll(t);
                return t.Any(x => x.Result == true);
            }
            else
            {
                Node_Parallel current = root;
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

        #endregion searchInPrepared
    }
}
