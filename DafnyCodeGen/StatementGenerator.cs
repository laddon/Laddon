using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyCodeGen
{
	public class StatementGenerator
	{
		private IEnumerable<VarDeclStmt> decStatementsToPrintAsUpdates;

		public StatementGenerator(IEnumerable<VarDeclStmt> excludedStatements)
		{
			this.decStatementsToPrintAsUpdates = excludedStatements;
		}

		public string GenerateString(Statement statement, int tabs)
		{
			if (statement is AssignStmt)
				return GenerateString(statement as AssignStmt, tabs);
			else if (statement is UpdateStmt)
				return GenerateString(statement as UpdateStmt, tabs);
			else if (statement is WhileStmt)
				return GenerateString(statement as WhileStmt, tabs);
			else if (statement is IfStmt)
				return GenerateString(statement as IfStmt, false, tabs);
			else if (statement is AlternativeStmt)
				return GenerateString(statement as AlternativeStmt, tabs);
			else if (statement is BlockStmt)
				return GenerateString(statement as BlockStmt, tabs);
			else if (statement is VarDeclStmt)
				return GenerateString(statement as VarDeclStmt, tabs);
			else if (statement is CallStmt)
				return GenerateString(statement as CallStmt, tabs);
			else if (statement is AssertStmt)
				return GenerateString(statement as AssertStmt, tabs);
			else if (statement is PrintStmt)
				return GenerateString(statement as PrintStmt, tabs);
			else if (statement is AssignSuchThatStmt)
				return GenerateString(statement as AssignSuchThatStmt, tabs);
			else if (statement is CalcStmt)
				return GenerateString(statement as CalcStmt, tabs);
			else if (statement is MatchStmt)
				return GenerateString(statement as MatchStmt, tabs);
			else
				throw new NotImplementedException();
		}

		public string GenerateString(AssignStmt statement, int tabs)
		{
			var bob = new StringBuilder();
			if (statement.Rhs is ExprRhs)
			{
				ApplyIndentation(bob, tabs);
				bob.Append(GenerateString(statement.Lhs) +
					" " +
					statement.Tok.val +
					" " +
					GenerateString((statement.Rhs as ExprRhs).Expr) +
					statement.EndTok.val);
			}
			return bob.ToString();
		}

		public string GenerateString(UpdateStmt statement, int tabs)
		{
			var bob = new StringBuilder();
			ApplyIndentation(bob, tabs);
			var leftHandVals = statement.Lhss.Select(l => GenerateString(l));
			if (leftHandVals.Count() > 0)
			{
				var left = String.Join(", ", leftHandVals);
				bob.Append(left);

				bob.Append(" " + statement.Tok.val + " ");
			}

			bool addedRhs = false;
			foreach (var rhs in statement.Rhss)
			{
				if (rhs is ExprRhs)
				{
					var exprRhs = rhs as ExprRhs;
					bob.Append(GenerateString(exprRhs.Expr));
					bob.Append(", ");
					addedRhs = true;
				}
				else
				{
					throw new NotImplementedException();
				}
			}

			if (addedRhs)
				bob.Remove(bob.Length - 2, 2); // Remove tailing ", "
			bob.Append(statement.EndTok.val);

			return bob.ToString();
		}

		public string GenerateString(WhileStmt statement, int tabs)
		{
			var bob = new StringBuilder();
			ApplyIndentation(bob, tabs);
			bob.Append("while ");

			// Print guard.
			bob.AppendLine(GenerateString(statement.Guard));

			// Print invariants.
			statement.Invariants.ForEach(x =>
									{
										ApplyIndentation(bob, tabs);
										bob.AppendLine("  invariant " + GenerateString(x.E));
									});

			// Print "decreases" caluses.
			//statement.Decreases.Expressions.ForEach(x =>
			//						{
			//							ApplyIndentation(bob, tabs);
			//							bob.AppendLine("  decreases " + GenerateString(x));
			//						});

			// Print body.
			bob.AppendLine(GenerateString(statement.SubStatements.First(), tabs));
			return bob.ToString();
		}

		public string GenerateString(IfStmt statement, bool isElseIf, int tabs)
		{
			var bob = new StringBuilder();
			ApplyIndentation(bob, tabs);

			// Generate condition guard.
			if (!isElseIf)
				bob.Append("if ");
			else
				bob.Append("else if ");

			bob.Append(GenerateString(statement.Guard));

			// Generate condition "true" body
			bob.AppendLine();
			bob.AppendLine(GenerateString(statement.Thn, tabs));

			// Generate additional condition paths ("else if" and "else"...)
			if (statement.Els != null)
			{
				if (statement.Els is IfStmt)
				{
					bob.AppendLine(GenerateString(statement.Els as IfStmt, true, tabs));
				}
				else
				{
					ApplyIndentation(bob, tabs);
					bob.AppendLine("else ");
					bob.AppendLine(GenerateString(statement.Els as BlockStmt, tabs));
				}
			}

			return bob.ToString();
		}

		public string GenerateString(AlternativeStmt statement, int tabs)
		{
			Expression[] cases = statement.SubExpressions.ToArray();
			Statement[] caseBodies = statement.SubStatements.ToArray();

			var bob = new StringBuilder();
			ApplyIndentation(bob, tabs);

			bob.AppendLine("if {");
			for (int i = 0; i < cases.Length; i++)
			{
				ApplyIndentation(bob, tabs);
				bob.Append("case " + GenerateString(cases[i]) + " =>");
				bob.AppendLine();
				ApplyIndentation(bob, tabs + 1);
				bob.AppendLine("{");
				bob.AppendLine(GenerateString(caseBodies[i], tabs + 2));
				ApplyIndentation(bob, tabs + 1);
				bob.AppendLine("}");
			}
			ApplyIndentation(bob, tabs);
			bob.Append("}");

			return bob.ToString();
		}

		public string GenerateString(BlockStmt statement, int tabs, bool inline = false)
		{
			var bob = new StringBuilder();
			ApplyIndentation(bob, tabs);

			if (!inline)
			{
				bob.AppendLine("{");
				foreach (Statement subStmt in statement.SubStatements)
				{
					bob.AppendLine(GenerateString(subStmt, tabs + 1));
				}
				ApplyIndentation(bob, tabs);
				bob.Append("}");
				return bob.ToString();
			}
			else
			{
				Statement subStmt = statement.SubStatements.FirstOrDefault();
				if (subStmt != null)
				{
					if (subStmt is BlockStmt)
					{
						bob.Append(GenerateString(subStmt as BlockStmt, tabs, true));
					}
					else
					{
						bob.Append("{ ");
						bob.Append(GenerateString(subStmt, tabs));
						bob.Append(" }");
					}
				}
			}

			return bob.ToString();
		}

		public string GenerateString(VarDeclStmt statement, int tabs)
		{
			if (decStatementsToPrintAsUpdates.Contains(statement))
				return GenerateString(statement.Update as UpdateStmt, tabs);

			var bob = new StringBuilder();
			ApplyIndentation(bob, tabs);
			bob.Append("var ");
			if (statement.Update != null)
				bob.Append(GenerateString(statement.Update, 0));
			else
			{
				foreach (LocalVariable locVar in statement.Locals)
				{
					bob.Append(locVar.Name + ": " + locVar.Type.ToString() + ", ");
				}
				bob.Remove(bob.Length - 2, 2);
				bob.Append(statement.EndTok.val);
			}
			return bob.ToString();
		}

		public string GenerateString(CallStmt statement, int tabs)
		{
			var bob = new StringBuilder();
			ApplyIndentation(bob, tabs);

			// Generate left hand side.
			if (statement.Lhs.Any())
			{
				foreach (Expression lhs in statement.Lhs)
				{
					bob.Append(GenerateString(lhs));
					bob.Append(", ");
				}

				bob.Remove(bob.Length - 2, 2);
				bob.Append(" " + statement.Tok.val + " ");
			}

			// Generate right hand side.
			bob.Append(statement.Receiver.tok.val + "(");
			if (statement.Args.Any())
			{
				foreach (Expression arg in statement.Args)
				{
					bob.Append(GenerateString(arg));
					bob.Append(", ");
				}

				bob.Remove(bob.Length - 2, 2);
			}
			bob.Append(")" + statement.EndTok.val);

			return bob.ToString();
		}

		public string GenerateString(AssertStmt statement, int tabs)
		{
			StringBuilder bob = new StringBuilder();
			ApplyIndentation(bob, tabs);
			bob.Append(statement.Tok.val + " " + GenerateString(statement.Expr) + statement.EndTok.val);
			return bob.ToString();
		}

		public string GenerateString(PrintStmt statement, int tabs)
		{
			StringBuilder bob = new StringBuilder();
			ApplyIndentation(bob, tabs);
			bob.Append(statement.Tok.val + " "); // append "print "
			foreach (Expression arg in statement.Args)
			{
				bob.Append(GenerateString(arg));
				bob.Append(", ");
			}
			bob.Remove(bob.Length - 2, 2);
			bob.Append(statement.EndTok.val);
			return bob.ToString();
		}

		public string GenerateString(AssignSuchThatStmt statement, int tabs)
		{
			StringBuilder bob = new StringBuilder();
			ApplyIndentation(bob, tabs);
			foreach (Expression lhs in statement.Lhss)
			{
				bob.Append(GenerateString(lhs));
				bob.Append(", ");
			}

			bob.Remove(bob.Length - 2, 2);
			bob.Append(" " + statement.Tok.val + " ");
			bob.Append(GenerateString(statement.Expr) + statement.EndTok.val);
			return bob.ToString();
		}

		public string GenerateString(CalcStmt statement, int tabs)
		{
			StringBuilder bob = new StringBuilder();
			ApplyIndentation(bob, tabs);
			bob.AppendLine(statement.Tok.val);
			ApplyIndentation(bob, tabs);
			bob.AppendLine("{");
			//ApplyIndentation(bob, tabs + 1);

			int numOfSteps = statement.Steps.Count;

			// Print all steps and hints.
			for (int i = 0; i < numOfSteps; i++)
			{
				if (i > 0)
					bob.AppendLine();
				ApplyIndentation(bob, tabs + 1);
				bob.AppendLine(GenerateString(statement.Lines[i]) + ";");

				ApplyIndentation(bob, tabs);
				if (i < numOfSteps - 1)
					bob.Append(GenerateString(statement.StepOps[i]));

				BlockStmt currHint = statement.Hints[i];
				if (currHint.Body != null && currHint.Body.Count > 0)
				{
					bob.Append(" ");
					bob.Append(GenerateString(currHint, 0, true));
				}
			}

			ApplyIndentation(bob, tabs - 1);
			bob.AppendLine("}");
			return bob.ToString();
			//return string.Empty;
		}

		public string GenerateString(MatchStmt statement, int tabs)
		{
			var bob = new StringBuilder();
			ApplyIndentation(bob, tabs);

			bob.AppendLine(statement.Tok.val + " " + GenerateString(statement.Source) + " {");
			foreach (MatchCase matchCase in statement.Cases)
			{
				ApplyIndentation(bob, tabs + 1);
				bob.Append("case " + matchCase.Ctor.Name);
				if (matchCase.Arguments.Any())
				{
					bob.Append("(");
					foreach (BoundVar arg in matchCase.Arguments)
					{
						bob.Append(arg.Name + ", ");
					}

					bob.Remove(bob.Length - 2, 2);
					bob.Append(")");
				}
				bob.Append(" =>");

				if (matchCase is MatchCaseStmt)
				{
					bob.AppendLine();
					MatchCaseStmt mcs = matchCase as MatchCaseStmt;
					foreach (Statement sub in mcs.Body)
						bob.AppendLine(GenerateString(sub, tabs + 2));
				}
				else if (matchCase is MatchCaseExpr)
				{
					MatchCaseExpr mcs = matchCase as MatchCaseExpr;
					bob.Append(GenerateString(mcs.Body));
				}
			}
			ApplyIndentation(bob, tabs);
			bob.Append("}");

			return bob.ToString();
		}

		public string GenerateString(Expression expression)
		{
			if (expression is ApplySuffix)
			{
				return GenerateString(expression as ApplySuffix);
			}
			else if (expression is IdentifierExpr)
			{
				return GenerateString(expression as IdentifierExpr);
			}
			else if (expression is NameSegment)
			{
				return GenerateString(expression as NameSegment);
			}
			else if (expression is ExprDotName)
			{
				return GenerateString(expression as ExprDotName);
			}
			else if (expression is LiteralExpr)
			{
				return GenerateString(expression as LiteralExpr);
			}
			else if (expression is BinaryExpr)
			{
				return GenerateString(expression as BinaryExpr);
			}
			else if (expression is ParensExpression)
			{
				return GenerateString(expression as ParensExpression);
			}
			else if (expression is AutoGhostIdentifierExpr)
			{
				return GenerateString(expression as AutoGhostIdentifierExpr);
			}
			else if (expression is ChainingExpression)
			{
				return GenerateString(expression as ChainingExpression);
			}
			else if (expression is FunctionCallExpr)
			{
				return GenerateString(expression as FunctionCallExpr);
			}
			else if (expression is SeqSelectExpr)
			{
				return GenerateString(expression as SeqSelectExpr);
			}
			else if (expression is SeqDisplayExpr)
			{
				return GenerateString(expression as SeqDisplayExpr);
			}
			else if (expression is UnaryOpExpr)
			{
				return GenerateString(expression as UnaryOpExpr);
			}
			else if (expression is MemberSelectExpr)
			{
				return GenerateString(expression as MemberSelectExpr);
			}
			else if (expression is ForallExpr)
			{
				return GenerateString(expression as ForallExpr);
			}
			else if (expression is NegationExpression)
			{
				return GenerateString(expression as NegationExpression);
			}
			else if (expression is ITEExpr)
			{
				return GenerateString(expression as ITEExpr);
			}
			else if (expression is SetDisplayExpr)
			{
				return GenerateString(expression as SetDisplayExpr);
			}
			else if (expression is SetComprehension)
			{
				return GenerateString(expression as SetComprehension);
			}
			else if (expression is MultiSetDisplayExpr)
			{
				return GenerateString(expression as MultiSetDisplayExpr);
			}
			else if (expression is MultiSetFormingExpr)
			{
				return GenerateString(expression as MultiSetFormingExpr);
			}
			else if (expression is Microsoft.Dafny.OldExpr)
			{
				return GenerateString(expression as Microsoft.Dafny.OldExpr);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public string GenerateString(ApplySuffix expression)
		{
			var left = GenerateString(expression.Lhs);
			var self = expression.tok.val;
			var rightNames = expression.Args.Select(a => GenerateString(a));
			var right = String.Join(", ", rightNames);
			var close = ")";

			return left + self + right + close;
		}

		public string GenerateString(IdentifierExpr expression)
		{
			return expression.tok.val;
		}

		public string GenerateString(NameSegment expression)
		{
			return expression.tok.val;
		}

		public string GenerateString(ExprDotName expression)
		{
			var left = GenerateString(expression.Lhs);
			var dot = ".";
			var self = expression.tok.val;

			return left + dot + self;
		}

		public string GenerateString(LiteralExpr expression)
		{
			return expression.tok.val;
		}

		public string GenerateString(BinaryExpr expression)
		{
			string left = string.Empty;// = expression.E0.tok.val;
			string right = string.Empty; //expression.E1.tok.val;

			if (expression.E0.tok.val == "(" && !(expression.E0 is ApplySuffix) && !(expression.E0 is MultiSetFormingExpr))
				left = "(" + GenerateString(expression.E0.SubExpressions.First()) + ")";
			else
				left = GenerateString(expression.E0);

			if (expression.E1.tok.val == "(" && !(expression.E1 is ApplySuffix) && !(expression.E1 is MultiSetFormingExpr))
				right = "(" + GenerateString(expression.E1.SubExpressions.First()) + ")";
			else
				right = GenerateString(expression.E1);

			string op = string.Empty;
			op = GenerateString(expression.Op);

			// For some reason, in case of "<==" operator,
			// E0 and E1 switch "sides".
			if (expression.Op == BinaryExpr.Opcode.Exp)
			{
				return right + " " + op + " " + left;
			}
			else
			{
				return left + " " + op + " " + right;
			}
		}

		public string GenerateString(ParensExpression expression)
		{
			return "(" + GenerateString(expression.E) + ")";
		}

		public string GenerateString(AutoGhostIdentifierExpr expression)
		{
			return expression.Name;
		}

		public string GenerateString(ChainingExpression expression)
		{
			var bob = new StringBuilder();

			int i;
			for (i = 0; i < expression.Operators.Count; i++)
			{
				var operand = GenerateString(expression.Operands[i]);
				var op = GenerateString(expression.Operators[i]);
				bob.Append(operand + " " + op + " ");
			}
			var last = GenerateString(expression.Operands[i]);
			bob.Append(last);

			return bob.ToString();
		}

		public string GenerateString(FunctionCallExpr expression)
		{
			var self = expression.tok.val;
			var open = expression.OpenParen.val;
			var rightNames = expression.Args.Select(a => GenerateString(a));
			var right = String.Join(", ", rightNames);
			var close = ")";

			return self + open + right + close;
		}

		public string GenerateString(SeqSelectExpr expression)
		{
			var bob = new StringBuilder();
			bob.Append(GenerateString(expression.Seq));
			bob.Append("[");
			if (expression.SelectOne)
			{
				bob.Append(GenerateString(expression.E0));
			}
			else
			{
				if (expression.E0 != null)
					bob.Append(GenerateString(expression.E0));
				bob.Append("..");
				if (expression.E1 != null)
					bob.Append(GenerateString(expression.E1));
			}
			bob.Append("]");
			return bob.ToString();
		}

		public string GenerateString(SeqDisplayExpr expression)
		{
			var bob = new StringBuilder();
			bob.Append("[");
			foreach (Expression element in expression.Elements)
			{
				bob.Append(GenerateString(element));
				bob.Append(", ");
			}

			if (expression.Elements.Count > 0)
				bob.Remove(bob.Length - 2, 2);
			bob.Append("]");
			return bob.ToString();
		}

		public string GenerateString(UnaryOpExpr expression)
		{
			string result = string.Empty;
			switch (expression.Op)
			{
				case UnaryOpExpr.Opcode.Cardinality:
					{
						result = "|" + GenerateString(expression.E) + "|";
						break;
					}
				case UnaryOpExpr.Opcode.Not:
					{
						result = "!" + GenerateString(expression.E);
						break;
					}
			}

			return result;
		}

		public string GenerateString(MemberSelectExpr expression)
		{
			return expression.Obj.tok.val + "." + expression.MemberName;
		}

		public string GenerateString(ForallExpr expression)
		{
			StringBuilder bob = new StringBuilder();

			bob.Append(expression.tok.val);
			bob.Append(" ");

			var boundVars = expression.BoundVars.Select(v => v.tok.val);
			bob.Append(String.Join(", ", boundVars));

			bob.Append(" :: ");

			bob.Append(GenerateString(expression.Term));

			return bob.ToString();
		}

		public string GenerateString(NegationExpression expression)
		{
			return expression.tok.val + GenerateString(expression.E);
		}

		public string GenerateString(ITEExpr expression)
		{
			// TODO: Implement correctly. Currently it is tailored to the form "n - i" to support our demo.
			return expression.Thn.tok.val + " - " + expression.Test.tok.val;
		}

		public string GenerateString(SetDisplayExpr expression)
		{
			var bob = new StringBuilder();
			bob.Append("{");
			foreach (Expression element in expression.Elements)
			{
				bob.Append(GenerateString(element));
				bob.Append(", ");
			}

			if (expression.Elements.Count > 0)
				bob.Remove(bob.Length - 2, 2);
			bob.Append("}");
			return bob.ToString();
		}

		public string GenerateString(SetComprehension expression)
		{
			var bob = new StringBuilder();
			bob.Append(expression.tok.val + " "); // appends "set "
			//string boundName = expression.BoundVars.First().Name; // Assume for now that only a single bound variable exists.
			//bob.Append(boundName + " | " + GenerateString(expression.Range)); 

			BoundVar boundVar = expression.BoundVars.First();
			bob.Append(boundVar.Name + ": " + boundVar.Type.ToString() + " | " + GenerateString(expression.Range));
			// Not all set comprehension expressions contain a "Term" section.
			// For example: set x | x in {0,1,2,3,4,5} && x < 3;
			if (expression.Term.tok.val != "set")
			{
				bob.Append(" :: ");
				bob.Append(GenerateString(expression.Term));
			}

			return bob.ToString();
		}

		public string GenerateString(MultiSetDisplayExpr expression)
		{
			var bob = new StringBuilder();
			bob.Append(expression.tok.val + "{");
			foreach (Expression element in expression.Elements)
			{
				bob.Append(GenerateString(element));
				bob.Append(", ");
			}

			if (expression.Elements.Count > 0)
				bob.Remove(bob.Length - 2, 2);
			bob.Append("}");
			return bob.ToString();
		}

		public string GenerateString(MultiSetFormingExpr expression)
		{
			return "multiset(" + GenerateString(expression.E) + ")";
		}

		public string GenerateString(OldExpr expression)
		{
			return expression.tok.val + "(" + GenerateString(expression.E) + ")";
		}

		public string GenerateConditionString(AssertStmt assert, IDictionary<string, string> rename)
		{
			return GenerateConditionString(assert.Expr, rename);
		}

		public string GenerateConditionString(ComprehensionExpr.BoundedPool bound, IDictionary<string, string> rename, string boundVar)
		{
			if (bound is ComprehensionExpr.IntBoundedPool)
			{
				return GenerateConditionString(bound as ComprehensionExpr.IntBoundedPool, rename, boundVar);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public string GenerateConditionString(ComprehensionExpr.IntBoundedPool bound, IDictionary<string, string> rename, string boundVar)
		{
			var left = GenerateConditionString(bound.LowerBound, rename);
			var right = GenerateConditionString(bound.UpperBound, rename);

			// Super-duper hard-coded.
			// Is there even a way to determine whether we're using "<" or "<="???
			return left + " <= " + boundVar + " < " + right;
		}

		public string GenerateConditionString(Expression expression, IDictionary<string, string> rename)
		{
			if (expression is NameSegment)
			{
				return GenerateConditionString(expression as NameSegment, rename);
			}
			else if (expression is LiteralExpr)
			{
				return GenerateConditionString(expression as LiteralExpr, rename);
			}
			else if (expression is ChainingExpression)
			{
				return GenerateConditionString(expression as ChainingExpression, rename);
			}
			else if (expression is BinaryExpr)
			{
				return GenerateConditionString(expression as BinaryExpr, rename);
			}
			else if (expression is ApplySuffix)
			{
				return GenerateConditionString(expression as ApplySuffix, rename);
			}
			else if (expression is ForallExpr)
			{
				return GenerateConditionString(expression as ForallExpr, rename);
			}
			else if (expression is ExprDotName)
			{
				return GenerateConditionString(expression as ExprDotName, rename);
			}
			else if (expression is MemberSelectExpr)
			{
				return GenerateConditionString(expression as MemberSelectExpr, rename);
			}
			else if (expression is IdentifierExpr)
			{
				return GenerateConditionString(expression as IdentifierExpr, rename);
			}
			else if (expression is SeqSelectExpr)
			{
				return GenerateConditionString(expression as SeqSelectExpr, rename);
			}
			else if (expression is SeqDisplayExpr)
			{
				return GenerateConditionString(expression as SeqDisplayExpr, rename);
			}
			else if (expression is ParensExpression)
			{
				return GenerateConditionString(expression as ParensExpression, rename);
			}
			else if (expression is SetDisplayExpr)
			{
				return GenerateConditionString(expression as SetDisplayExpr, rename);
			}
			else if (expression is SetComprehension)
			{
				return GenerateConditionString(expression as SetComprehension, rename);
			}
			else if (expression is UnaryOpExpr)
			{
				return GenerateConditionString(expression as UnaryOpExpr, rename);
			}
			else if (expression is MultiSetFormingExpr)
			{
				return GenerateConditionString(expression as MultiSetFormingExpr, rename);
			}
			else if (expression is Microsoft.Dafny.OldExpr)
			{
				return GenerateConditionString(expression as Microsoft.Dafny.OldExpr, rename);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public string GenerateConditionString(NameSegment expression, IDictionary<string, string> rename)
		{
			var isFormal = !expression.Type.IsArrowType;
			if (isFormal)
			{
				if (rename.ContainsKey(expression.tok.val))
				{
					return rename[expression.tok.val];
				}
				else
				{
					return expression.tok.val;
				}
			}
			else
			{
				return expression.tok.val;
			}
		}

		public string GenerateConditionString(LiteralExpr expression, IDictionary<string, string> rename)
		{
			return expression.tok.val;
		}

		public string GenerateConditionString(ChainingExpression expression, IDictionary<string, string> rename)
		{
			var bob = new StringBuilder();

			int i;
			for (i = 0; i < expression.Operators.Count; i++)
			{
				var operand = GenerateConditionString(expression.Operands[i], rename);
				var op = GenerateString(expression.Operators[i]);
				bob.Append(operand + " " + op + " ");
			}
			var last = GenerateConditionString(expression.Operands[i], rename);
			bob.Append(last);

			return bob.ToString();
		}

		public string GenerateConditionString(BinaryExpr expression, IDictionary<string, string> rename)
		{
			var left = GenerateConditionString(expression.E0, rename);
			var right = GenerateConditionString(expression.E1, rename);

			return left + " " + expression.tok.val + " " + right;
		}

		public string GenerateConditionString(ApplySuffix expression, IDictionary<string, string> rename)
		{
			var left = GenerateConditionString(expression.Lhs, rename);
			var self = expression.tok.val;
			var rightNames = expression.Args.Select(a => GenerateConditionString(a, rename));
			var right = String.Join(", ", rightNames);
			var close = ")";

			return left + self + right + close;
		}

		public string GenerateConditionString(ForallExpr expression, IDictionary<string, string> rename)
		{
			StringBuilder bob = new StringBuilder();

			bob.Append(expression.tok.val);
			bob.Append(" ");

			var boundVars = expression.BoundVars.Select(v => v.tok.val);
			bob.Append(String.Join(", ", boundVars));

			bob.Append(" :: ");

			bob.Append(GenerateConditionString(expression.Term, rename));

			return bob.ToString();
		}

		public string GenerateConditionString(ExprDotName expression, IDictionary<string, string> rename)
		{
			var left = GenerateConditionString(expression.Lhs, rename);
			var dot = ".";
			var self = expression.tok.val;

			return left + dot + self;
		}

		public string GenerateConditionString(MemberSelectExpr expression, IDictionary<string, string> rename)
		{
			var left = GenerateConditionString(expression.Obj, rename);
			var dot = ".";
			var member = expression.MemberName;

			return left + dot + member;
		}

		public string GenerateConditionString(IdentifierExpr expression, IDictionary<string, string> rename)
		{
			return expression.Var.DisplayName;
		}

		public string GenerateConditionString(SeqSelectExpr expression, IDictionary<string, string> rename)
		{
			var bob = new StringBuilder();
			bob.Append(GenerateConditionString(expression.Seq, rename));
			bob.Append("[");
			if (expression.SelectOne)
			{
				bob.Append(GenerateConditionString(expression.E0, rename));
			}
			else
			{
				if (expression.E0 != null)
					bob.Append(GenerateConditionString(expression.E0, rename));
				bob.Append("..");
				if (expression.E1 != null)
					bob.Append(GenerateConditionString(expression.E1, rename));
			}
			bob.Append("]");
			return bob.ToString();
		}

		public string GenerateConditionString(SeqDisplayExpr expression, IDictionary<string, string> rename)
		{
			var bob = new StringBuilder();
			bob.Append("[");
			foreach (Expression element in expression.Elements)
			{
				bob.Append(GenerateConditionString(element, rename));
				bob.Append(", ");
			}

			if (expression.Elements.Count > 0)
				bob.Remove(bob.Length - 2, 2);
			bob.Append("]");
			return bob.ToString();
		}

		public string GenerateConditionString(SetDisplayExpr expression, IDictionary<string, string> rename)
		{
			var bob = new StringBuilder();
			bob.Append("{");
			foreach (Expression element in expression.Elements)
			{
				bob.Append(GenerateConditionString(element, rename));
				bob.Append(", ");
			}

			if (expression.Elements.Count > 0)
				bob.Remove(bob.Length - 2, 2);
			bob.Append("}");
			return bob.ToString();
		}

		public string GenerateConditionString(SetComprehension expression, IDictionary<string, string> rename)
		{
			var bob = new StringBuilder();
			bob.Append(expression.tok.val + " "); // appends "set "
			string boundName = expression.BoundVars.First().Name; // Assume for now that only a single bound variable exists.
			bob.Append(boundName + " | " + GenerateConditionString(expression.Range, rename));

			// Not all set comprehension expressions contain a "Term" section.
			// For example: set x | x in {0,1,2,3,4,5} && x < 3;
			if (expression.Term.tok.val != "set")
			{
				bob.Append(" :: ");
				bob.Append(GenerateConditionString(expression.Term, rename));
			}

			return bob.ToString();
		}

		public string GenerateConditionString(UnaryOpExpr expression, IDictionary<string, string> rename)
		{
			string result = string.Empty;
			switch (expression.Op)
			{
				case UnaryOpExpr.Opcode.Cardinality:
					{
						result = "|" + GenerateConditionString(expression.E, rename) + "|";
						break;
					}
				case UnaryOpExpr.Opcode.Not:
					{
						result = "!" + GenerateConditionString(expression.E, rename);
						break;
					}
			}

			return result;
		}

		public string GenerateConditionString(MultiSetDisplayExpr expression, IDictionary<string, string> rename)
		{
			var bob = new StringBuilder();
			bob.Append(expression.tok.val + "{");
			foreach (Expression element in expression.Elements)
			{
				bob.Append(GenerateConditionString(element, rename));
				bob.Append(", ");
			}

			if (expression.Elements.Count > 0)
				bob.Remove(bob.Length - 2, 2);
			bob.Append("}");
			return bob.ToString();
		}

		public string GenerateConditionString(ParensExpression expression, IDictionary<string, string> rename)
		{
			var bob = new StringBuilder();
			bob.Append(expression.tok.val);
			bob.Append(GenerateConditionString(expression.E, rename));
			bob.Append(")");
			return bob.ToString();
		}

		public string GenerateConditionString(MultiSetFormingExpr expression, IDictionary<string, string> rename)
		{
			return "multiset(" + GenerateConditionString(expression.E, rename) + ")";
		}

		public string GenerateConditionString(OldExpr expression, IDictionary<string, string> rename)
		{
			return expression.tok.val + "(" + GenerateConditionString(expression.E, rename) + ")";
		}

		public string GenerateString(BinaryExpr.Opcode opcode)
		{
			switch (opcode)
			{
				case BinaryExpr.Opcode.Ge:
					return ">=";
				case BinaryExpr.Opcode.Gt:
					return ">";
				case BinaryExpr.Opcode.Le:
					return "<=";
				case BinaryExpr.Opcode.Lt:
					return "<";
				case BinaryExpr.Opcode.Eq:
					return "==";
				case BinaryExpr.Opcode.Neq:
					return "!=";
				case BinaryExpr.Opcode.Add:
					return "+";
				case BinaryExpr.Opcode.Sub:
					return "-";
				case BinaryExpr.Opcode.Mul:
					return "*";
				case BinaryExpr.Opcode.Div:
					return "/";
				case BinaryExpr.Opcode.Mod:
					return "%";
				case BinaryExpr.Opcode.And:
					return "&&";
				case BinaryExpr.Opcode.Or:
					return "||";
				case BinaryExpr.Opcode.Imp:
					return "==>";
				case BinaryExpr.Opcode.Exp:
					return "<==";
				case BinaryExpr.Opcode.In:
					return "in";
				case BinaryExpr.Opcode.NotIn:
					return "!in";
				case BinaryExpr.Opcode.Disjoint:
					return "!!";
				default:
					throw new NotImplementedException();
			}
		}

		public string GenerateString(CalcStmt.CalcOp calcOp)
		{
			return calcOp.ToString();
		}

		private void ApplyIndentation(StringBuilder sb, int tabs)
		{
			if (tabs <= 0)
				return;

			for (int i = 1; i <= tabs; i++)
			{
				sb.Append("\t");
			}
		}
	}
}
