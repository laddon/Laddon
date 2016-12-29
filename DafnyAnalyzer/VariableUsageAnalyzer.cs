using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyAnalyzer
{
	public class VariableUsageAnalyzer
	{
		public static bool IsDeclaredInStatements(string varName, IEnumerable<Statement> statements, out VarDeclStmt declerationStatement)
		{
			foreach (Statement s in statements)
			{
				if (s is VarDeclStmt)
				{
					VarDeclStmt vds = s as VarDeclStmt;
					//foreach (Microsoft.Dafny.Expression exp in vds.Update.Lhss)
					foreach (LocalVariable locVar in vds.Locals)
					{
						if (locVar.Name == varName)
						{
							declerationStatement = vds;
							return true;
						}
					}
				}
				else
				{
					bool isDec = IsDeclaredInStatements(varName, s.SubStatements, out declerationStatement);
					if (isDec)
						return true;
				}
			}

			// Reaching here means a declaration of the specified variable was not found.
			declerationStatement = null;
			return false;
		}

		public static bool IsUsedAfterStatement(string varName, Method method, int fromOffset)
		{
			try
			{
				var followingStatements = method.GetAllStatementsRecursive()
					.Where(s => s.Tok.pos > fromOffset);

				bool isUsed = VariableFinder.GetAllVariables(followingStatements).Where(x => x.Name == varName).Any();
				return isUsed;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}
