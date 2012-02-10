/*	$Id: emt.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;
using System.Reflection;

namespace Compiler {

	class Emit {

		IAsm iroot;			
		IAsm icur;			
		VarList localvars;	
		Io io;
		Lib lib;
		Exe exe;			

		void NextInsn(int incr) {
			int ncount = 0;
			if (iroot == null) {
				icur = iroot = new IAsm();
			} else {
				ncount = icur.getICount() + incr;
				icur.setNext(new IAsm());
				icur = icur.getNext();
		    }
			icur.setICount(ncount);
		}
        
		public void FieldDef(Var e) {
			NextInsn(0);
			icur.setIType(IAsm.I_FIELD);
			icur.setVar(e);
		}

		public void FuncBegin(Var e) {
			NextInsn(0);
			icur.setIType(IAsm.I_FUNC_BEGIN);
			icur.setVar(e);
		}

		public void LocalVars(VarList v) {
			NextInsn(0);
			localvars = v;
			icur.setIType(IAsm.I_LOCALDEF);
		}

		public void Insn(String s) {
			NextInsn(1);
			icur.setIType(IAsm.I_INSN);
			icur.setInsn(s);
		}
        
		public void Label(String lname) {
			NextInsn(0);
			icur.setIType(IAsm.I_LABEL);
			icur.setLabel(lname);
		}
        
		public void Branch(String s, String lname) {
			NextInsn(1);
			icur.setIType(IAsm.I_BRANCH);
			icur.setInsn(s);
			icur.setLabel(lname);
		}
        
		public void Store(Var e) {
			NextInsn(1);
			icur.setIType(IAsm.I_INSN_STORE);
			icur.setVar(e);
		}
        
		public void Load(Var e) {
			NextInsn(1);
			icur.setIType(IAsm.I_INSN_LOAD);
			icur.setVar(e);
		}
        
		public void LoadConst(String s) {
			NextInsn(1);
			icur.setIType(IAsm.I_INSN_LOAD_CONST);
			icur.setInsn(s);
		}
        
		public void Call(Var e) {
			NextInsn(1);
			icur.setIType(IAsm.I_CALL);
			icur.setVar(e);
		}
        
		public void Finish() {
			iroot = icur = null;
		}
        
		public void CommentHolder() { // ???
			NextInsn(0);
			icur.setIType(IAsm.I_COMMENT);
		}
        
		public void Ret() {
			NextInsn(1);
			icur.setIType(IAsm.I_RET);
		}
        
		public void FuncEnd() {
			NextInsn(0);
			icur.setIType(IAsm.I_FUNC_END);
		}
        
		public void IL() {
			IAsm a = iroot;
			IAsm p;
			while (a != null) {
				switch (a.getIType()) {
				case IAsm.I_INSN:
					exe.Insn(a);
					break;
				case IAsm.I_LABEL:
					exe.Label(a);
					break;
				case IAsm.I_BRANCH:
					exe.Branch(a);
					break;
				case IAsm.I_INSN_STORE:
					exe.Store(a);
					break;
				case IAsm.I_INSN_LOAD:
					exe.Load(a);
					break;
				case IAsm.I_INSN_LOAD_CONST:
					exe.LoadConst(a);
					break;
				case IAsm.I_FUNC_BEGIN:
					exe.FuncBegin(a);
					break;
				case IAsm.I_FUNC_END:
					exe.FuncEnd();
					break;
				case IAsm.I_CALL:
					exe.Call(a);
					break;
				case IAsm.I_RET:
					exe.Ret(a);
					break;
				case IAsm.I_FIELD:
					exe.FieldDef(a);
					break;
				case IAsm.I_LOCALDEF:
					exe.LocalVars(localvars);
					break;
				case IAsm.I_COMMENT:
					break;
				default:
					io.Abort("PL0301: unhandled instruction type " + a.getIType());
					break;
				}
				p = a;
				a = a.getNext();
			}
		}
        
       
		public void LIST() {
			IAsm a = iroot;
			IAsm p;
			Asm x = new Asm(io, lib);
			while (a != null) {
				switch (a.getIType())
				{
				case IAsm.I_INSN:
					x.Insn(a);
					break;
				case IAsm.I_LABEL:
					x.Label(a);
					break;
				case IAsm.I_BRANCH:
					x.Branch(a);
					break;
				case IAsm.I_INSN_STORE:
					x.Store(a);
					break;
				case IAsm.I_INSN_LOAD:
					x.Load(a);
					break;
				case IAsm.I_INSN_LOAD_CONST:
					x.LoadConst(a);
					break;
				case IAsm.I_FUNC_BEGIN:
					x.FuncBegin(a); 
					break;
				case IAsm.I_FUNC_END:
					x.FuncEnd();
					break;
				case IAsm.I_CALL:
					x.Call(a);
					break;
				case IAsm.I_RET:
					x.Ret(a);
					break;
				case IAsm.I_COMMENT:
					break;
				case IAsm.I_FIELD:
					x.FieldDef(a);
					break;
				case IAsm.I_LOCALDEF:
					x.LocalVars(localvars);
					break;
				default:
					io.Abort("PL0302: unhandled instruction type " + a.getIType());
					break;
				}
				p = a;
				a = a.getNext();
			}
		}
        
		public void BeginModule() {
			exe.BeginModule(io.GetInputFilename() );
		}
        
		public void BeginClass() {
			exe.BeginClass(io.GetClassname(), TypeAttributes.Public);
			if (io.getGenList()) {
				io.Out(".assembly '"+io.GetClassname()+"'\r\n");
				io.Out("{\r\n");
				io.Out("\t.ver 0:0:0:0\r\n");
				io.Out("}\r\n\r\n");
				io.Out(".class " + io.GetClassname() + "{\r\n");
			}
		}
        
		public void EndClass() {
			exe.EndClass();
			if (io.getGenList()) io.Out("}\r\n");
		}
        
		public void EndModule() {
			exe.EndModule();
		}
        
		public Emit(Io o, Lib l) {
			io = o;
			lib = l;
			exe = new Exe(io.GetClassname(), io, l);
		}
	}
}
