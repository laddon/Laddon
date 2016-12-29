using DafnyAnalyzer;
using DafnyCodeGen.Enums;
using Microsoft.Dafny;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTA.Laddon
{
	public class ExtractLemmaWindow : AddMethodWindow
	{
		public override MethodType MethodType
		{
			get { return MethodType.Lemma; }
		}

		public ExtractLemmaWindow(IWpfTextView view)
			: base(view)
		{
			this.Title = "Extract lemma";
			lblMethodName.Text = "Lemma name";
		}

		protected override ValidationResult CanExtractMethod()
		{
			var bob = new StringBuilder();

			var res = new ValidationResult
			{
				IsValid = true
			};

			if (!IsFollowedByAssert())
			{
				res.IsValid = false;
				bob.AppendLine("* There is nothing for lemma to ensure, as there is no assert following the selected statements");
				bob.AppendLine();
			}

			res.Message = bob.ToString();

			return res;
		}

		protected override void DetermineInsAndOuts()
		{
			if (SelectedStatements.Any())
			{
				DetermineIns();
			}
			else
			{
				DetermineInsFromAsserts();
			}
		}

		protected override string GenerateCode()
		{
			if (SelectedStatements.Any())
			{
				return GeneratePartsFromSelectedStatements();
			}
			else
			{
				return GenerateEmptyBodyParts();
			}
		}

		private bool IsFollowedByAssert()
		{
			if (SelectedStatements.Any())
			{
				return StatementAnalyzer.AreStatementsFollowedByAssert(SelectionScope, SelectedStatements);
			}
			else
			{
				var offset = textView.Selection.End.Position.Position;
				return StatementAnalyzer.AreStatementsFollowedByAssert(SelectionScope, offset);
			}
			
		}
	}
}
