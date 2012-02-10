/*	$Id: exe.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/


using System;
using System.Diagnostics.SymbolStore;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections;

namespace Compiler {

	public class Exe {

		AssemblyName	appname;
		string			filename;
		string			classname;
   		ISymbolDocumentWriter	srcdoc;

		AppDomain		appdomain;
		AssemblyBuilder	appbuild;
		ModuleBuilder	emodule;
		TypeBuilder		eclass;		// current class
		MethodBuilder	emethod;		// current method

		Hashtable		opcodehash;
		ILGenerator		il;
		Io				io;
		Lib				lib;
		VarList			localvars;

		public void initOpcodeHash() {

			opcodehash = new Hashtable(32); // default initial size
			opcodehash["neg"] = OpCodes.Neg;
			opcodehash["mul"] = OpCodes.Mul;
			opcodehash["div"] = OpCodes.Div;
			opcodehash["add"] = OpCodes.Add;
			opcodehash["sub"] = OpCodes.Sub;
			opcodehash["not"] = OpCodes.Not;
			opcodehash["and"] = OpCodes.And;
			opcodehash["or"] = OpCodes.Or;
			opcodehash["xor"] = OpCodes.Xor;
			opcodehash["pop"] = OpCodes.Pop;
			
			opcodehash["br"] = OpCodes.Br;
			opcodehash["beq"] = OpCodes.Beq;
			opcodehash["bge"] = OpCodes.Bge;
			opcodehash["ble"] = OpCodes.Ble;
			opcodehash["blt"] = OpCodes.Blt;
			opcodehash["bgt"] = OpCodes.Bgt;
			opcodehash["brtrue"] = OpCodes.Brtrue;
			opcodehash["brfalse"] = OpCodes.Brfalse;
			
			opcodehash["cgt"] = OpCodes.Cgt;
			opcodehash["clt"] = OpCodes.Clt;
			opcodehash["ceq"] = OpCodes.Ceq;
			
			opcodehash["ldc.i4.-1"] = OpCodes.Ldc_I4_M1;
			opcodehash["ldc.i4.0"] = OpCodes.Ldc_I4_0;
			opcodehash["ldc.i4.1"] = OpCodes.Ldc_I4_1;
			opcodehash["ldc.i4.2"] = OpCodes.Ldc_I4_2;
			opcodehash["ldc.i4.3"] = OpCodes.Ldc_I4_3;
			opcodehash["ldc.i4.4"] = OpCodes.Ldc_I4_4;
			opcodehash["ldc.i4.5"] = OpCodes.Ldc_I4_5;
			opcodehash["ldc.i4.6"] = OpCodes.Ldc_I4_6;
			opcodehash["ldc.i4.7"] = OpCodes.Ldc_I4_7;
			opcodehash["ldc.i4.8"] = OpCodes.Ldc_I4_8;
		}

		public Exe(string exename, Io io, Lib l) {
			this.io = io;
			this.lib = l;
			filename = exename;
			initOpcodeHash();
		}

		AssemblyName getAssemblyName(string s) {
			AssemblyName a = new AssemblyName();
			a.Name = filename + "_assembly";
			return a;
		}

		public void BeginModule(string ifile) {
			appdomain	= System.Threading.Thread.GetDomain();
			appname		= getAssemblyName(filename);
			appbuild	= appdomain.DefineDynamicAssembly(appname, AssemblyBuilderAccess.Save, ".");
			emodule		= appbuild.DefineDynamicModule(filename + "_module", io.GetOutputFilename(), io.getGenDebug());
			Guid g 		= System.Guid.Empty;
			if (io.getGenDebug()) srcdoc = emodule.DefineDocument(ifile, g, g, g);
		}

		public void EndModule() {
			try {
			    string s = io.GetOutputFilename();
				appbuild.Save(s);
				Console.WriteLine("saving assembly as " + s);
			}
			catch (Exception e) {
				io.ICE(e.ToString());
			}
		}

		public void BeginClass(String name, TypeAttributes access) {
			classname = name;
			eclass = emodule.DefineType(name, access);
		}

		public void EndClass() {
			eclass.CreateType();		// create the class
		}

		Type ilSType(bool sign, int type) {
			if (sign) {
				switch (type) {
				case Tok.TOK_FIXED:		return Type.GetType("System.Int32");
				case Tok.TOK_FLOAT:		return Type.GetType("System.Single");
				case Tok.TOK_COMPLEX:	return Type.GetType("System.Double");
				case Tok.TOK_REAL:		return Type.GetType("System.Single");
				case Tok.TOK_BINARY:	return Type.GetType("System.Int16");
				case Tok.TOK_DECIMAL:	return Type.GetType("System.Int32");
				case Tok.TOK_VOID:		return null;
				default:
					io.ICE("unhandled type " + type);
					return null;
				}
			} else {
				switch (type) {
				case Tok.TOK_FIXED:  	return Type.GetType("U4");
				case Tok.TOK_DECIMAL: 	return Type.GetType("U4");
				case Tok.TOK_DEFTYPE:	return Type.GetType("U4");
				case Tok.TOK_BINARY:	return Type.GetType("U2");
				case Tok.TOK_VOID:		return null;
				default:
					io.ICE("unhandled type " + type);
					return null;
				}
			}
		}

		/*
			common routine to construct a signature string for a given varlist item
			requires a destination ptr, will return the updated dest ptr
		*/

		private Type genDataTypeSig(Var e) {
			bool sign = true;
			if (e == null) return null;
			if (e.getSign() == Tok.T_UNSIGNED)	/* if var is unsigned, put it in sig */
				sign = false;
			Type sig = ilSType(sign, e.getTypeId());	/* get the datatype */
			return (sig);
		}

		void genLoad(Var e) {
			int id = e.getClassId();
			if (e == null) io.ICE("load instruction with no variable ptr");
			if (e.getLocalToken() != null) {

				// LocalToken lt = (LocalToken) e.getLocalToken();
				LocalBuilder lt = (LocalBuilder) e.getLocalToken();
				il.Emit(OpCodes.Ldloc, lt);

			} else {

				if (e.getFieldBuilder() != null) {

					FieldBuilder fb = (FieldBuilder) e.getFieldBuilder();
					if (id == Tok.T_STATIC) il.Emit(OpCodes.Ldsfld, fb);
					else il.Emit(OpCodes.Ldfld, fb);

				} else {

					int index = e.getIndex();
					if (id == Tok.T_PARAM) {

						if (index <= 256) il.Emit(OpCodes.Ldarg_S, index); else il.Emit(OpCodes.Ldarg, index);

					} else {

						if (id == Tok.T_AUTO || id == Tok.T_DEFCLASS) {
							if (index <= 256) il.Emit(OpCodes.Ldloc_S, e.getIndex());
							else il.Emit(OpCodes.Ldloc, e.getIndex());
						} else {
							io.ICE("instruction load of unknown class (" + e.getClassId()+")");
						}
					}
				}
			}
		}

		public void Load(IAsm a) {
			Var e = a.getVar();
			genLoad(e);
		}

		public void Store(IAsm a) {
			if (a.getVar() == null) io.ICE("store instruction with no variable ptr");
			Var e = localvars.FindByName(a.getVar().getName());
			if (e == null) e = a.getVar();
			int id = e.getClassId();
			if (e.getLocalToken() != null) {

				LocalBuilder lt = (LocalBuilder) e.getLocalToken();
				il.Emit(OpCodes.Stloc, lt);

			} else {

            	if (e.getFieldBuilder() != null) {

    				FieldBuilder fb = (FieldBuilder) e.getFieldBuilder();
					if (id == Tok.T_STATIC) il.Emit(OpCodes.Stsfld, fb);
					else il.Emit(OpCodes.Stfld, fb);

				} else {

					int index = e.getIndex();
					if (id == Tok.T_PARAM) {

						if (index <= 256) il.Emit(OpCodes.Starg_S, index);
						else il.Emit(OpCodes.Starg, index);
					} else {
					       
                		if (id == Tok.T_AUTO || id == Tok.T_DEFCLASS) il.Emit(OpCodes.Stloc, index);
						else io.ICE("instruction load of unknown class (" + e.getClassId()+")");
					}
				}
			}
		}

		public void FuncBegin(IAsm a) {
			Var func = a.getVar();
			Type funcsig = genDataTypeSig(a.getVar()); 

			VarList paramlist = func.getParams(); 
			Type[] paramTypes = null;

			if (paramlist.Length() > 0) {
				int max = paramlist.Length();
				paramTypes = new Type[max];
				for (int i = 0; i < max; i++) {
					Var e = paramlist.FindByIndex(i);
					paramTypes[i] = genDataTypeSig(e);
				}
			}

			emethod = eclass.DefineMethod(func.getName(),
				MethodAttributes.Static|MethodAttributes.Public,
				funcsig, paramTypes);
			func.setMethodBuilder(emethod);

            for (int i = 0; i < paramlist.Length(); i++)
				emethod.DefineParameter(i+1, 0, paramlist.FindByIndex(i).getName());

            il = emethod.GetILGenerator(); 

			if (func.getName().ToLower().Equals("main"))
				appbuild.SetEntryPoint(emethod);
    //    emodule.SetUserEntryPoint(emethod);

			labelhash = new Hashtable();

		}

		public void Call(IAsm a) {
			Var func = a.getVar();
			Object o = func.getMethodBuilder(); 

			LibFunc lfunc = lib.lookup_func(a.getVar().getName());

            if (lfunc != null) {
				il.Emit(OpCodes.Call, lfunc.methodInfo);
			} else {               
	            if (o == null) io.ICE("no previous extern for (" + func.getName() + ")");
    	        MethodBuilder mb = (MethodBuilder) o;
				il.Emit(OpCodes.Call, mb);
			}
		}

		public void Insn(IAsm a) {
			Object o = opcodehash[a.getInsn()];

			if (o == null) io.ICE("instruction opcode (" + a.getInsn() + ") not found in hash");
			il.Emit((OpCode) o);
		}

		private Hashtable labelhash;

		private Object getILLabel(IAsm a) {
			String s = a.getLabel();
			Object l = labelhash[s];

            if (l == null) {
				l = (Object) il.DefineLabel();
				labelhash[s] = l;
			}
			return l;
		}

		public void Label(IAsm a) {
			il.MarkLabel((Label) getILLabel(a));
		}

		public void Branch(IAsm a) {
			Object o = opcodehash[a.getInsn()];

			if (o == null) io.ICE("instruction branch opcode (" + a.getInsn() + ") not found in hash");

            il.Emit((OpCode) o, (Label) getILLabel(a));
		}

		public void Ret(IAsm a) {
			il.Emit(OpCodes.Ret);
		}

		public void FuncEnd() {
			il = null;
		}

		public void LocalVars(VarList v) {
			int max = v.Length();

			for (int i = 0; i < max; i++) {
				Var e = v.FindByIndex(i);
				Type et = genDataTypeSig(e);
			    LocalBuilder t = il.DeclareLocal(et);
				if (io.getGenDebug()) t.SetLocalSymInfo(e.getName());
				e.setLocalToken(t);
			}
			localvars = v;
		}

		public void FieldDef(IAsm a) {
			Var e = a.getVar();
			FieldAttributes attr = FieldAttributes.Private;

			if (e.getClassId() == Tok.T_STATIC)
				attr |= FieldAttributes.Static;

			Type t = genDataTypeSig(e);		/* gen type info */

			FieldBuilder f = eclass.DefineField(e.getName(), t, attr);
			e.setFieldBuilder((Object) f);
		}

		public void LoadConst(IAsm a) {
			int value = Convert.ToInt32(a.getInsn());

			if (value > 127 || value < -128) {
				il.Emit(OpCodes.Ldc_I4, value);
			} else if (value > 8 || value < -1) {
				il.Emit(OpCodes.Ldc_I4_S, Convert.ToSByte(value));
			} else if (value == -1) {
				il.Emit(OpCodes.Ldc_I4_M1);
			} else {
				Object o = opcodehash["ldc.i4."+a.getInsn()];
				if (o == null) io.ICE("could not find opcode for (ldc.i4." + a.getInsn() + ")");
				il.Emit((OpCode) o);
			}
		}

		private bool IsNextNonInsnGen(IAsm a) {
			IAsm cur = a.getNext();
			if (cur == null) return true;
	
    	    int type = cur.getIType();
    		
			while (type == IAsm.I_LABEL) {
				cur = cur.getNext();
				type = cur.getIType();
			}

			if (type == IAsm.I_COMMENT)	return true;

			return false;
		}

	}

}
