/*	$Id: cli.cs,v 1.1.1.1 2004/06/03 08:59:35 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;
using Compiler;

namespace CliCompiler {

    public class Planet {

    	public static void Main() {
			String[] args = Environment.GetCommandLineArgs();
			CmdIo source = new CmdIo();
			try {
				source = new CmdIo(args);
				Tok tokenizer = new Tok(source);
				Lib funclibrary = new Lib();

				Parser parser = new Parser(source, tokenizer, funclibrary);
				parser.parse_and_compile();
				source.Finish();
            } catch {
				source.Message("plil: compiler error.");
			}
		}
	}
}
