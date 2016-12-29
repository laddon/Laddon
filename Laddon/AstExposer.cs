using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using DafnyServiceProvider;

namespace MTA.Laddon
{
	/// <summary>
	/// This class will not be used for now.
	/// At the moment, Laddon will reparse the Dafny program 
	/// instead of fetching the parsed program from DafnyExtension.
	/// </summary>
	public class AstExposer
	{
		//private IDafnyService dafnyService;

		//public AstExposer(IDafnyService service)
		//{
		//	dafnyService = service;
		//}

		///// <summary>
		///// Gets the offset (from file start) of method's end.
		///// </summary>
		///// <param name="method"></param>
		///// <returns></returns>
		//public int GetEndOffset(Method method)
		//{
		//	return method.BodyEndTok.pos + 1; // add one to go to next char
		//}

		//public IEnumerable<Statement> GetMethodStatements(Method method)
		//{
		//	return method.Body.SubStatements;
		//}

		//public IEnumerable<Statement> GetStatements(string filename, int startPos, int endPos)
		//{
		//	var method = FindMethod(filename, startPos);
		//	//TODO: make sure both offsets are within same method!
		//	var allStatements = GetMethodStatements(method);
		//	var statements = allStatements
		//		.Where(s => s.Tok.pos <= startPos
		//		&& s.EndTok.pos >= endPos);

		//	return statements;
		//}

		///// <summary>
		///// Finds method which given offset resides in.
		///// </summary>
		///// <param name="filename"></param>
		///// <param name="offset"></param>
		///// <returns></returns>
		//public Method FindMethod(string filename, int offset)
		//{
		//	DafnyOptions.Install(new DafnyOptions());

		//	//var program = GetProgram(filename);
		//	var program = dafnyService.GetResolvedProgram();
		//	var decls = program.Modules.SelectMany(m => m.TopLevelDecls).ToList();
		//	var callables = ModuleDefinition.AllCallables(decls);

		//	var method = callables.Where(c => c is Method
		//		&& ((Method)c).BodyStartTok.pos <= offset
		//		&& ((Method)c).BodyEndTok.pos >= offset).First() as Method;

		//	return method;
		//}

		//public static Program GetProgram(string filename)
		//{
		//	var files = new List<string> { filename };
		//	Program ret;
		//	Main.ParseCheck(files, filename, out ret);
		//	return ret;
		//}
	}
}
