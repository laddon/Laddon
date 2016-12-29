using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

namespace DafnyAnalyzer
{
	public class MethodFinder
	{
		/// <summary>
		/// Gets the offset (from file start) of method's end.
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public int GetEndOffset(Method method)
		{
			return method.BodyEndTok.pos + 1; // add one to go to next char
		}

		public IEnumerable<Statement> GetMethodStatements(Method method)
		{
			return method.Body.SubStatements;
		}

		public IEnumerable<Statement> GetStatements(string filename, int startPos, int endPos)
		{
			var method = FindMethod(filename, startPos);
			//TODO: make sure both offsets are within same method!
			var allStatements = GetMethodStatements(method);
			var statements = allStatements
				.Where(s => s.Tok.pos <= startPos
				&& s.EndTok.pos >= endPos);

			return statements;
		}

		/// <summary>
		/// Finds method which given offset resides in.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public Method FindMethod(string filename, int offset)
		{
			// This was commented since it was causing verification to crash. (Boogie is likely to be the source)
			// DafnyOptions.Install(new DafnyOptions());

			var program = GetProgram(filename);
			var decls = program.Modules.SelectMany(m => m.TopLevelDecls).ToList();
			var callables = ModuleDefinition.AllCallables(decls);

			var method = callables.Where(c => c is Method
				&& ((Method)c).BodyStartTok.pos <= offset
				&& ((Method)c).BodyEndTok.pos >= offset);

			if (method.Any())
			{
				return method.First() as Method;
			}
			else
			{
				return null;
			}
		}

		private Program GetProgram(string filename)
		{
			var files = new List<string> { filename };
			Program ret;
			Main.ParseCheck(files, filename, out ret);
			return ret;
		}
	}
}
