using Common;
//using Dafny;
using DafnyAnalyzer;
using DafnyCodeGen;
using DafnyCodeGen.Enums;
using DafnyRefactoringEngine;
//using Microsoft.Boogie;
using Microsoft.Dafny;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MTA.Laddon
{
	/// <summary>
	/// Interaction logic for AddMethod.xaml
	/// </summary>
	public abstract partial class AddMethodWindow : Window
	{
		protected IWpfTextView textView;
		protected Method CurrentMethod;

		protected List<DafnyVariable> Ins = new List<DafnyVariable>();
		protected List<DafnyVariable> Outs = new List<DafnyVariable>();
		protected IEnumerable<string> Duplicates = new List<string>();
		protected List<VarDeclStmt> DeclerationStatementsToPrintAsUpdates;
		protected IEnumerable<Statement> SelectedStatements;
		protected Statement SelectionScope;
		protected List<TextBox> InputTextboxes;
		protected List<TextBox> OutputTextboxes;
		protected Dictionary<string, string> InputNamesMapping;
		protected Dictionary<string, string> OutputNamesMapping;

		public abstract MethodType MethodType { get; }

		public bool IsValid { get; set; }
		
		public AddMethodWindow(IWpfTextView view)
		{
			InitializeComponent();
			textView = view;
			IsValid = true;

			GetCurrentMethod();
			if (CurrentMethod == null)
			{
				AlertUser("Selection must be within method.");
				IsValid = false;
				return;
			}

			DeclerationStatementsToPrintAsUpdates = new List<VarDeclStmt>();
			SelectedStatements = GetSelectedStatements();

			var result = CanExtractMethod();
			if (result.IsValid)
			{
				DetermineInsAndOuts();
				AddVariableNameChoosers();
			}
			else
			{
				AlertUser(result.Message);
				IsValid = false;
			}
		}

		private void GetCurrentMethod()
		{
			var fileName = GetFileName();
			var caretPosition = textView.Caret.Position.BufferPosition.Position;

			var finder = new MethodFinder();
			CurrentMethod = finder.FindMethod(fileName, caretPosition);
		}

		protected abstract void DetermineInsAndOuts();

		protected void DetermineIns()
		{
			Ins = StatementAnalyzer.GetInVariables(SelectedStatements).ToList();
			Ins.AddRange(StatementAnalyzer.GetInVariablesForConditions(SelectionScope, SelectedStatements));

			AnalyzeArgumentUsageInCode(ref Ins);
		}

		protected void DetermineInsFromAsserts()
		{
			var offset = textView.Selection.Start.Position.Position;
			Ins = StatementAnalyzer.GetInVariablesForConditions(SelectionScope, offset).ToList();
		}

		protected void AnalyzeArgumentUsageInCode(ref List<DafnyVariable> argumentList)
		{
			List<DafnyVariable> argListCopy = new List<DafnyVariable>(argumentList);

			// Analyze variable usage.
			foreach (DafnyVariable arg in argListCopy)
			{
				VarDeclStmt declerationStatement;
				bool isDeclaredInside = VariableUsageAnalyzer.IsDeclaredInStatements(arg.Name, SelectedStatements, out declerationStatement);
				bool isUsedAfter = VariableUsageAnalyzer.IsUsedAfterStatement(arg.Name, CurrentMethod, SelectedStatements.Last().EndTok.pos);

				// If the variable is declared inside the new (extracted) method and never used outside that method,
				// it should not be an output argument. It will be completely contained within the extracted method.
				if (isDeclaredInside && !isUsedAfter)
					argumentList.Remove(arg);

				// If the variable is declared in the selected statements but also used after them,
				// it should be declared outisde the extracted method - before the call to it.
				if (isDeclaredInside && isUsedAfter && declerationStatement != null)
					DeclerationStatementsToPrintAsUpdates.Add(declerationStatement);

				arg.IsDeclaredInSelectedStatements = isDeclaredInside;
				arg.IsUsedAfterSelectedStatements = isUsedAfter;

				
				// Check if variable is placed on the heap and is modified 
				// (and therefore should appear in a "modifies" clause.
				if (arg.Type.AsArrayType != null)
				{
					IEnumerable<UpdateStmt> updates = SelectedStatements.OfType<UpdateStmt>();
					foreach (UpdateStmt us in updates)
					{
						foreach (SeqSelectExpr lhs in us.Lhss.OfType<SeqSelectExpr>())
						{
							if (lhs.Seq.tok.val == arg.Name)
							{
								arg.IsOnHeap = true;
							}
						}
					}
				}
			}
		}

		private void AddVariableNameChoosers()
		{
			InputTextboxes = new List<TextBox>();
			OutputTextboxes = new List<TextBox>();

			if (Ins.Count() > 0)
			{
				lblIn.Visibility = System.Windows.Visibility.Visible;
				//TODO: toggle lblIn visible
			}
			else
			{
				lblIn.Visibility = System.Windows.Visibility.Hidden;
			}
			foreach (var inVar in Ins)
			{
				var textBox = new TextBox();
				textBox.Tag = inVar.Name; // Store original variable as textbox's tag for later reference
				textBox.Margin = new Thickness(20, 5, 0, 0);
				
				// Special name for input representation of a double-purpose (both input and output) variable.
				if (Duplicates.Contains(inVar.Name))
				{
					var digit = 0;
					var inVars = Ins.Select(i => i.Name);
					var bodyVars = VariableFinder.GetAllVariables(SelectedStatements).Select(s => s.Name);
					while (inVars.Contains(inVar.Name + digit) || bodyVars.Contains(inVar.Name + digit))
					{
						digit++;
					}
					textBox.Text = inVar.Name + digit;
				}
				else
					textBox.Text = inVar.Name;

				textBox.IsEnabled = false; // for now we do not allow renaming
				pnlIns.Children.Add(textBox);
				InputTextboxes.Add(textBox);
			}

			if (Outs.Count() > 0)
			{
				lblOut.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				lblOut.Visibility = System.Windows.Visibility.Hidden;
			}
			foreach (var outVar in Outs)
			{
				var textBox = new TextBox();
				textBox.Tag = outVar.Name; // Store original variable as textbox's tag for later reference
				textBox.Margin = new Thickness(20, 5, 0, 0);
				textBox.Text = outVar.Name;
				textBox.IsEnabled = false; // for now we do not allow renaming
				pnlOuts.Children.Add(textBox);
				OutputTextboxes.Add(textBox);
			}
		}

		private void btnAddMethod_Click(object sender, RoutedEventArgs e)
		{
			// Gather original and new variable names.
			InputNamesMapping = new Dictionary<string, string>();
			OutputNamesMapping = new Dictionary<string, string>();

			// We treat all input and output arguments as if they were renamed for the new method.
			foreach (TextBox textBox in InputTextboxes)
				InputNamesMapping.Add((string)textBox.Tag, textBox.Text);
			foreach (TextBox textBox in OutputTextboxes)
				OutputNamesMapping.Add((string)textBox.Tag, textBox.Text);

			// Apply changes in the code file.
			InsertNewMethod();
			ReplaceSelectedCode();
		}

		private void InsertNewMethod()
		{
			string code = GenerateCode();

			// Place the new method implementation in the code.
			int position = GetMethodEnd();
			ITextEdit edit = textView.TextBuffer.CreateEdit();
			edit.Insert(position, code);
			edit.Apply();
			this.Close();
		}

		protected string GeneratePartsFromSelectedStatements()
		{
			var gen = new MethodGenerator();
			string methodName = txtMethodName.Text;

			StringBuilder methodBodyBuilder = new StringBuilder();

			//TODO: Generate text from NewStatements
			//var stmtGen = new StatementGenerator();
			var stmtGen = new StatementGenerator(DeclerationStatementsToPrintAsUpdates);

			// Generate preconditions
			var requires = StatementAnalyzer.DeterminePreConditionsFromPrecedingAsserts(SelectionScope,
				SelectedStatements, Ins, InputNamesMapping);

			// Generate postconditions
			var ensures = StatementAnalyzer.DeterminePostConditionsFromFollowingAsserts(SelectionScope,
				SelectedStatements, Ins, Outs, OutputNamesMapping).ToList();

			// Rename output variables in statements to the user specified names.
			foreach (string varName in OutputNamesMapping.Keys)
				VariableRenamer.RenameVariable(varName, OutputNamesMapping[varName], SelectedStatements);

			// Rename input variables in statements to the user specified names.
			// UNNEEDED as long as the user cannot rename via the GUI.
			// Later the duplicate variable special case needs to be handled.
			//foreach (string varName in InputNamesMapping.Keys)
			//	VariableRenamer.RenameVariable(varName, InputNamesMapping[varName], SelectedStatements);

			// Generate new method's body.
			//	First we check special case where entire selection is empty block statement,
			//	in which case we will generate a method with empty body.
			if (SelectedStatements.Count() == 1
				&& SelectedStatements.First() is BlockStmt
				&& !SelectedStatements.First().SubStatements.Any())
			{
				// do nothing.
			}
			else
			{
				foreach (Statement stmt in SelectedStatements)
					methodBodyBuilder.AppendLine(stmtGen.GenerateString(stmt, 1));
			}

			return gen.Generate(MethodType, methodName, CreateInputArray(), CreateOutputArray(),
				InputNamesMapping, OutputNamesMapping, Duplicates,
				requires, ensures,
				false,
				methodBodyBuilder.ToString());
		}

		protected string GenerateEmptyBodyParts()
		{
			var gen = new MethodGenerator();
			string methodName = txtMethodName.Text;

			var offset = textView.Selection.Start.Position.Position;

			// Generate preconditions
			var requires = StatementAnalyzer.DeterminePreConditionsFromPrecedingAsserts(SelectionScope,
				offset, Ins, InputNamesMapping);

			// Generate postconditions
			var ensures = StatementAnalyzer.DeterminePostConditionsFromFollowingAsserts(SelectionScope,
				offset, Ins, Outs, OutputNamesMapping).ToList();

			return gen.Generate(MethodType, methodName, CreateInputArray(), CreateOutputArray(),
				InputNamesMapping, OutputNamesMapping, Duplicates,
				requires, ensures,
				true,
				string.Empty);
		}

		private void ReplaceSelectedCode()
		{
			string methodName = txtMethodName.Text;

			var gen = new MethodCallGenerator();
			var methodCall = gen.Generate(Ins, Outs, methodName);

			ITextEdit edit = textView.TextBuffer.CreateEdit();

			if (SelectedStatements.Any())
			{
				var start = GetStatementStart(SelectedStatements.First());
				var end = SelectedStatements.Last().EndTok.pos + 1;
				var length = end - start;

				edit.Replace(start, length, methodCall);
			}
			else
			{
				var offset = textView.Selection.Start.Position.Position;
				edit.Insert(offset, methodCall);
			}
			
			edit.Apply();
			this.Close();
		}

		// TODO: this method needs serious work.
		private int GetStatementStart(Statement statement)
		{
			if (statement is UpdateStmt)
			{
				var update = (UpdateStmt)statement;
				if (update.Lhss.Any())
				{
					if (update.Lhss[0] is SeqSelectExpr)
						return ((SeqSelectExpr)update.Lhss[0]).Seq.tok.pos;
					return update.Lhss[0].tok.pos;
				}
				else
				{
					if (update.Rhss.Any())
					{
						if (update.Rhss[0] is ExprRhs)
						{
							var exprRhs = update.Rhss[0] as ExprRhs;
							if (exprRhs.Expr is ApplySuffix)
							{
								var suf = exprRhs.Expr as ApplySuffix;
								return suf.Lhs.tok.pos;
							}
							else
							{
								throw new NotImplementedException();
							}
						}
						else
						{
							throw new NotImplementedException();
						}
					}
					else
					{
						return update.Tok.pos;
					}
				}
			}
			else if (statement is AssignSuchThatStmt)
			{
				var assignSuchThat = ((AssignSuchThatStmt)statement);
				return assignSuchThat.Lhss[0].tok.pos;
			}
			else if (statement is AssignStmt)
			{
				var assign = (AssignStmt)statement;
				return assign.Lhs.tok.pos;
			}
			else if (statement is CallStmt)
			{
				var call = (CallStmt)statement;
				if (call.Lhs.Any())
					return call.Lhs[0].tok.pos;
				else
					return call.Receiver.tok.pos;
			}
			else
			{
				return statement.Tok.pos;
			}
		}

		private DafnyVariable[] CreateInputArray()
		{
			var inVars = new List<DafnyVariable>();
			for (int i = 0; i < Ins.Count(); i++)
			{
				var newName = ((TextBox)pnlIns.Children[i]).Text;
				//inVars.Add(new Microsoft.Dafny.Formal(Ins[i].Tok, newName, Ins[i].Type, true, false));
				inVars.Add(new DafnyVariable(newName, Ins[i].Type, Ins[i].IsGhost) { IsOnHeap = Ins[i].IsOnHeap });
			}

			return inVars.ToArray();
		}

		private DafnyVariable[] CreateOutputArray()
		{
			var outVars = new List<DafnyVariable>();
			for (int i = 0; i < Outs.Count(); i++)
			{
				var newName = ((TextBox)pnlOuts.Children[i]).Text;
				//outVars.Add(new Microsoft.Dafny.Formal(Outs[i].Tok, newName, Outs[i].Type, true, false));
				outVars.Add(new DafnyVariable(newName, Outs[i].Type, Outs[i].IsGhost));
			}

			return outVars.ToArray();
		}

		private int GetMethodEnd()
		{
			var finder = new MethodFinder();
			var end = finder.GetEndOffset(CurrentMethod);
			return end;
		}

		private string GetFileName()
		{
			ITextDocument doc;
			textView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out doc);
			return doc.FilePath;
		}

		private IEnumerable<Statement> GetSelectedStatements()
		{
			var fileName = GetFileName();
			var start = textView.Selection.Start.Position.Position;
			var end =  textView.Selection.End.Position.Position;
			var finder = new MethodFinder();

			// Get the highest-level statements in the method.
			var methodStatements = finder.GetMethodStatements(CurrentMethod);

			// Invoke the recursive method on the highest-level statements in the current method.
			SelectionScope = CurrentMethod.Body;
			List<Statement> selected = GetSelectedStatementsRecursive(methodStatements, start, end);
			return selected;
		}

		private bool IsStatementInRange(Statement s, int start, int end)
		{
			return ((s.Tok.pos <= start && s.EndTok.pos >= start) ||
					(s.Tok.pos <= end && s.EndTok.pos >= end) ||
					(s.Tok.pos >= start && s.EndTok.pos <= end) ||
					(s.Tok.pos <= start && s.EndTok.pos >= end));
		}

		private bool IsExpressionInRange(Microsoft.Dafny.Expression e, int start, int end)
		{
			return (e.tok.pos >= start && e.tok.pos <= end);
		}

		private List<Statement> GetSelectedStatementsRecursive(IEnumerable<Statement> statements, int start, int end)
		{
			List<Statement> selected = new List<Statement>();

			// Run over all the method's statements and retrieve the selected statements only.
			// Block statements (such as while, if, etc...) recieve special care.
			foreach (var s in statements)
			{
				if (s is WhileStmt)
				{
					WhileStmt w = s as WhileStmt;

					// If the while loop guard is grabbed by the selection, we consider the entire loop in.
					if (IsExpressionInRange(w.Guard, start, end))
						selected.Add(s);
					else
					{
						if (s.Tok.pos < start && end < s.EndTok.pos)
						{
							SelectionScope = s;
						}

						selected.AddRange 
						(
							GetSelectedStatementsRecursive(w.SubStatements.First().SubStatements, start, end)
						);
					}
				}
				else if (s is IfStmt)
				{
					// For now we can only grab an entire "if/else" clause, along with all "if else" clauses,
					// but not individual statements WITHIN any of the clauses.
					// TODO: Support individual statements within an "if", "if else" or "else" clause.
					IfStmt i = s as IfStmt;
					if (IsExpressionInRange(i.Guard, start, end))
						selected.Add(s);
					else
					{
						if (s.Tok.pos < start && end < s.EndTok.pos)
						{
							SelectionScope = s;
						}

						selected.AddRange 
						(
							GetSelectedStatementsRecursive(i.Thn.SubStatements, start, end)
						);
						
						if (i.Els is IfStmt) // i.Els represents "else if"
						{
							selected.AddRange
							(
								GetSelectedStatementsRecursive(new List<Statement> { i.Els }, start, end)
							);
						}
						else if (i.Els is BlockStmt) // i.Els represents "else"
						{
							selected.AddRange
							(
								GetSelectedStatementsRecursive(i.Els.SubStatements, start, end)
							);
						}
					}
				}
				else if (s is AlternativeStmt)
				{
					if (IsExpressionInRange(s.SubExpressions.First(), start, end))
						selected.Add(s);
					else
					{
						SelectionScope = s;

						foreach (Statement block in s.SubStatements)
						{
							selected.AddRange
							(
								GetSelectedStatementsRecursive(block.SubStatements, start, end)
							);
						}
					}
				}
				else if (s is BlockStmt)
				{
					if (IsStatementInRange(s, start, end))
					{
						if (s.Tok.pos < start && end < s.EndTok.pos)
						{
							SelectionScope = s;

							selected.AddRange
							(
								GetSelectedStatementsRecursive(s.SubStatements, start, end)
							);
						}
						else
						{
							selected.Add(s);
						}
					}
				}
				else
				{
					if (IsStatementInRange(s, start, end))
						selected.Add(s);
				}
			}

			return selected;
		}

		protected void AlertUser(string message)
		{
			MessageBox.Show(message);
		}

		protected abstract ValidationResult CanExtractMethod();

		protected abstract string GenerateCode();
	}
}
