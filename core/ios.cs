/*	$Id: ios.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;
using System.Text;
using System.IO;

namespace Compiler {

	public abstract class Io {

		public static int MAXBUF = 512;
		public static int MAXSTR = 512;
		public virtual void ReadChar() { }
		public virtual char getNextChar() { return '0'; }
		public virtual void Abort(string s) { }
		public virtual void Message(string s) { }
		public virtual void ICE(string s) { } 
		public virtual void Out(string s) { }
		public virtual void Finish() { }
		public virtual bool EOF() { return false; }
		public virtual void TreeDraw(VarList tree) { }
		public virtual string GetClassname() { return null; }
		public virtual string GetInputFilename() { return null; }
		public virtual string GetOutputFilename() { return null; } 
		public virtual bool getGenList() { return false; }
		public virtual bool getGenDebug() { return false; }        
	}

}
