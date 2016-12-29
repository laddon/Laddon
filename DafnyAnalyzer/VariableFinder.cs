﻿using Dfy = Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dafny;
using Common;

namespace DafnyAnalyzer
{
	public static class VariableFinder
	{
		public static IEnumerable<DafnyVariable> GetAllVariables(IEnumerable<Statement> statements)
		{
			var res = new List<DafnyVariable>();

			foreach (var statement in statements)
			{
				var variables = GetVariables(statement);
				foreach (var v in variables)
				{
					if (!res.Any(x => x.Name == v.Name)) // var not in res
					{
						res.Add(v);
					}
				}
			}

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(Statement statement)
		{
			if (statement == null)
			{
				return new List<DafnyVariable>();
			}

			if (statement is UpdateStmt)
			{
				return GetVariables(statement as UpdateStmt);
			}
			else if (statement is CallStmt)
			{
				return GetVariables(statement as CallStmt);
			}
			else if (statement is AssertStmt)
			{
				return GetVariables(statement as AssertStmt);
			}
			else if (statement is AssignStmt)
			{
				return GetVariables(statement as AssignStmt);
			}
			else if (statement is WhileStmt)
			{
				return GetVariables(statement as WhileStmt);
			}
			else if (statement is IfStmt)
			{
				return GetVariables(statement as IfStmt);
			}
			else if (statement is AlternativeStmt)
			{
				return GetVariables(statement as AlternativeStmt);
			}
			else if (statement is BlockStmt)
			{
				return GetVariables(statement as BlockStmt);
			}
			else if (statement is VarDeclStmt)
			{
				return GetVariables(statement as VarDeclStmt);
			}
			else if (statement is PrintStmt)
			{
				return GetVariables(statement as PrintStmt);
			}
			else if (statement is AssignSuchThatStmt)
			{
				return GetVariables(statement as AssignSuchThatStmt);
			}
			else if (statement is CalcStmt)
			{
				return GetVariables(statement as CalcStmt);
			}
			else if (statement is MatchStmt)
			{
				return GetVariables(statement as MatchStmt);
			}
			else
			{
				throw new NotImplementedException(statement.ToString());
			}
		}

		public static IEnumerable<DafnyVariable> GetVariables(UpdateStmt statement)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromExpressions(statement.Lhss, res);
			AddVariablesFromAssignments(statement.Rhss, res);
			AddVariablesFromStatements(statement.SubStatements, res);
			AddVariablesFromExpressions(statement.SubExpressions, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(CallStmt statement)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromExpressions(statement.Lhs, res);
			AddVariablesFromStatements(statement.SubStatements, res);
			AddVariablesFromExpressions(statement.SubExpressions, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(AssertStmt statement)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromSingleExpression(statement.Expr, res);
			AddVariablesFromStatements(statement.SubStatements, res);
			AddVariablesFromExpressions(statement.SubExpressions, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(AssignStmt statement)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromSingleExpression(statement.Lhs, res);
			AddVariablesFromSingleAssignment(statement.Rhs, res);
			AddVariablesFromStatements(statement.SubStatements, res);
			AddVariablesFromExpressions(statement.SubExpressions, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(WhileStmt statement)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromSingleExpression(statement.Guard, res);
			AddVariablesFromStatements(statement.SubStatements, res);
			AddVariablesFromExpressions(statement.Invariants.Select(x => x.E), res);
			AddVariablesFromExpressions(statement.Decreases.Expressions.Where(x => !(x is AutoGeneratedExpression)), res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(IfStmt statement)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromSingleExpression(statement.Guard, res);
			AddVariablesFromStatements(statement.SubStatements, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(AlternativeStmt statement)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromExpressions(statement.SubExpressions, res);
			AddVariablesFromStatements(statement.SubStatements, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(BlockStmt statement)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromStatements(statement.SubStatements, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(VarDeclStmt statement)
		{
			var res = new List<DafnyVariable>();
			foreach (var local in statement.Locals)
			{
				//TODO: check inParam
				//var formal = new Dfy.Formal(local.Tok, local.CompileName, local.Type, true, false);
				var variable = new DafnyVariable(local.Name, local.Type, local.IsGhost);
				variable.IsDeclaredInSelectedStatements = true;
				res.Add(variable);
			}

			AddVariablesFromStatements(statement.SubStatements, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(PrintStmt statement)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromExpressions(statement.Args, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(AssignSuchThatStmt statement)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromExpressions(statement.Lhss, res);
			AddVariablesFromSingleExpression(statement.Expr, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(CalcStmt statement)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromStatements(statement.Hints, res);
			AddVariablesFromExpressions(statement.Steps, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(MatchStmt statement)
		{
			var res = new List<DafnyVariable>();
			foreach (MatchCase matchCase in statement.Cases)
			{
				if (matchCase is MatchCaseStmt)
					AddVariablesFromStatements((matchCase as MatchCaseStmt).Body, res);
				else if (matchCase is MatchCaseExpr)
					AddVariablesFromSingleExpression((matchCase as MatchCaseExpr).Body, res);

				// Local bound variables must not be included in the results.
				foreach (BoundVar boundVar in matchCase.Arguments)
					res.RemoveAll(x => x.Name == boundVar.Name);
			}

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(Expression expression)
		{
			if (expression == null)
			{
				return new List<DafnyVariable>();
			}

			if (expression is ApplySuffix)
			{
				return GetVariables(expression as ApplySuffix);
			}
			else if (expression is NameSegment)
			{
				return GetVariables(expression as NameSegment);
			}
			else if (expression is ExprDotName)
			{
				return GetVariables(expression as ExprDotName);
			}
			else if (expression is Microsoft.Dafny.IdentifierExpr)
			{
				return GetVariables(expression as Microsoft.Dafny.IdentifierExpr);
			}
			else if (expression is MemberSelectExpr)
			{
				return GetVariables(expression as MemberSelectExpr);
			}
			else if (expression is BinaryExpr)
			{
				return GetVariables(expression as BinaryExpr);
			}
			else if (expression is Microsoft.Dafny.LiteralExpr)
			{
				return new List<DafnyVariable>();
			}
			else if (expression is ChainingExpression)
			{
				return GetVariables(expression as ChainingExpression);
			}
			else if (expression is ParensExpression)
			{
				return GetVariables(expression as ParensExpression);
			}
			else if (expression is SeqSelectExpr)
			{
				return GetVariables(expression as SeqSelectExpr);
			}
			else if (expression is SeqDisplayExpr)
			{
				return GetVariables(expression as SeqDisplayExpr);
			}
			else if (expression is UnaryOpExpr)
			{
				return GetVariables(expression as UnaryOpExpr);
			}
			else if (expression is Dfy.ForallExpr)
			{
				return GetVariables(expression as Dfy.ForallExpr);
			}
			else if (expression is NegationExpression)
			{
				return GetVariables(expression as NegationExpression);
			}
			else if (expression is SetDisplayExpr)
			{
				return GetVariables(expression as SetDisplayExpr);
			}
			else if (expression is SetComprehension)
			{
				return GetVariables(expression as SetComprehension);
			}
			else if (expression is MultiSetDisplayExpr)
			{
				return GetVariables(expression as MultiSetDisplayExpr);
			}
			else if (expression is MultiSetFormingExpr)
			{
				return GetVariables(expression as MultiSetFormingExpr);
			}
			else if (expression is ImplicitThisExpr)
			{
				return GetVariables(expression as ImplicitThisExpr);
			}
			else if (expression is Microsoft.Dafny.OldExpr)
			{
				return GetVariables(expression as Microsoft.Dafny.OldExpr);
			}
			else if (expression is ForallExpr)
			{
				return GetVariables(expression as ForallExpr);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public static IEnumerable<DafnyVariable> GetVariables(ApplySuffix expression)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromSingleExpression(expression.Lhs, res);
			AddVariablesFromExpressions(expression.Args, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(NameSegment expression)
		{
			var res = new List<DafnyVariable>();

			if (IsFormal(expression))
			{
				//TODO: check inParam
				var isGhost = IsExpressionGhostVariable(expression);
				//var formal = new Dfy.Formal(expression.tok, expression.tok.val, expression.Type, true, false);
				var variable = new DafnyVariable(expression.tok.val, expression.Type, isGhost);
				res.Add(variable);
			}

			return res;
		}

		private static bool IsExpressionGhostVariable(NameSegment expression)
		{
			if (expression.ResolvedExpression is IdentifierExpr)
			{
				return (expression.ResolvedExpression as IdentifierExpr).Var.IsGhost;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public static IEnumerable<DafnyVariable> GetVariables(ExprDotName expression)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromSingleExpression(expression.Lhs, res);
			AddVariablesFromExpressions(expression.SubExpressions, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(IdentifierExpr expression)
		{
			var res = new List<DafnyVariable>();
			//AddVariablesFromExpressions(expression.SubExpressions, res);
			var variable = new DafnyVariable(expression.tok.val, expression.Type, expression.Var.IsGhost);
			res.Add(variable);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(MemberSelectExpr expression)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromExpressions(expression.SubExpressions, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(BinaryExpr expression)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromSingleExpression(expression.E0, res);
			AddVariablesFromSingleExpression(expression.E1, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(ChainingExpression expression)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromSingleExpression(expression.E, res);
			AddVariablesFromExpressions(expression.Operands, res);
			AddVariablesFromExpressions(expression.SubExpressions, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(ParensExpression expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromExpressions(expression.SubExpressions, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(SeqSelectExpr expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromSingleExpression(expression.Seq, res);
			if (expression.E0 != null)
				AddVariablesFromSingleExpression(expression.E0, res);
			if (expression.E1 != null)
				AddVariablesFromSingleExpression(expression.E1, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(SeqDisplayExpr expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromExpressions(expression.Elements, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(UnaryOpExpr expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromSingleExpression(expression.E, res);
			return res;
		}

		//public static IEnumerable<DafnyVariable> GetVariables(Dfy.ForallExpr expression)
		//{
		//	var res = new List<DafnyVariable>();
		//	AddVariablesFromSingleExpression(expression.Term, res);
		//	AddVariablesFromBoundedPools(expression.Bounds, res);
		//	return res;
		//}

		public static IEnumerable<DafnyVariable> GetVariables(NegationExpression expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromSingleExpression(expression.E, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(SetDisplayExpr expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromExpressions(expression.Elements, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(SetComprehension expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromSingleExpression(expression.Term, res);
			AddVariablesFromSingleExpression(expression.Range, res);

			// Bounded variables are meaningless outside the set comprehension expression, so we remove them from the result.
			var boundedVars = expression.BoundVars.Select(x => x.Name);
			res.RemoveAll(x => boundedVars.Contains(x.Name));

			// The "set" word may fetched from the comprehension expression and be added as a variable name, 
			// which is naturally undesired. We remove it from the result.
			res.RemoveAll(x => x.Name == "set");

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(MultiSetDisplayExpr expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromExpressions(expression.Elements, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(MultiSetFormingExpr expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromSingleExpression(expression.E, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(ImplicitThisExpr expression)
		{
			var res = new List<DafnyVariable>();
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(OldExpr expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromSingleExpression(expression.E, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(ForallExpr expression)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromSingleExpression(expression.Term, res);

			// Bounded vars have no meaning outside the expression and thus are not returnd.
			foreach (string boundedVarName in expression.BoundVars.Select(x => x.tok.val))
			{
				res.RemoveAll(x => x.Name == boundedVarName);
			}

			AddVariablesFromBoundedPools(expression.Bounds, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(AssignmentRhs assignment)
		{
			if (assignment == null)
			{
				return new List<DafnyVariable>();
			}

			if (assignment is ExprRhs)
			{
				return GetVariables(assignment as ExprRhs);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public static IEnumerable<DafnyVariable> GetVariables(ExprRhs assignment)
		{
			var res = new List<DafnyVariable>();

			AddVariablesFromSingleExpression(assignment.Expr, res);
			AddVariablesFromExpressions(assignment.SubExpressions, res);
			AddVariablesFromStatements(assignment.SubStatements, res);

			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(ComprehensionExpr.BoundedPool bound)
		{
			if (bound == null)
			{
				return new List<DafnyVariable>();
			}

			if (bound is ComprehensionExpr.IntBoundedPool)
			{
				return GetVariables(bound as ComprehensionExpr.IntBoundedPool);
			}
			else if (bound is ComprehensionExpr.SetBoundedPool)
			{
				return GetVariables(bound as ComprehensionExpr.SetBoundedPool);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public static IEnumerable<DafnyVariable> GetVariables(ComprehensionExpr.IntBoundedPool bound)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromSingleExpression(bound.UpperBound, res);
			AddVariablesFromSingleExpression(bound.LowerBound, res);
			return res;
		}

		public static IEnumerable<DafnyVariable> GetVariables(ComprehensionExpr.SetBoundedPool bound)
		{
			var res = new List<DafnyVariable>();
			AddVariablesFromSingleExpression(bound.Set, res);
			return res;
		}

		private static void AddVariablesFromStatements(IEnumerable<Statement> statements, ICollection<DafnyVariable> vars)
		{
			foreach (var statement in statements)
			{
				AddVariablesFromSingleStatement(statement, vars);
			}
		}

		private static void AddVariablesFromSingleStatement(Statement statement, ICollection<DafnyVariable> vars)
		{
			var variables = GetVariables(statement);
			foreach (var v in variables)
			{
				if (!vars.Any(x => x.Name == v.Name)) // var not in res
				{
					vars.Add(v);
				}
			}
		}

		private static void AddVariablesFromExpressions(IEnumerable<Expression> expressions, ICollection<DafnyVariable> vars)
		{
			foreach (var expression in expressions)
			{
				AddVariablesFromSingleExpression(expression, vars);
			}
		}

		private static void AddVariablesFromSingleExpression(Expression expression, ICollection<DafnyVariable> vars)
		{
			var variables = GetVariables(expression);
			foreach (var v in variables)
			{
				if (!vars.Any(x => x.Name == v.Name)) // var not in res
				{
					vars.Add(v);
				}
			}
		}

		private static void AddVariablesFromAssignments(IEnumerable<AssignmentRhs> assignments, ICollection<DafnyVariable> vars)
		{
			foreach (var assignment in assignments)
			{
				AddVariablesFromSingleAssignment(assignment, vars);
			}
		}

		private static void AddVariablesFromSingleAssignment(AssignmentRhs assignment, ICollection<DafnyVariable> vars)
		{
			var variables = GetVariables(assignment);
			foreach (var v in variables)
			{
				if (!vars.Any(x => x.Name == v.Name)) // var not in res
				{
					vars.Add(v);
				}
			}
		}

		private static void AddVariablesFromBoundedPools(IEnumerable<ComprehensionExpr.BoundedPool> bounds, ICollection<DafnyVariable> vars)
		{
			foreach (var bound in bounds)
			{
				AddVariablesFromSingleBoundedPool(bound, vars);
			}
		}
		
		private static void AddVariablesFromSingleBoundedPool(ComprehensionExpr.BoundedPool bound, ICollection<DafnyVariable> vars)
		{
			var variables = GetVariables(bound);
			foreach (var v in variables)
			{
				if (!vars.Any(x => x.Name == v.Name)) // var not in res
				{
					vars.Add(v);
				}
			}
		}

		private static bool IsFormal(NameSegment nameSegment)
		{
			// Making sure it's actually a variable, and not a method call
			// NOTE: There are probably more checks to make here.
			return !nameSegment.Type.IsArrowType;
		}
	}
}
