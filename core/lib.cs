/*	$Id: lib.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;
using System.Text;
using System.Collections;
using System.Reflection.Emit;
using System.Reflection;

namespace Compiler {

	public class LibFunc {

    	public string		nameShort;
		public string		nameFull;
		public ArrayList	typeParams;
		public int			typeReturn;
		public MethodInfo	methodInfo;

		public LibFunc(string nS, string nF, ArrayList tP, int tR, MethodInfo mi) {
			nameShort  = nS;
			nameFull   = nF;
			typeParams = tP.Clone() as ArrayList;
			typeReturn = tR;
			methodInfo = mi;
		}
	}

	public class Lib {

        static Hashtable tokens;

        public void InitHash() {
			ArrayList	param;
			LibFunc		lfunc;
			MethodInfo	minfo;

            tokens = new Hashtable();
			param = new ArrayList();
			minfo = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(Int32) });

			param.Add(Tok.TOK_FIXED);
			lfunc = new LibFunc("PRINT_I", 
								"[mscorlib]System.Console::WriteLine(int32)",
								param,
								Tok.TOK_VOID,
								minfo
								);
            add_tok(lfunc,		"PRINT_I");
		}   

		public static void add_tok(LibFunc i, string s)	{ tokens.Add(s, i); }

		public LibFunc lookup_func(string s)
        {
            object k = tokens[s];
            return (LibFunc) k;
        }  

		public IDictionaryEnumerator get_enum() { return tokens.GetEnumerator(); }

        public int Length() { return tokens.Count; }

        public Lib() {
            InitHash();
        }
	}

}