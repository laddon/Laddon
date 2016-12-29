using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MTA.Laddon
{
    interface IVariableFinder
    {
        IEnumerable<Span> GetVariableLocations(IWpfTextView view, string varName);
    }
}
