using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyAnalyzer
{
	public static class MethodExtensions
	{
		public static IEnumerable<Statement> GetAllStatementsRecursive(this Method m)
		{
			List<Statement> statements = new List<Statement>();
			foreach (Statement subStmt in m.Body.SubStatements)
			{
				statements.Add(subStmt);
				subStmt.GetSubStatementsRecursive(ref statements);
			}

			return statements;
		}
	}

	public static class StatementExtensions
	{
		public static void GetSubStatementsRecursive(this Statement s, ref List<Statement> subStatements)
		{
			foreach (Statement subStmt in s.SubStatements)
			{
				subStatements.Add(subStmt);
				subStmt.GetSubStatementsRecursive(ref subStatements);
			}
		}
	}
}
