/*	$Id: blk.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;
using System.Collections;

namespace Compiler {

	public class VarList
	{
		public Hashtable vhash;
		int vindex;
		int index = 0;

		public VarList()
		{
			vhash = new Hashtable(8);
			vindex = 0;	
		}

		public void add(Var e)
		{
			int index = vindex++;
			e.setIndex(index);
			vhash.Add(e.getName(), e);
			vhash.Add(index, e);
		}

		public void addNodes(VarList p) 
		{ 
			int i;
			for (i=0;i<p.Length();i++) 
			{
				add(p.FindByIndex(i));
			}
		}

		public void mergeNodes(VarList p) { 
			int i;
			for (i=0; i<p.Length(); i++) {
				Var varSrc = p.FindByIndex(i);
				Var varTrg = FindByName(varSrc.getName());
				if ((varTrg != null) && (varTrg.getType() != Var.VAR_PARAM))
					add(varSrc);
				else
				if (varTrg == null) {
					add(varSrc);
				} else {
					FindByName(varSrc.getName()).setTypeId(varSrc.getTypeId());
					FindByName(varSrc.getName()).setGranularity(varSrc.getGranularity());
				}				
			}
		}

		public void setNodesGranularity(int p) { 
			int i;
			for (i=0;i<Length();i++) {
				FindByIndex(i).setGranularity(p);
			}
		}

		public void setNodesType(int p) { 
			int i;
			for (i=0;i<Length();i++) {
				FindByIndex(i).setType(p);
			}
		}

		public void setNodesTypeId(int p) { 
			int i;
			for (i=0;i<Length();i++) {
				FindByIndex(i).setTypeId(p);
			}
		}

		public Var FindByName(String s)
		{
			Object o = vhash[s];
			if (o == null)
			return null;
			return ((Var)o);
		}

		public Var FindByIndex(int i)
		{
			Object x = i;
			Object o = vhash[x];
			if (o == null) return null;
			return (Var)o;
		}

		public int Length()
		{
			return vindex;	
		}

		public string genName()
		{
			string n = "NAME" + index;
			index++;
			return n;
		}
	}
}
