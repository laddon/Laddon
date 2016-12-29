using Microsoft.Boogie;
using Dfy = Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dafny;
using DafnyAnalyzer;

namespace MTA.Laddon
{
	/// <summary>
	/// Obsolete - the correct one is now within the DafnyAnalyzer assembly.
	/// </summary>
	//public class VariableFinder
	//{
		//public IEnumerable<Dfy.Formal> GetAllVariables(IEnumerable<Microsoft.Dafny.Statement> statements)
		//{
		//	var res = new List<Microsoft.Dafny.Formal>();

		//	foreach (var statement in statements)
		//	{
		//		var variables = GetVariables(statement);
		//		foreach (var v in variables)
		//		{
		//			if (!res.Any(x => x.DisplayName == v.DisplayName)) // var not in res
		//			{
		//				res.Add(v);
		//			}
		//		}
		//	}

		//	return res;
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(Statement statement)
		//{
		//	if (statement is UpdateStmt)
		//	{
		//		return GetVariables(statement as UpdateStmt);
		//	}
		//	else if (statement is CallStmt)
		//	{
		//		return GetVariables(statement as CallStmt);
		//	}
		//	else if (statement is AssertStmt)
		//	{
		//		return GetVariables(statement as AssertStmt);
		//	}
		//	else
		//	{
		//		throw new NotImplementedException(statement.ToString());
		//	}
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(UpdateStmt statement)
		//{
		//	var res = new List<Microsoft.Dafny.Formal>();

		//	AddVariablesFromExpressions(statement.Lhss, res);
		//	AddVariablesFromAssignments(statement.Rhss, res);
		//	AddVariablesFromStatements(statement.SubStatements, res);
		//	AddVariablesFromExpressions(statement.SubExpressions, res);

		//	return res;
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(CallStmt statement)
		//{
		//	var res = new List<Dfy.Formal>();

		//	AddVariablesFromExpressions(statement.Lhs, res);
		//	AddVariablesFromStatements(statement.SubStatements, res);
		//	AddVariablesFromExpressions(statement.SubExpressions, res);

		//	return res;
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(AssertStmt statement)
		//{
		//	var res = new List<Dfy.Formal>();

		//	AddVariablesFromSingleExpression(statement.Expr, res);
		//	AddVariablesFromStatements(statement.SubStatements, res);
		//	AddVariablesFromExpressions(statement.SubExpressions, res);

		//	return res;
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(Expression expression)
		//{
		//	if (expression is ApplySuffix)
		//	{
		//		return GetVariables(expression as ApplySuffix);
		//	}
		//	else if (expression is NameSegment)
		//	{
		//		return GetVariables(expression as NameSegment);
		//	}
		//	else if (expression is ExprDotName)
		//	{
		//		return GetVariables(expression as ExprDotName);
		//	}
		//	else if (expression is Microsoft.Dafny.IdentifierExpr)
		//	{
		//		return GetVariables(expression as Microsoft.Dafny.IdentifierExpr);
		//	}
		//	else if (expression is MemberSelectExpr)
		//	{
		//		return GetVariables(expression as MemberSelectExpr);
		//	}
		//	else if (expression is BinaryExpr)
		//	{
		//		return GetVariables(expression as BinaryExpr);
		//	}
		//	else
		//	{
		//		throw new NotImplementedException();
		//	}
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(ApplySuffix expression)
		//{
		//	var res = new List<Dfy.Formal>();

		//	AddVariablesFromSingleExpression(expression.Lhs, res);
		//	AddVariablesFromExpressions(expression.Args, res);

		//	return res;
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(NameSegment expression)
		//{
		//	return new List<Dfy.Formal>()
		//	{
		//		//TODO: check inParam, check isGhost
		//		new Dfy.Formal(expression.tok, expression.tok.val, expression.Type, true, false)
		//	};
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(ExprDotName expression)
		//{
		//	var res = new List<Dfy.Formal>();

		//	AddVariablesFromSingleExpression(expression.Lhs, res);
		//	AddVariablesFromExpressions(expression.SubExpressions, res);

		//	return res;
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(Microsoft.Dafny.IdentifierExpr expression)
		//{
		//	var res = new List<Dfy.Formal>();

		//	AddVariablesFromExpressions(expression.SubExpressions, res);

		//	return res;
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(MemberSelectExpr expression)
		//{
		//	var res = new List<Dfy.Formal>();

		//	AddVariablesFromExpressions(expression.SubExpressions, res);

		//	return res;
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(BinaryExpr expression)
		//{
		//	var res = new List<Dfy.Formal>();

		//	AddVariablesFromSingleExpression(expression.E0, res);
		//	AddVariablesFromSingleExpression(expression.E1, res);

		//	return res;
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(AssignmentRhs assignment)
		//{
		//	if (assignment is ExprRhs)
		//	{
		//		return GetVariables(assignment as ExprRhs);
		//	}
		//	else
		//	{
		//		throw new NotImplementedException();
		//	}
		//}

		//public IEnumerable<Dfy.Formal> GetVariables(ExprRhs assignment)
		//{
		//	var res = new List<Microsoft.Dafny.Formal>();

		//	AddVariablesFromSingleExpression(assignment.Expr, res);
		//	AddVariablesFromExpressions(assignment.SubExpressions, res);
		//	AddVariablesFromStatements(assignment.SubStatements, res);

		//	return res;
		//}

		//protected void AddVariablesFromStatements(IEnumerable<Statement> statements, ICollection<Dfy.Formal> vars)
		//{
		//	foreach (var statement in statements)
		//	{
		//		AddVariablesFromSingleStatement(statement, vars);
		//	}
		//}

		//protected void AddVariablesFromSingleStatement(Statement statement, ICollection<Dfy.Formal> vars)
		//{
		//	var variables = GetVariables(statement);
		//	foreach (var v in variables)
		//	{
		//		if (!vars.Any(x => x.DisplayName == v.DisplayName)) // var not in res
		//		{
		//			vars.Add(v);
		//		}
		//	}
		//}

		//protected void AddVariablesFromExpressions(IEnumerable<Expression> expressions, ICollection<Dfy.Formal> vars)
		//{
		//	foreach (var expression in expressions)
		//	{
		//		AddVariablesFromSingleExpression(expression, vars);
		//	}
		//}

		//protected void AddVariablesFromSingleExpression(Expression expression, ICollection<Dfy.Formal> vars)
		//{
		//	var variables = GetVariables(expression);
		//	foreach (var v in variables)
		//	{
		//		if (!vars.Any(x => x.DisplayName == v.DisplayName)) // var not in res
		//		{
		//			vars.Add(v);
		//		}
		//	}
		//}

		//protected void AddVariablesFromAssignments(IEnumerable<AssignmentRhs> assignments, ICollection<Dfy.Formal> vars)
		//{
		//	foreach (var assignment in assignments)
		//	{
		//		AddVariablesFromSingleAssignment(assignment, vars);
		//	}
		//}

		//protected void AddVariablesFromSingleAssignment(AssignmentRhs assignment, ICollection<Dfy.Formal> vars)
		//{
		//	var variables = GetVariables(assignment);
		//	foreach (var v in variables)
		//	{
		//		if (!vars.Any(x => x.DisplayName == v.DisplayName)) // var not in res
		//		{
		//			vars.Add(v);
		//		}
		//	}
		//}
    //}
}
