/*	$Id: var.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

namespace Compiler {
	using System;
	using System.Text;
	using System.Collections;
	using System.Reflection;

	public class Var {
		public const int VAR_UNDEF = 0;
		public const int VAR_LOCAL = 1;
		public const int VAR_PARAM = 2;
		public const int VAR_BLOCK = 4;
		public const int VAR_LABEL = 8;
		public const int VAR_CONST = 16;

		public const int VAR_LAST  = VAR_CONST;

		private Object fieldbuilder;
		private Object methodbuilder;
		private Object localtoken;
		private int vclass;
		private int sign;


		public String[] VAR_Names = 
		{
			"(UND)",
			"(LOC)",
			"(PAR)",
			"(BLK)",
			"(LAB)",
			"(CNS)"
		};

		public int type;
		public String name;
		public Object val;
		public int granularity = 0;
		public int size;
		public int index = 0;
		public int type_id = 0;
		public VarList nodes;

		public int getType() { return type; }

		public String getTypeString() {
			if (type > VAR_LABEL)
				return "(LOC)";
			else
				return VAR_Names[type];
		}

		public void setType(int t) { type = t; }
		public String getName() { return name; }
		public void setName(String s) { name = s; }

		public void genName()
		{
			name = "NAME" + index;
			index++;
		}

		public VarList getNodes(int Type)
		{
			VarList val = new VarList();
			int i;
			for (i=0;i<nodes.Length();i++) 
			{	
				if ((nodes.FindByIndex(i).type & Type) != 0)
					val.add(nodes.FindByIndex(i));
			}
			return val;	
		}

		public VarList getParams() 
		{
			return getNodes(VAR_PARAM);
		}

		public VarList getLocals() 
		{
			return getNodes(VAR_LOCAL);
		}

		public VarList getVisible() 
		{
			return getNodes(VAR_LOCAL|VAR_PARAM);
		}

		
		public void setNodes(VarList p) { 
			nodes = p; 
		}

		public void addNodes(VarList p) { 
			int i;
			for (i=0;i<p.Length();i++) {
				nodes.add(p.FindByIndex(i));
			}
		}

		public void setNodesType(int p) 
		{ 
			int i;
			for (i=0;i<nodes.Length();i++) 
			{
				nodes.FindByIndex(i).setType(p);
			}
		}
		public void add(Var p) { nodes.add(p); 	}
		public int getIndex() { return index; }
		public void setIndex(int i) { index = i; }
		public void setGranularity(int i) { granularity = i; }
		public int getGranularity() { return granularity; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(name);
			// TODO
			return sb.ToString();
		} 

		public int getTypeId() { return type_id; }
		public void setTypeId(int t) { type_id = t; }
		public int getClassId() {
			if (type == VAR_PARAM)
				return Tok.T_PARAM;
			else
				return Tok.T_AUTO;
		}
		public void setClassId(int v) { vclass = v; }
		public int getSign() { return sign; }
		public void setSign(int i) { sign = i; }
		public Object getFieldBuilder() { return fieldbuilder; }
		public void setFieldBuilder(Object f) { fieldbuilder = f; }
		public Object getMethodBuilder() { return methodbuilder; }
		public void setMethodBuilder(Object f) { methodbuilder = f; }
		public Object getLocalToken() { return localtoken; }
		public void setLocalToken(Object f) { localtoken = f; }


    }
}
