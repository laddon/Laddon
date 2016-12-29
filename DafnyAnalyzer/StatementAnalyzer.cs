using Common;
using DafnyCodeGen;
using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyAnalyzer
{
	public class StatementAnalyzer
	{
		#region Input Analyzer

		/// <summary>
		/// Returns all variables whose value is changed anywhere in the code fragment.
		/// </summary>
		/// <param name="statements">Collection of statements representing a code fragment</param>
		/// <returns>List of variables whose value is altered at least once anywhere in the statements</returns>
		public static IEnumerable<DafnyVariable> GetInVariables(IEnumerable<Statement> statements)
		{
			var ins = new List<DafnyVariable>();
			GetInVariables(statements, ref ins);
			return ins;
		}

		private static void GetInVariables(IEnumerable<Statement> statements, ref List<DafnyVariable> varCollection)
		{
			foreach (Statement statement in statements)
			{
				if (statement is AssignStmt)
				{
					GetInVariables(statement as AssignStmt, ref varCollection);
				}
				else if (statement is UpdateStmt)
				{
					GetInVariables(statement as UpdateStmt, ref varCollection);
				}
				else if (statement is WhileStmt)
				{
					GetInVariables(statement as WhileStmt, ref varCollection);
				}
				else if (statement is IfStmt)
				{
					GetInVariables(statement as IfStmt, ref varCollection);
				}
				else if (statement is BlockStmt)
				{
					GetInVariables(statement as BlockStmt, ref varCollection);
				}
				else if (statement is AssertStmt)
				{
					GetInVariables(statement as AssertStmt, ref varCollection);
				}
				else if (statement is AlternativeStmt)
				{
					GetInVariables(statement as AlternativeStmt, ref varCollection);
				}
				else if (statement is VarDeclStmt)
				{
					if (((VarDeclStmt)statement).Update != null)
						GetInVariables(((VarDeclStmt)statement).Update as UpdateStmt, ref varCollection);
				}
				else if (statement is CallStmt)
				{
					GetInVariables(statement as CallStmt, ref varCollection);
				}
				else if (statement is PrintStmt)
				{
					GetInVariables(statement as PrintStmt, ref varCollection);
				}
				else if (statement is AssignSuchThatStmt)
				{
					GetInVariables(statement as AssignSuchThatStmt, ref varCollection);
				}
				else if (statement is CalcStmt)
				{
					GetInVariables(statement as CalcStmt, ref varCollection);
				}
				else if (statement is MatchStmt)
				{
					GetInVariables(statement as MatchStmt, ref varCollection);
				}
				else
				{
					throw new NotImplementedException(statement.ToString());
				}
			}
		}

		private static void GetInVariables(UpdateStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach (Statement subStatement in statement.SubStatements)
			{
				if (subStatement is AssignStmt)
				{
					GetInVariables(subStatement as AssignStmt, ref varCollection);
				}
				else if (subStatement is CallStmt)
				{
					GetInVariables(subStatement as CallStmt, ref varCollection);
				}
			}
		}

		private static void GetInVariables(WhileStmt statement, ref List<DafnyVariable> varCollection)
		{
			// Grab all variables used in the loop guard.
			if (statement.Guard is ParensExpression)
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(statement.Guard as ParensExpression));
			else if (statement.Guard is BinaryExpr)
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(statement.Guard as BinaryExpr));

			// Grab all variables used in the loop invariants.
			foreach (var invariant in statement.Invariants)
			{
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(invariant.E));
			}

			// Grab all variables used in the "decreases" clauses.
			foreach (Expression dec in statement.Decreases.Expressions)
			{
				//GetInVariables(dec, ref varCollection);

				// TODO: Implement correct handling of "decreases"
				//addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(dec));
			}

			// Grab input variables from the loop body.
			foreach (Statement subStatement in statement.SubStatements)
			{
				if (subStatement is BlockStmt)
				{
					GetInVariables(subStatement as BlockStmt, ref varCollection);
				}
				else if (subStatement is UpdateStmt)
				{
					GetInVariables(subStatement as UpdateStmt, ref varCollection);
				}
			}
		}

		private static void GetInVariables(IfStmt statement, ref List<DafnyVariable> varCollection)
		{
			// Handle condition guard.
			if (statement.Guard is ParensExpression)
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(statement.Guard as ParensExpression));
			else if (statement.Guard is BinaryExpr)
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(statement.Guard as BinaryExpr));

			// Handle condition "true" body.
			GetInVariables(statement.Thn as BlockStmt, ref varCollection);

			// Handle alternative condition paths. ("if else" and "else")
			if (statement.Els != null)
			{
				// statement.Els is "if else {...}". Recursive call in this case.
				if (statement.Els is IfStmt)
					GetInVariables(statement.Els as IfStmt, ref varCollection);
				// statement.Els is "else {...}"																
				else if (statement.Els is BlockStmt)
					GetInVariables(statement.Els as BlockStmt, ref varCollection);
			}
		}

		private static void GetInVariables(BlockStmt statement, ref List<DafnyVariable> varCollection)
		{
			GetInVariables(statement.SubStatements, ref varCollection);
		}

		private static void GetInVariables(AlternativeStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach (Expression guardExpr in statement.SubExpressions)
			{
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(guardExpr));
			}

			foreach (Statement subStmt in statement.SubStatements)
			{
				GetInVariables(new List<Statement>() { subStmt }, ref varCollection);
			}

			//foreach (BlockStmt block in statement.SubStatements)
			//{
			//	GetInVariables(block, ref varCollection);		
			//}
		}

		private static void GetInVariables(AssertStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach (Expression expr in statement.SubExpressions)
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(expr));
		}

		private static void GetInVariables(AssignStmt statement, ref List<DafnyVariable> varCollection)
		{
			// Whatever is on the RHS of an assignment must be input.
			Microsoft.Dafny.Expression expr = statement.SubExpressions.Last();
			addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(expr));
		}

		private static void GetInVariables(PrintStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach(Expression arg in statement.Args)
			{
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(arg));
			}
		}

		private static void GetInVariables(AssignSuchThatStmt statement, ref List<DafnyVariable> varCollection)
		{
			addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(statement.Expr));
		}

		private static void GetInVariables(CalcStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach (BlockStmt hint in statement.Hints)
				GetInVariables(hint, ref varCollection);

			foreach (Expression line in statement.Lines)
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(line));
		}

		private static void GetInVariables(MatchStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach (MatchCase matchCase in statement.Cases)
			{
				if (matchCase is MatchCaseStmt)
					GetInVariables(((MatchCaseStmt)matchCase).Body, ref varCollection);
				else if (matchCase is MatchCaseExpr)
					addVariablesToCollection(ref varCollection, VariableFinder.GetVariables((matchCase as MatchCaseExpr).Body));

				// Local bound variables must not be included in the results.
				foreach (BoundVar boundVar in matchCase.Arguments)
				{
					varCollection.RemoveAll(x => x.Name == boundVar.Name);
				}
			}
		}

		private static void GetInVariables(CallStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach (Expression arg in statement.Args)
			{
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(arg));
			}
		}

		#endregion

		#region Output Analyzer

		/// <summary>
		/// Returns all variables whose value is used to change another variable's value anywhere in the code fragment.
		/// </summary>
		/// <param name="statements">Collection of statements representing a code fragment</param>
		/// <returns>List of variables whose value is at least once used to calculate 
		/// another variable's value anywhere in the statements</returns>
		public static IEnumerable<DafnyVariable> GetOutVariables(IEnumerable<Statement> statements)
		{
			var outs = new List<DafnyVariable>();
			GetOutVariables(statements, ref outs);
			return outs;
		}

		private static void GetOutVariables(IEnumerable<Statement> statements, ref List<DafnyVariable> varCollection)
		{
			foreach (Statement statement in statements)
			{
				if (statement is AssignStmt)
				{
					GetOutVariables(statement as AssignStmt, ref varCollection);
				}
				else if (statement is UpdateStmt)
				{
					GetOutVariables(statement as UpdateStmt, ref varCollection);
				}
				else if (statement is WhileStmt)
				{
					GetOutVariables(statement as WhileStmt, ref varCollection);
				}
				else if (statement is IfStmt)
				{
					GetOutVariables(statement as IfStmt, ref varCollection);
				}
				else if (statement is BlockStmt)
				{
					GetOutVariables(statement as BlockStmt, ref varCollection);
				}
				else if (statement is AlternativeStmt)
				{
					GetOutVariables(statement as AlternativeStmt, ref varCollection);
				}
				else if (statement is VarDeclStmt)
				{
					if (((VarDeclStmt)statement).Update != null)
						GetOutVariables(((VarDeclStmt)statement).Update as UpdateStmt, ref varCollection);
				}
				else if (statement is CallStmt)
				{
					GetOutVariables(statement as CallStmt, ref varCollection);
				}
				else if (statement is AssignSuchThatStmt)
				{
					GetOutVariables(statement as AssignSuchThatStmt, ref varCollection);
				}
				else if (statement is MatchStmt)
				{
					GetOutVariables(statement as MatchStmt, ref varCollection);
				}
			}
		}

		private static void GetOutVariables(UpdateStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach (Statement subStatement in statement.SubStatements)
			{
				if (subStatement is AssignStmt)
				{
					GetOutVariables(subStatement as AssignStmt, ref varCollection);
				}
				else if (subStatement is CallStmt)
				{
					GetOutVariables(subStatement as CallStmt, ref varCollection);
				}
			}
		}

		private static void GetOutVariables(WhileStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach (Statement subStatement in statement.SubStatements)
			{
				if (subStatement is BlockStmt)
				{
					GetOutVariables(subStatement as BlockStmt, ref varCollection);
				}
				else if (subStatement is UpdateStmt)
				{
					GetOutVariables(subStatement as UpdateStmt, ref varCollection);
				}
			}
		}

		private static void GetOutVariables(IfStmt statement, ref List<DafnyVariable> varCollection)
		{
			// Handle condition "true" body.
			GetOutVariables(statement.Thn as BlockStmt, ref varCollection);

			// Handle alternative condition paths. ("if else" and "else")
			if (statement.Els != null)
			{
				// statement.Els is "if else {...}". Recursive call in this case.
				if (statement.Els is IfStmt)
					GetOutVariables(statement.Els as IfStmt, ref varCollection);
				// statement.Els is "else {...}"																
				else if (statement.Els is BlockStmt)
					GetOutVariables(statement.Els as BlockStmt, ref varCollection);
			}
		}

		private static void GetOutVariables(AlternativeStmt statement, ref List<DafnyVariable> varCollection)
		{
			// Since case guards are binary expressions that never change the value of any variable,
			// we can ignore the SubExpressions when looking for output variables (UNLIKE looking for input variables).
			foreach (Statement subStmt in statement.SubStatements)
			{
				GetOutVariables(new List<Statement>() { subStmt }, ref varCollection);
			}

			//foreach (BlockStmt block in statement.SubStatements)
			//{
			//	GetOutVariables(block, ref varCollection);
			//}
		}

		private static void GetOutVariables(AssignSuchThatStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach(Expression expr in statement.Lhss)
			{
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(expr));
			}
		}

		private static void GetOutVariables(CallStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach (Expression lhsItem in statement.Lhs)
			{
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(lhsItem));
			}
		}

		private static void GetOutVariables(BlockStmt statement, ref List<DafnyVariable> varCollection)
		{
			GetOutVariables(statement.SubStatements, ref varCollection);
		}

		private static void GetOutVariables(AssignStmt statement, ref List<DafnyVariable> varCollection)
		{
			// Whatever is on the LHS of an assignment must be output,
			// except when updating an array cell - performed on an array that already exists on the heap.
			if (!(statement.Lhs is SeqSelectExpr))
				addVariablesToCollection(ref varCollection, VariableFinder.GetVariables(statement.Lhs));
		}

		private static void GetOutVariables(MatchStmt statement, ref List<DafnyVariable> varCollection)
		{
			foreach (MatchCase matchCase in statement.Cases)
			{
				if (matchCase is MatchCaseStmt)
					GetOutVariables(((MatchCaseStmt)matchCase).Body, ref varCollection);

				// Local bound variables must not be included in the results.
				foreach (BoundVar boundVar in matchCase.Arguments)
				{
					varCollection.RemoveAll(x => x.Name == boundVar.Name);
				}
			}
		}

		#endregion

		#region Pre- and Post-conditions Analyzer

		public static IEnumerable<DafnyVariable> GetInVariablesForConditions(Statement selectionScope, IEnumerable<Statement> statements)
		{
			var res = new List<DafnyVariable>();

			IEnumerable<string> varNamesInBody = VariableFinder.GetAllVariables(statements).Select(v => v.Name);

			IEnumerable<AssertStmt> preAsserts = GetPercedingAsserts(selectionScope, statements.First().Tok.pos);
			IEnumerable<AssertStmt> postAsserts = GetFollowingAsserts(selectionScope, statements.Last().EndTok.pos);
			var asserts = preAsserts.Concat(postAsserts);

			foreach (var assert in asserts)
			{
				var variablesInAssert = VariableFinder.GetVariables(assert);
				IEnumerable<string> varNamesInAssert = variablesInAssert.Select(v => v.Name);
				// NOTE: currently not checking if asserts (and their variables) are really needed.
				// Should eventually be implemented, correctly.
				//
				//if (varNamesInBody.Intersect(varNamesInAssert).Any())
				{
					// there are some vars both in body and in asserts
					var leftOutVars = variablesInAssert.Where(v => !varNamesInBody.Contains(v.Name));
					foreach (var leftOut in leftOutVars)
					{
						if (!res.Select(r => r.Name).Contains(leftOut.Name)) // var is not added yet
						{
							res.Add(leftOut);
						}
					}
				}
			}

			return res;
		}

		public static IEnumerable<DafnyVariable> GetInVariablesForConditions(Statement selectionScope, int offset)
		{
			var res = new List<DafnyVariable>();

			IEnumerable<AssertStmt> preAsserts = GetPercedingAsserts(selectionScope, offset);
			IEnumerable<AssertStmt> postAsserts = GetFollowingAsserts(selectionScope, offset);
			var asserts = preAsserts.Concat(postAsserts);

			foreach (var assert in asserts)
			{
				var variablesInAssert = VariableFinder.GetVariables(assert);
				IEnumerable<string> varNamesInAssert = variablesInAssert.Select(v => v.Name);
				
				foreach (var variable in variablesInAssert)
				{
					if (!res.Select(r => r.Name).Contains(variable.Name))
					{
						res.Add(variable);
					}
				}
			}

			return res;
		}

		public static IEnumerable<string> DeterminePreConditionsFromPrecedingAsserts(Statement selectionScope,
			IEnumerable<Statement> statements, IEnumerable<DafnyVariable> ins,
			IDictionary<string, string> rename)
		{
			var preconditions = new List<string>();

			IEnumerable<AssertStmt> asserts = GetPercedingAsserts(selectionScope, statements.First().Tok.pos);

			IEnumerable<string> usedVars = ins.Select(i => i.Name);

			foreach (var assert in asserts)
			{
				// NOTE: currently not checking if asserts are really needed.
				// Should eventually be implemented, correctly.
				//
				//var varsInAssert = VariableFinder.GetVariables(assert).Select(v => v.Name);
				//var allAssertVariablesInBody = !varsInAssert.Except(usedVars).Any();
				//if (allAssertVariablesInBody)
				{
					preconditions.Add(GeneratePreCondition(assert, rename));
				}
			}

			return preconditions;
		}

		public static IEnumerable<string> DeterminePreConditionsFromPrecedingAsserts(Statement selectionScope,
			int offset, IEnumerable<DafnyVariable> ins,
			IDictionary<string, string> rename)
		{
			var preconditions = new List<string>();

			IEnumerable<AssertStmt> asserts = GetPercedingAsserts(selectionScope, offset);

			IEnumerable<string> usedVars = ins.Select(i => i.Name);

			foreach (var assert in asserts)
			{
				preconditions.Add(GeneratePreCondition(assert, rename));
			}

			return preconditions;
		}

		public static IEnumerable<string> DeterminePostConditionsFromFollowingAsserts(Statement selectionScope,
			IEnumerable<Statement> statements, IEnumerable<DafnyVariable> ins,
			IEnumerable<DafnyVariable> outs, IDictionary<string, string> rename)
		{
			var postconditions = new List<string>();

			IEnumerable<AssertStmt> asserts = GetFollowingAsserts(selectionScope, statements.Last().EndTok.pos);

			IEnumerable<string> usedVars = ins.Select(i => i.Name).Concat(outs.Select(o => o.Name));

			foreach (var assert in asserts)
			{
				// NOTE: currently not checking if asserts are really needed.
				// Should eventually be implemented, correctly.
				//
				//var varsInAssert = VariableFinder.GetVariables(assert).Select(v => v.Name);
				//var allAssertVariablesInBody = !varsInAssert.Except(usedVars).Any();
				//if (allAssertVariablesInBody)
				{
					postconditions.Add(GeneratePostCondition(assert, rename));
				}
			}

			return postconditions;
		}

		public static IEnumerable<string> DeterminePostConditionsFromFollowingAsserts(Statement selectionScope,
			int offset, IEnumerable<DafnyVariable> ins,
			IEnumerable<DafnyVariable> outs, IDictionary<string, string> rename)
		{
			var postconditions = new List<string>();

			IEnumerable<AssertStmt> asserts = GetFollowingAsserts(selectionScope, offset);

			IEnumerable<string> usedVars = ins.Select(i => i.Name).Concat(outs.Select(o => o.Name));

			foreach (var assert in asserts)
			{
				postconditions.Add(GeneratePostCondition(assert, rename));
			}

			return postconditions;
		}

		private static IEnumerable<AssertStmt> GetPercedingAsserts(Statement selectionScope, int endOffset)
		{
			IEnumerable<AssertStmt> asserts;

			var inScope = new List<Statement>();
			selectionScope.GetSubStatementsRecursive(ref inScope);
			var allPreceding = inScope.Where(s => s.Tok.pos < endOffset);
			var nonAssert = allPreceding
				.Where(s => s is AssertStmt == false);
			if (nonAssert.Any())
			{
				var lastNonAssert = nonAssert.Last();
				asserts = allPreceding
					.Where(s => (s.Tok.pos > lastNonAssert.EndTok.pos) || (s.Tok.pos > lastNonAssert.Tok.pos &&
																			s.Tok.pos < lastNonAssert.EndTok.pos &&
																			endOffset > lastNonAssert.Tok.pos &&
																			endOffset < lastNonAssert.EndTok.pos))
					.OfType<AssertStmt>();
			}
			else
			{
				asserts = allPreceding.Cast<AssertStmt>();
			}

			return asserts;
		}

		private static IEnumerable<AssertStmt> GetFollowingAsserts(Statement selectionScope, int startOffset)
		{
			IEnumerable<AssertStmt> asserts;

			var inScope = new List<Statement>();
			selectionScope.GetSubStatementsRecursive(ref inScope);
			var allFollowing = inScope.Where(s => s.Tok.pos > startOffset);// || (s.Tok.pos < startOffset && s.EndTok.pos > startOffset));

			var nonAssert = allFollowing.Where(s => s is AssertStmt == false);

			if (nonAssert.Any())
			{
				var firstNonAssert = nonAssert.First();
				asserts = allFollowing
					.Where(s => (s.EndTok.pos < firstNonAssert.Tok.pos) || (s.EndTok.pos > firstNonAssert.Tok.pos &&
																			s.EndTok.pos < firstNonAssert.EndTok.pos &&
																			startOffset > firstNonAssert.Tok.pos &&
																			startOffset < firstNonAssert.EndTok.pos))
					.OfType<AssertStmt>();
			}
			else
			{
				asserts = allFollowing.Cast<AssertStmt>();
			}

			return asserts;
		}

		private static string GeneratePreCondition(AssertStmt assert, IDictionary<string, string> rename)
		{
			return "requires " + GenerateConditionBody(assert, rename);
		}

		private static string GeneratePostCondition(AssertStmt assert, IDictionary<string, string> rename)
		{
			return "ensures " + GenerateConditionBody(assert, rename);
		}

		private static string GenerateConditionBody(AssertStmt assert, IDictionary<string, string> rename = null)
		{
			var gen = new StatementGenerator(new List<VarDeclStmt>());
			return gen.GenerateConditionString(assert, rename);
		}

		public static bool AreStatementsFollowedByAssert(Statement selectionScope, IEnumerable<Statement> statements)
		{
			return GetFollowingAsserts(selectionScope, statements.Last().EndTok.pos).Any();
		}

		public static bool AreStatementsFollowedByAssert(Statement selectionScope, int offset)
		{
			return GetFollowingAsserts(selectionScope, offset).Any();
		}

		#endregion

		#region General

		/// <summary>
		/// Adds variables to the supplied variable collection.
		/// Variables that already exist in the target collection are ignored - No duplicates are allowed in the collection.
		/// </summary>
		/// <param name="collection">Target collection</param>
		/// <param name="varsToAdd">Variables to add</param>
		private static void addVariablesToCollection(ref List<DafnyVariable> collection, IEnumerable<DafnyVariable> varsToAdd)
		{
			foreach (DafnyVariable variable in varsToAdd)
			{
				if (!collection.Any(x => x.Name == variable.Name))
				{
					collection.Add
					(
						new DafnyVariable(variable.Name, variable.Type, variable.IsGhost)
					);
				}
			}
		}

		#endregion
	}


}
