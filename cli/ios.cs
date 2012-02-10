/*	$Id: ios.cs,v 1.1.1.1 2004/06/03 08:59:35 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;
using System.IO;
using System.Text;
using Compiler;
using System.Reflection;

namespace CliCompiler {
	class CmdIo: Io {
		public new int MAXBUF = 512;
		public new int MAXSTR = 512;

		private FileStream ifile;
		private StreamReader rfile;
		
		private FileStream lst_ofile;
		private StreamWriter lst_wfile;

		private static String ifilename;
		private static String ofilename;
		private static String lst_ofilename;
		private static String Classname;

		public bool gendebug = false;
		public bool genlist = false;
		public bool gentoken = false;
		public static bool genexe = true;
		public static bool gendll = false;
		public static string genpath = ".";

		private String[] args;
		private char[] ibuf;

		private int ibufidx;
		private int ibufread;
		private bool _eof;
	
		private char look;			
		private StringBuilder buf;	
		int bufline = 0;			
		int bufcolumn = 0;			
		int curline = 0;			


		public override string GetClassname() { return Classname; }
		public override String GetInputFilename() {	return ifilename; }
		public override bool getGenList() { return genlist;	}                                                             
		public override bool getGenDebug() { return gendebug; }

		public override void ReadChar()	{
  			if (_eof)	
    			return;
  			if (ibuf == null || ibufidx >= MAXBUF)
    		{
    			ibuf = new char[MAXBUF];
    			_eof = false;
    			ibufread = rfile.Read(ibuf, 0, MAXBUF);
    			ibufidx = 0;
    			if (buf == null)
      				buf = new StringBuilder(MAXSTR);
    		}
			look = ibuf[ibufidx++];
	  		if (ibufread < MAXBUF && ibufidx > ibufread)
    		_eof = true;

			buf.Append(look);
			bufcolumn++;
  			if (look == '\n') { bufline++; bufcolumn=0; }
  		}

		public override char getNextChar() { return look; }

		public override void Abort(string s) {
  			StringBuilder sb = new StringBuilder();
  			sb.Append(ifilename);
  			sb.Append("(");
  			sb.Append(bufline+1);
			sb.Append(",");
			sb.Append(bufcolumn-1);
			sb.Append("): error ");
  			sb.Append(s);
  			Console.WriteLine(sb.ToString());
  			throw new ApplicationException("Aborting compilation");
  		}

		public override void Message(string s) {
			StringBuilder sb = new StringBuilder();
			sb.Append(s);
			if ((gentoken) || (String.Compare(s, 0, "token{", 0, 6, true) != 0))
				Console.WriteLine(sb.ToString());
		}

		public override void ICE(string s) {
  			StringBuilder sb = new StringBuilder();
  			sb.Append(ifilename);
  			sb.Append("(ICE): error PL0001: Internal Compiler Error: ");
  			sb.Append(s);
  			Console.WriteLine(sb.ToString());
  			throw new ApplicationException("Aborting compilation");
  		}

		void ParseArgs()
  		{
  			int i = 1;

  			if (args.Length < 2) {
    			Message("plil [/list] [/token] [/debug] [/dll] [/exe] [/outdir:path] filename.plm\n");
                Environment.Exit(1);
    		}

  			while (true) {
    			if (args[i][0] != '/') break;
    			if (args[i].Equals("/?"))
      			{
      				Message("plil [/list] [/token] [/debug] [/dll] [/exe] [/outdir:path] filename.plm\n");
      				Environment.Exit(1);
      			}
    			if (args[i].Equals("/exe"))
      			{
      				genexe = true;
      				gendll = false;
      				i++;
      				continue;
      			}
    			if (args[i].Equals("/list"))
      			{
      				genlist = true;
      				i++;
      				continue;
      			}
    			if (args[i].Equals("/debug"))
      			{
      				gendebug = true;
      				i++;
      				continue;
      			}
    			if (args[i].Equals("/token"))
      			{
      				gentoken = true;
      				i++;
      				continue;
      			}
    			if (args[i].Length > 8 && args[i].Substring(0,8).Equals("/outdir:"))
      			{
      				genpath = args[i].Substring(8);
      				i++;
      				continue;
      			}

    			Abort("Unmatched switch = '"+args[i]+"'\nArguments are:\nlpanet [/debug] [/nodebug] [/list] [/dll] [/exe] [/outdir:path] filename.plm\n");
    		}

  			if (args.Length-i != 1)
    		{
    			Message("plil [/list] [/token] [/debug] [/dll] [/exe] [/outdir:path] filename.plm\n");
                Environment.Exit(1);
    		}
  			
			ifilename = args[args.Length-1];
  		}

		public CmdIo()
  		{
		}

		public CmdIo(String[] a)
  		{
  			int i;

  			args = a;
 			ParseArgs();

			ifile = new FileStream(ifilename, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
  			if (ifile == null)
    		{
    			Abort("Could not open file '"+ifilename+"'\n");
    		}
  			rfile = new StreamReader(ifile); 

			i = ifilename.LastIndexOf('.');
  			if (i < 0) Abort("Bad filename '"+ifilename+"'");
  			int j = ifilename.LastIndexOf('\\');
  			if (j < 0) j = 0; else j++;

  			Classname = ifilename.Substring(j,i-j);
			if (genexe) ofilename = Classname+".exe";
  			if (genlist) {
    			lst_ofilename = Classname+".il";
    			lst_ofile = new FileStream(lst_ofilename, FileMode.Create, FileAccess.Write, FileShare.Write, 8192);
    			if (lst_ofile == null) Abort("Could not open file '"+ofilename+"'\n");
    			lst_wfile = new StreamWriter(lst_ofile);
    		}
  		}

		public override void Out(String s)
  		{
  			lst_wfile.Write(s);
  			lst_wfile.Flush();
  		}

		public override void Finish()
  		{
  			rfile.Close();
  			ifile.Close();
  			if (genlist) {
    			lst_wfile.Close();
    			lst_ofile.Close();
    		}
  		}

		public override bool EOF()
  		{
  			return _eof;
  		}

		public int commentGetCurrentLine()
  		{
  			return curline+1;
  		}

		public override string GetOutputFilename()
  		{
  			return ofilename;
  		}

		public override void TreeDraw(VarList tree)
		{
		}

	}
}
