/*	$Id: vma.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;
using System.Collections;
using System.Reflection;

namespace Compiler {

    public class IAsm {
		public const int I_INSN				= 101;
		public const int I_LABEL			= 102;
		public const int I_BRANCH	 		= 103;
		public const int I_CALL				= 104;
		public const int I_RET 				= 105;
		public const int I_INSN_STORE		= 111;
		public const int I_INSN_LOAD		= 112;
		public const int I_INSN_LOAD_CONST 	= 113;
		public const int I_COMMENT			= 120;
		public const int I_FUNC_BEGIN		= 150;
		public const int I_FUNC_END			= 151;
		public const int I_FIELD			= 161;
		public const int I_LOCALDEF			= 162;
		
		private IAsm next;
		private int icount;
		private int itype;		
		private String insn;	
		private String label;	
		private Var ivar;		
		
		public void setNext(IAsm n) { next = n; }
		public IAsm getNext() { return next; }
		public void setICount(int i) { icount = i; }
		public int getICount() { return icount; }
		public void setIType(int i) { itype = i; }
		public int getIType() { return itype; }
		public void setInsn(String s) { insn = s; }
		public String getInsn() { return insn; }
		public void setLabel(String l) { label = l; }
		public String getLabel() { return label; }
		public void setVar(Var v) { ivar = v; }
		public Var getVar() { return ivar; }
	}
		
}
