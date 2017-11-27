using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpProject
{
    interface IAhoKorasik
    {
        bool IsPreparable { get; }

        void AddStrings(List<string> strings);

        void PrepareTransitions();

        bool IsOneOfStringsInText(string text);

        bool IsOneOfStringsInText_Prepared(string text);
    }
}
