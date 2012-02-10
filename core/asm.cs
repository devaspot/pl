/*	$Id: asm.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;
using System.Text;

namespace Compiler {

	class Asm {

		private Io io;
		private Lib lib;

		public Asm(Io ihandle, Lib l) {
			io = ihandle;
			lib = l;
		}
  
		private String ilSType(int type) {
			switch (type) {
			case Tok.TOK_FIXED:		return "int32";
			case Tok.TOK_FLOAT:		return "float32";
			case Tok.TOK_COMPLEX:	return "float64";
			case Tok.TOK_REAL:		return "float32";
			case Tok.TOK_BINARY:	return "int16";
			case Tok.TOK_DECIMAL:	return "int32";
			case Tok.TOK_VOID:		return "void";
			default:
				io.Abort("PL0401: unhandled type " + type);
				break;
			}
			return null;
		}

		private String genDataTypeSig(Var e) {
			if (e == null) return null;
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			if (e.getSign() == Tok.T_UNSIGNED) sb.Append("unsigned ");
			sb.Append(ilSType(e.getTypeId()));
			return (sb.ToString());
		}

		private string genFieldRef(Var e) {
			if (e == null) return null;
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			if (e.getSign() == Tok.T_UNSIGNED) sb.Append("unsigned ");
			sb.Append(ilSType(e.getTypeId()));
			sb.Append(" ");
			sb.Append("Class" + io.GetClassname());	
			sb.Append(".");
			sb.Append(e.getName());	
			return (sb.ToString());
		}

		public void Load(IAsm a) {
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			Var e = a.getVar();
			if (e == null) {
				io.Abort("PL0402: load instruction with no variable ptr");
			}
			switch (e.getClassId()) {
			case Tok.T_STATIC:
				sb.Append("\tldsfld ");
				sb.Append(genFieldRef(e));
				sb.Append("\t\t\t\t\t//");
				sb.Append(a.getICount());
				sb.Append(", ");
				sb.Append(e.getName());
				sb.Append("\r\n");
				break;
			case Tok.T_AUTO:
			case Tok.T_DEFCLASS:
				sb.Append("\tldloc ");
				sb.Append(e.getIndex());
				sb.Append("\t\t\t\t\t//");
				sb.Append(a.getICount());
				sb.Append(", ");
				sb.Append(e.getName());
				sb.Append("\r\n");
				break;
			case Tok.T_PARAM:
				sb.Append("\tldarg ");
				sb.Append(e.getIndex());
				sb.Append("\t\t\t\t\t//");
				sb.Append(a.getICount());
				sb.Append(", ");
				sb.Append(e.getName());
				sb.Append("\r\n");
				break;
			default:
				io.Abort("PL0403: instruction load of unknown class (" + e.getClassId() + ")");
				break;
			}
			io.Out(sb.ToString());
		}

		public void Store(IAsm a) {
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			Var e = a.getVar();
			if (e == null) {
				io.Abort("PL0404: store instruction with no variable ptr");
			}
			switch (e.getClassId()) {
			case Tok.T_STATIC:
				sb.Append("\tstsfld ");
				sb.Append(genFieldRef(e));
				sb.Append("\t\t\t\t\t//");
				sb.Append(a.getICount());
				sb.Append(", ");
				sb.Append(e.getName());
				sb.Append("\r\n");
				break;
			case Tok.T_AUTO:
			case Tok.T_DEFCLASS:
				sb.Append("\tstloc ");
				sb.Append(e.getIndex());
				sb.Append("\t\t\t\t\t//");
				sb.Append(a.getICount());
				sb.Append(", ");
				sb.Append(e.getName());
				sb.Append("\r\n");
				break;
			case Tok.T_PARAM:
				sb.Append("\tstarg ");
				sb.Append(e.getIndex());
				sb.Append("\t\t\t\t\t//");
				sb.Append(a.getICount());
				sb.Append(", ");
				sb.Append(e.getName());
				sb.Append("\r\n");
				break;
			default:
				io.Abort("PL0405: instruction load of unknown class (" + e.getClassId() + ")");
				break;
			}
			io.Out(sb.ToString());
		}

		public void FuncBegin(IAsm a) {
			Var func = a.getVar();
			String funcsig = genDataTypeSig(a.getVar());

			VarList x = func.getParams();
			String paramsig = "";
			
			if (x.Length() > 0) {
				int max = x.Length();
				StringBuilder t = new StringBuilder(Io.MAXSTR);
				for (int i = 0; i < max; i++) {
					Var e = x.FindByIndex(i);
					t.Append(genDataTypeSig(e));
					if (i < max-1) t.Append(",");
				}
				paramsig = t.ToString();
			}
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			sb.Append("\t.method public ");
			sb.Append("static ");
			sb.Append(funcsig);
			sb.Append(" ");
			sb.Append(func.getName());
			sb.Append("(");
			sb.Append(paramsig);
			sb.Append(") {\r\n");
			io.Out(sb.ToString());

			if (func.getName().ToLower().Equals("main")) 
				io.Out("\t.entrypoint\r\n");
		}
  
		public void Call(IAsm a) {
            Var func = a.getVar();
            LibFunc lfunc;

			String funcsig = genDataTypeSig(a.getVar()); 

			VarList x = func.getParams(); 
			String paramsig = "";
			if (x.Length() > 0) {
				int max = x.Length();
				StringBuilder t = new StringBuilder(Io.MAXSTR);
				for (int i = 0; i < max; i++) {
					Var e = x.FindByIndex(i);
					t.Append(genDataTypeSig(e));
					if (i < max-1) t.Append(",");
				}
				paramsig = t.ToString();
			}

			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			sb.Append("\tcall ");
			sb.Append(funcsig);
			sb.Append(" ");

			lfunc = lib.lookup_func(a.getVar().getName());
			if (lfunc != null) {
				sb.Append(lfunc.nameFull);
			} else {
				sb.Append(io.GetClassname());
				sb.Append("::");
				sb.Append(func.getName());
				sb.Append("(");
				sb.Append(paramsig);
				sb.Append(")");
			}

			sb.Append("\t\t\t\t\t//");
			sb.Append(a.getICount());
			sb.Append("\r\n");
			io.Out(sb.ToString());
		}

		public void Insn(IAsm a) {
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			sb.Append("\t");
			sb.Append(a.getInsn());
			sb.Append("\t\t\t\t\t//");
			sb.Append(a.getICount());
			sb.Append("\r\n");
			io.Out(sb.ToString());
		}

		public void Label(IAsm a) {
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			sb.Append(a.getLabel());
			sb.Append(":\r\n");
			io.Out(sb.ToString());
		}

		public void Branch(IAsm a) {
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			sb.Append("\t");
			sb.Append(a.getInsn());
			sb.Append(" ");
			sb.Append(a.getLabel());
			sb.Append("\t\t\t\t\t//");
			sb.Append(a.getICount());
			sb.Append("\r\n");
			io.Out(sb.ToString());
		}

		public void Ret(IAsm a) {
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			sb.Append("\tret\t\t\t\t\t//");
			sb.Append(a.getICount());
			sb.Append("\r\n");
			io.Out(sb.ToString());
		}

		public void FuncEnd() {
			io.Out("\t}\r\n");
		}

		public void LocalVars(VarList v) {
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			sb.Append("\t.locals (");
			int max = v.Length();

			for (int i = 0; i < max; i++) {
				Var e = v.FindByIndex(i);
				String stype = "";
				switch (e.getTypeId()) {
				case Tok.TOK_FIXED:		stype = "int32"; break;
				case Tok.TOK_FLOAT:		stype = "float32"; break;
				case Tok.TOK_COMPLEX:	stype = "float64"; break;
				case Tok.TOK_REAL:		stype = "float32"; break;
				case Tok.TOK_BINARY:	stype = "int16"; break;
				case Tok.TOK_DECIMAL:	stype = "int32"; break;
				default:
					io.Abort("PL0406: could not find type for local");
					break;
				}
				sb.Append(stype);
				if (i < max-1) sb.Append(",");
			}

			sb.Append(")\r\n");
            io.Out(sb.ToString());
		}

		public void FieldDef(IAsm a) {
			Var e = a.getVar();		
			String prefix = "";
			switch (e.getClassId()) {
			case Tok.T_STATIC:
				prefix = "\t.field ";
				break;
			case Tok.T_AUTO:
			case Tok.T_DEFCLASS:
				prefix = "\t.field ";
				break;
			default:
				io.Abort("PL0407: unhandled field def type");
			break;
			}

			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			sb.Append(prefix);	
			sb.Append(genDataTypeSig(e)); 
			sb.Append(" ");
			sb.Append(e.getName());	
			sb.Append("\r\n");
			io.Out(sb.ToString());
		}

		public void LoadConst(IAsm a) {
			StringBuilder sb = new StringBuilder(Io.MAXSTR);
			int value = Convert.ToInt32(a.getInsn());

			sb.Append("\tldc.i4");
			if (value > 127 || value < -128) {
				sb.Append(" ");
			} else if (value > 8 || value < -1) {
				sb.Append(".s ");
			} else {
				sb.Append(".");
			}
			sb.Append(a.getInsn());
			sb.Append("\t\t\t\t\t//");
			sb.Append(a.getICount());
			sb.Append("\r\n");

		    io.Out(sb.ToString());
		}
	}
}
