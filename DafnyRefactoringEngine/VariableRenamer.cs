using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyRefactoringEngine
{
	/// <summary>
	/// Used to rename variables within a given code fragment of a parsed Dafny program.
	/// This will be used in a new generated method and not for editing the variable names within an existing method.
	/// </summary>
    public class VariableRenamer
    {
		public static void RenameVariable(string oldName, string newName, IEnumerable<Statement> statements)
		{
			foreach (Statement stmt in statements)
			{
				RenameVariable(oldName, newName, stmt.SubExpressions);
				RenameVariable(oldName, newName, stmt.SubStatements);
			}
		}

		public static void RenameVariable(string oldName, string newName, IEnumerable<Expression> expressions)
		{
			foreach (Expression expr in expressions)
			{
				if (expr is NameSegment || expr is IdentifierExpr)
				{
					if (expr.tok.val == oldName)
						expr.tok.val = newName;
				}
				else
				{
					RenameVariable(oldName, newName, expr.SubExpressions);
				}
			}
		}
    }
}
