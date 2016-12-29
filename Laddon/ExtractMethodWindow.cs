using Common;
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
	public class ExtractMethodWindow : AddMethodWindow
	{
		public override MethodType MethodType
		{
			get { return MethodType.Method; }
		}

		public ExtractMethodWindow(IWpfTextView view)
			: base(view)
		{
			this.Title = "Extract method";
			lblMethodName.Text = "Method name";
		}

		protected override ValidationResult CanExtractMethod()
		{
			var bob = new StringBuilder();

			var res = new ValidationResult
			{
				IsValid = true
			};

			if (!SelectedStatements.Any())
			{
				res.IsValid = false;
				bob.AppendLine("* No statements selected");
				bob.AppendLine();
			}

			if (CurrentMethod.IsGhost)
			{
				res.IsValid = false;
				bob.AppendLine("* Cannot extract method from ghost method");
				bob.AppendLine();
			}

			res.Message = bob.ToString();

			return res;
		}

		protected override void DetermineInsAndOuts()
		{
			DetermineIns();

			Outs = StatementAnalyzer.GetOutVariables(SelectedStatements).ToList();
			AnalyzeArgumentUsageInCode(ref Outs);

			Duplicates = from x in Ins
						 where Outs.Any(y => x.Name == y.Name)
						 select x.Name;
		}

		protected override string GenerateCode()
		{
			return GeneratePartsFromSelectedStatements();
		}
	}
}
