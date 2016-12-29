using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MTA.Laddon
{
    public class DummyVariableFinder : IVariableFinder
    {
        public IEnumerable<Span> GetVariableLocations(IWpfTextView view, string varName)
        {
            return new List<Span>
            {
                new Span(473, 1),
                new Span(431, 1),
                new Span(357, 1),
                new Span(291, 1),
                new Span(237, 1),
                new Span(216, 1)
            };
        }
    }
}
