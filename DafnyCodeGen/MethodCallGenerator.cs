using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyCodeGen
{
    public class MethodCallGenerator
    {
		public string Generate(IEnumerable<DafnyVariable> ins,
			IEnumerable<DafnyVariable> outs,
            string methodName)
        {
            var bob = new StringBuilder();
			
            if (outs.Count() > 0)
            {
				IEnumerable<string> outNames;
				IEnumerable<DafnyVariable> varsToPreserveDeclarations = outs.Where(x => x.IsDeclaredInSelectedStatements && 
																						x.IsUsedAfterSelectedStatements);
				if (varsToPreserveDeclarations.Any())
				{
					bob.Append("var ");
					outNames = varsToPreserveDeclarations.Select(o => o.Name);
					bob.Append(String.Join(", ", outNames) + ";");
					bob.AppendLine();
					
				}

                outNames = outs.Select(o => o.Name);
                bob.Append(String.Join(", ", outNames));
                bob.Append(" := ");
            }

            bob.Append(methodName);
            bob.Append("(");

            if (ins.Count() > 0)
            {
                var inNames = ins.Select(i => i.Name);
                bob.Append(String.Join(", ", inNames));
            }

            bob.Append(")");

            bob.Append(";");

            return bob.ToString();
        }
    }
}
