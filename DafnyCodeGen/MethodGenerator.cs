using Common;
using DafnyCodeGen.Enums;
using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyCodeGen
{
	public class MethodGenerator : CodeGeneratorBase<Method>
	{
		/// <summary>
		/// Generates the code for a new Dafny method from the supplied information about the method.
		/// </summary>
		/// <param name="methodType">Type of method to generate</param>
		/// <param name="name">Name of the new method</param>
		/// <param name="ins">Input arguments of the new method, with their original names</param>
		/// <param name="outs">Output arguments of the new method, with their original names</param>
		/// <param name="inputRenames">Mapping of original names to new names for the input arguments</param>
		/// <param name="outputRenames">Mapping of original names to new names for the output arguments</param>
		/// <param name="duplicates">List of original names of arguments that are both input and output</param>
		/// <param name="body">Body of the new method</param>
		/// <returns>A string representing a complete and valid Dafny code fragment representing the new method</returns>
		public string Generate(MethodType methodType,
							   string name,
							   DafnyVariable[] ins,
							   DafnyVariable[] outs, 
							   Dictionary<string, string> inputRenames, 
							   Dictionary<string, string> outputRenames,
							   IEnumerable<string> duplicates,
							   IEnumerable<string> preconditions,
							   IEnumerable<string> postConditions,
							   bool makeWithoutBody,
							   string body)
		{
			StringBuilder b = new StringBuilder();

			// Prepate method signature line.
			string typeName;
			switch (methodType)
			{
				case MethodType.Method:
					typeName = "method";
					break;
				case MethodType.Lemma:
					typeName = "lemma";
					break;
				default:
					throw new NotImplementedException();
			}
			string signature = string.Format("\r\n\r\n{0} {1}(", typeName, name);
			foreach (DafnyVariable input in ins)
			{
				if (input.IsGhost)
				{
					signature += "ghost ";
				}

				signature += string.Format("{0}: {1}, ", input.Name, input.Type.ToString());
			}

			if (ins.Any())
				signature = signature.Remove(signature.Length - 2, 2); // Remove tailing ","

			signature += ")";
			
			if (outs.Any())
			{
				signature += " returns (";

				foreach (DafnyVariable output in outs)
				{
					if (output.IsGhost)
					{
						signature += "ghost ";
					}

					signature += string.Format("{0}: {1}, ", output.Name, output.Type.ToString());
				}

				signature = signature.Remove(signature.Length - 2, 2); // Remove tailing ", "

				signature += ")";
			}
			b.AppendLine(signature);

			// Add conditions
			foreach (var pre in preconditions)
			{
				b.AppendLine("\t" + pre);
			}
			foreach (var post in postConditions)
			{
				b.AppendLine("\t" + post);
			}

			if (methodType == MethodType.Method)
			{
				// Add "modifies"
				IEnumerable<DafnyVariable> argsOnHeap = ins.Where(x => x.IsOnHeap);
				if (argsOnHeap.Any())
				{
					b.Append("\tmodifies ");
					foreach (DafnyVariable argOnHeap in argsOnHeap)
					{
						b.Append(argOnHeap.Name + ", ");
					}

					b.Remove(b.Length - 2, 2);
					b.AppendLine();
				}
			}

			if (makeWithoutBody)
			{
				TrimEnd(b); // remove last line break
				b.Append(";");
			}
			else
			{
				b.AppendLine("{");

				// Prepare method body
				if (duplicates.Any())
				{
					b.Append("\t");
					foreach (string dup in duplicates)
					{
						b.Append(outputRenames[dup] + ", ");
					}
					b.Remove(b.Length - 2, 2);

					b.Append(" := ");

					foreach (string dup in duplicates)
					{
						b.Append(inputRenames[dup] + ", ");
					}
					b.Remove(b.Length - 2, 2);
					b.AppendLine(";");
				}

				//foreach (string dup in duplicates)
				//{
				//	b.AppendLine("\t" + outputRenames[dup] + " := " + inputRenames[dup] + ";");
				//}

				if (!String.IsNullOrEmpty(body))
				{
					//b.AppendLine(body);
					b.Append(body);
				}

				b.Append("}");
			}

			b.Append("\r\n");
			return b.ToString();
		}

		public override string Generate(Method dafnyObject)
		{
			var b = new StringBuilder();

			b.AppendLine(GenerateHeader(dafnyObject));
			//b.AppendLine("ensures 1 == 1;");
			b.AppendLine("{");
			b.AppendLine(GenerateBody(dafnyObject));
			b.AppendLine("}\r\n");

			return b.ToString();
		}

		private string GenerateHeader(Method method)
		{
			var b = new StringBuilder();

			//b.Append("\n");
			b.AppendLine();
			b.Append("method ");
			b.Append(method.Name);
			b.Append(GenerateIns(method));
			b.Append(" ");
			b.Append(GenerateOuts(method));

			return b.ToString();
		}

		private string GenerateIns(Method method)
		{
			var b = new StringBuilder();

			var ins = new List<string>();
			foreach (var i in method.Ins)
			{
				ins.Add(i.Name + ": " + i.Type.ToString());
			}

			b.Append("(");
			if (ins.Count() > 0)
			{
				b.Append(String.Join(", ", ins));
			}
			b.Append(")");

			return b.ToString();
		}

		private string GenerateOuts(Method method)
		{
			var b = new StringBuilder();

			var outs = new List<string>();
			foreach (var o in method.Outs)
			{
				outs.Add(o.Name + ": " + o.Type.ToString());
			}

			if (outs.Count() > 0)
			{
				b.Append("returns (");
				b.Append(String.Join(", ", outs));
				b.Append(")");
			}

			return b.ToString();
		}

		private string GenerateBody(Method method)
		{
			var b = new StringBuilder();

			foreach (var s in method.Body.SubStatements)
			{
			}

			return b.ToString();
		}

		private void TrimEnd(StringBuilder b)
		{
			if (b != null && b.Length > 0)
			{
				int i = b.Length - 1;
				for (; i >= 0; i--)
				{
					if (!char.IsWhiteSpace(b[i]))
						break;
				}

				if (i < b.Length - 1)
					b.Length = i + 1;
			}
		}
	}
}
