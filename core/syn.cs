/*	$Id: syn.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;
using System.Text;
using System.Collections;

namespace Compiler {

    public class Parser {

        private Io io;
        private Lib lib;
        private Tok tok;
        private Emit emit;

		int label_count = 0;
		string last_label = null;

		public Parser(Io i, Tok t, Lib l)
		{
			io = i;
			tok = t;
			lib = l;
		}

		VarList tree;
		VarList currenttree;

        int typeCheck(int Type1, int Type2) {
            if (Type1 >= Type2)
            	return Type1;
			else
				return Type2;
        }

        void typeCheckAssign(int exprType, int varType) {

            if (varType >= exprType) return;
            if ((varType < Tok.TOK_FLOAT) && (exprType >= Tok.TOK_FLOAT))
            	io.Abort("PL0101: invalid typecast (float to int)");
            if ((varType == Tok.TOK_BINARY) && (exprType > Tok.TOK_BINARY))
            	io.Abort("PL0102: invalid typecast (long int to short int)");
            if ((varType < Tok.TOK_COMPLEX) && (exprType == Tok.TOK_COMPLEX))
            	io.Abort("PL0103: invalid typecast (long float to short float)");
        }

        Var call_construct(string s) {
			Var procvar = new Var();
			Var e;
			int i = 0;
			int parType = Tok.TOK_BINARY;

			e = tree.FindByName(s);

            if (e == null) io.Abort("PL0132: invalid procedure call: undefined procedure");
            if (e.getType() != Var.VAR_BLOCK) io.Abort("PL0133: invalid procedure call: not a procedure");

			tok.scan();
			if (tok.getId() != Tok.TOK_1_LBRACKET)
				io.Abort("PL0134: '(' expected");
			tok.scan();
			if (tok.getId() != Tok.TOK_1_RBRACKET) {
				parType = bool_expr(0);
				typeCheckAssign(parType, e.getParams().FindByIndex(i).getTypeId());
				i ++;
				while (tok.getId() == Tok.TOK_1_COMMA) {
					tok.scan();
					parType = bool_expr(0);
 					typeCheckAssign(parType, e.getParams().FindByIndex(i).getTypeId());
                    i ++;
				}
				if (tok.getId() != Tok.TOK_1_RBRACKET)
					io.Abort("PL0135: ')' expected");
			}

			if (i != e.getParams().Length())
				io.Abort("PL0136: invalid procedure call: wrong number of parametrs");

			return e;
        }

		int ident() {
  			int Typecast = Tok.TOK_VOID;
			
  			if (io.getNextChar() == '(') {

  				io.Message(tok+"[Ident]");

  				Var e = call_construct(tok.getValue());
  				Typecast = e.getTypeId();

    			if (Typecast == Tok.TOK_VOID)
	      			io.Abort("PL0147: using void function where expecting a value");

	            emit.Call(e);
                tok.scan();
    		}
  			else
    		{
    			if (currenttree.FindByName(tok.getValue()) == null) 
    				io.Abort("PL0105: undeclared variable");

                Typecast = currenttree.FindByName(tok.getValue()).getTypeId();
    			emit.Load(currenttree.FindByName(tok.getValue()));

                io.Message(tok+"[Ident]");
    			tok.scan();
    		}
    		return Typecast;
  		}

		int factor(int Type2) {
			int Typecast = Type2;

			if (tok.getFirstChar() == '(')
    		{
				io.Message(tok+"[(]");
    			tok.scan();

    			Typecast = typeCheck(Typecast, bool_expr(Typecast));

    			if (tok.getFirstChar() != ')') io.Abort("PL0106: ')' expected");

				io.Message(tok+"[)]");
    			tok.scan();
    		}
  			else if (tok.getId() == Tok.TOK_IDENT)
    		{      
    			Typecast = typeCheck(Typecast, ident());
    		}
  			else
    		{
    			emit.LoadConst(tok.getValue());
				Typecast = Tok.TOK_FIXED;
				io.Message(tok+"[Factor]");
    			tok.scan();
    		}
    		return Typecast;
  		}

		int unary_factor()	{
			int Typecast = Tok.TOK_BINARY;

  			if (tok.getId() == Tok.TOK_1_PLUS) {
				io.Message(tok+"[UnaryPlus]");
				tok.scan();

    			Typecast = typeCheck(Typecast, factor(Typecast));

    			return Typecast;
    		}
  			if (tok.getId() == Tok.TOK_1_MINUS)	{
				io.Message(tok+"[UnaryMinus]");
				tok.scan();

    			if (tok.getId() == Tok.TOK_DIGITS) {
      				StringBuilder sb = new StringBuilder("-");
      				sb.Append(tok.getValue());

      				emit.LoadConst(sb.ToString());
					Typecast = Tok.TOK_FIXED;

					io.Message(tok+"[UnaryFactor]");
      				tok.scan();

      				return Typecast;
      			}

    			Typecast = typeCheck(Typecast, factor(Typecast));
    			emit.Insn("neg");
    			return Typecast;
    		}
  			Typecast = typeCheck(Typecast, factor(Typecast));

  			return Typecast;
  		}

		int term_mul(int Type2) {
			int Typecast = Type2;

			io.Message(tok+"[*]");
  			tok.scan();

  			Typecast = typeCheck(Typecast, Typecast = factor(Typecast));

			emit.Insn("mul");

			return Typecast;
  		}

		int term_div(int Type2) {
		 	int Typecast = typeCheck(Type2, Tok.TOK_FLOAT);

			io.Message(tok+"[/]");
  			tok.scan();

  			Typecast = typeCheck(Typecast, factor(Typecast));

			emit.Insn("div");

			return Typecast;
  		}

		int term1(int Type2) {
	        int Typecast = Type2;

  			while ((tok.getId() == Tok.TOK_1_MUL) || (tok.getId() == Tok.TOK_1_DIV)) {
    			switch (tok.getId()) {
      				case Tok.TOK_1_MUL: Typecast = typeCheck(Typecast, term_mul(Typecast)); break;
      				case Tok.TOK_1_DIV: Typecast = typeCheck(Typecast, term_div(Typecast)); break;
      			}
    		}

    		return Typecast;
  		}

		int term(int Type2) {
			int Typecast = Type2;
			
  			Typecast = typeCheck(Typecast, factor(Typecast));
  			Typecast = typeCheck(Typecast, term1(Typecast));

  			return Typecast;
  		}

		int first_term() {
			int Typecast = Tok.TOK_BINARY;

  			Typecast = typeCheck(Typecast, unary_factor());
  			Typecast = typeCheck(Typecast, term1(Typecast));

  			return Typecast;
  		}

		int expr_add(int Type2) {
			int Typecast = Type2;

			io.Message(tok+"[+]");
  			tok.scan();

  			Typecast = typeCheck(Typecast, term(Typecast));

  			emit.Insn("add");

  			return Typecast;
  		}

		int expr_sub(int Type2) {
			int Typecast = Type2;

			io.Message(tok+"[-]");
  			tok.scan();

  			Typecast = typeCheck(Typecast, term(Typecast));

  			emit.Insn("sub");
  			return Typecast;
  		}

		int math_expr(int Type2) {
			int Typecast = Tok.TOK_BINARY;

            Typecast = typeCheck(Typecast, first_term());

  			while ((tok.getId() == Tok.TOK_1_PLUS) || (tok.getId() == Tok.TOK_1_MINUS)) {
    			switch (tok.getId()) {
      				case Tok.TOK_1_PLUS:  Typecast = typeCheck(Typecast, expr_add(Typecast)); break;
      				case Tok.TOK_1_MINUS: Typecast = typeCheck(Typecast, expr_sub(Typecast)); break;
      			}
    		}

    		return Typecast;
  		}

		void rel_1_EQUALS() {
			io.Message(tok+"[Equals]");
  			tok.scan();

  			math_expr(Tok.TOK_BINARY);

  			emit.Insn("ceq");
  		}

		void rel_2_NE() {
			io.Message(tok+"[!=]");
  			tok.scan();
  			math_expr(Tok.TOK_BINARY);

  			emit.Insn("ceq");
  			emit.LoadConst("1");
  			emit.Insn("ceq");
  		}

		void rel_1_L() {
			io.Message(tok+"[<]");
  			tok.scan();
  			math_expr(Tok.TOK_BINARY);

  			emit.Insn("clt");
  		}

		void rel_1_G() {
			io.Message(tok+"[>]");
  			tok.scan();
		  	math_expr(Tok.TOK_BINARY);

  			emit.Insn("cgt");
  		}

		void rel_2_GE() {
			io.Message(tok+"[>=]");
  			tok.scan();
  			math_expr(Tok.TOK_BINARY);

  			emit.Insn("clt");
  			emit.LoadConst("0");
  			emit.Insn("ceq");
  		}

		void rel_2_LE() {
			io.Message(tok+"[<=]");
  			tok.scan();
  			math_expr(Tok.TOK_BINARY);
  			emit.Insn("cgt");
  			emit.LoadConst("0");
  			emit.Insn("ceq");
  		}

		int rel_expr() {
			int Type1;
			int Typecast = Tok.TOK_BINARY;

  			Type1 = math_expr(Tok.TOK_VOID);
  			switch (tok.getId()) {
   			case Tok.TOK_1_EQUALS: rel_1_EQUALS(); break;
   			case Tok.TOK_2_NE: rel_2_NE(); break;
   			case Tok.TOK_1_L: rel_1_L(); break;
   			case Tok.TOK_1_G: rel_1_G(); break;
   			case Tok.TOK_2_GE: rel_2_GE(); break;
   			case Tok.TOK_2_LE: rel_2_LE(); break;
   			default: Typecast = Type1; break;
    		}

    		return Typecast;
  		}

		int not_factor() {
			int Typecast = Tok.TOK_BINARY;

            if (tok.getId() == Tok.TOK_1_NOT) {
    			Typecast = rel_expr();
    			emit.Insn("not");
    		} else
				Typecast = rel_expr();

			return Typecast;
  		}

		void bool_or() {
  			io.Message(tok+"[|]");
			tok.scan();
  			term_bool(0);
  			emit.Insn("or");
  		}

		void bool_xor() {
  			io.Message(tok+"[^]");
			tok.scan();
  			term_bool(0);
  			emit.Insn("xor");
  		}

		void bool_and() {
			io.Message(tok+"[&]");
 			tok.scan();
  			term_bool(0);
  			emit.Insn("and");
  		}

		int term_bool(int Type2) {
			int Typecast = Tok.TOK_BINARY;

  			Typecast = not_factor();
 			while (tok.getId() == Tok.TOK_1_OR ||
 				tok.getId() == Tok.TOK_1_XOR ||
 				tok.getId() == Tok.TOK_1_AND) {
    			switch (tok.getId()) {
				case Tok.TOK_1_OR  : bool_or();  break;
      			case Tok.TOK_1_XOR : bool_xor(); break;
      			case Tok.TOK_1_AND : bool_and(); break;
      			}
    		}

    		return Typecast;
  		}

		int bool_expr(int Type2) {
			int Typecast = Tok.TOK_BINARY;

  			Typecast = typeCheck(Typecast, term_bool(Type2));
  			int id = tok.getId();
  			if ((id == Tok.TOK_1_OR) || (id == Tok.TOK_1_AND)) {
    			string label1 = new_label();
    			string label2 = new_label();
    			string label3 = new_label();
    			while (true) {
      				id = tok.getId();
      				if (id == Tok.TOK_1_AND) emit.Branch("brfalse", label1);
      				else if (id == Tok.TOK_1_OR) emit.Branch("brtrue", label2);
      				else break;
      				tok.scan();
      				term_bool(0);
      			}
				emit.Branch("brtrue", label2);
				emit.Label(label1);	// false path
				emit.LoadConst("0");
				emit.Branch("br",label3);
				emit.Label(label2);	// true path
				emit.LoadConst("1");
				emit.Label(label3);	// common path
                Typecast = Tok.TOK_BINARY;
    		}

    		return Typecast;
  		}

		string new_label() {
			StringBuilder sb = new StringBuilder("L@@");
			sb.Append(label_count++);
			return (sb.ToString());
		}

		void var_list(VarList curtree) {
			Var var1 = new Var();
			var1.setName(tok.getValue());
			try
			{
				curtree.add(var1);
			}
			catch
			{
				io.Abort("PL0107: invalid variable declaration");
			}
			io.Message(tok+"[Var]");
			tok.scan();
			while ((tok.getId() == Tok.TOK_1_COMMA) || (tok.getId() == Tok.TOK_IDENT)) {
				if (tok.getFirstChar() != ',') io.Abort("PL0108: ',' or ')' expected");
				io.Message(tok+"[Comma]");
				tok.scan();
				if (tok.getId() != Tok.TOK_IDENT) io.Abort("PL0109: ident expected");
				var1 = new Var();
				var1.setName(tok.getValue());
				try
				{
					curtree.add(var1);
				}
				catch
				{
					io.Abort("PL0107: invalid variable declaration");
				}
   				io.Message(tok+"[Var]");
				tok.scan();
			}
		}

		void param(VarList curtree) {
			io.Message(tok+"[(]");
			tok.scan();
			var_list(curtree);
			if (tok.getId() != Tok.TOK_1_RBRACKET) io.Abort("PL0110: ')' expected");
			io.Message(tok+"[)]");
			tok.scan();
		}

		void label(Tok s) {

			Var var = new Var();
			String label1 = "L@@"+s.getValue();

			io.Message(s+"[Label]");
			var.setName(label1);
			var.setType(Var.VAR_LABEL);
			try
			{
				currenttree.add(var);
			}
			catch
			{
				io.Abort("PL0111: invalid label declaration");
			}
			if (tok.getId() != Tok.TOK_1_REL) io.Abort("PL0112: ':' expected");
			io.Message(tok+"[:]");
			last_label = s.getValue();
			tok.scan();			
		}

		void simple_var(VarList curtree) {
			Var var = new Var();
			var.setName(tok.getValue());		
            var.setClassId(Tok.T_DEFCLASS);
			try
			{
				curtree.add(var);
			}
			catch
			{
				io.Abort("PL0107: invalid variable declaration");
			}
			io.Message(tok+"[Var]");
			tok.scan();
		}

		void declarations(VarList curtree) {
			while ((tok.getId() == Tok.TOK_DCL) ||
				(tok.getId() == Tok.TOK_DECLARE)) {
					decl_stmt(curtree); 
					null_stmt();
			}
		}

		void decl_stmt(VarList curtree)
		{
			VarList vars = new VarList();

			io.Message(tok+"[DECLARE]");
			tok.scan();
			
			switch (tok.getId())
			{
				case Tok.TOK_1_LBRACKET:
					param(vars);
					break;
				case Tok.TOK_IDENT:
					simple_var(vars);
					break;
				case Tok.TOK_DIGITS:
					break;
			}

			if (tok.getId() == Tok.TOK_1_LBRACKET)
			{
				io.Message(tok+"[(]");
				tok.scan();
				if (tok.getId() != Tok.TOK_DIGITS) io.Abort("PL0113: constant expected");
				io.Message(tok+"[Constant]");

				vars.setNodesGranularity(System.Convert.ToInt32(tok.getValue()));

				tok.scan();
				if (tok.getId() != Tok.TOK_1_RBRACKET) io.Abort("PL0114: ')' expected");
				io.Message(tok+"[)]");
				tok.scan();
			}

			switch (tok.getId())
			{
				case Tok.TOK_FIXED:
				case Tok.TOK_FLOAT:   
				case Tok.TOK_COMPLEX: 
				case Tok.TOK_REAL:    
				case Tok.TOK_BINARY:  
				case Tok.TOK_DECIMAL:
					vars.setNodesType(Var.VAR_LOCAL);
					vars.setNodesTypeId(tok.getId());
					try 
					{
						curtree.mergeNodes(vars);
					}
					catch
					{
						io.Abort("PL0107: invalid variable declaration");
					}
					io.Message(tok+"[Type]");
					tok.scan();
				break;
				default:
					io.Abort("PL0115: type specifier expected");
					break;
			}
		}

		void proc_decl(VarList curtree, String label) {

			Var procvar = new Var();
			procvar.setName(label);
			procvar.setType(Var.VAR_BLOCK);
            procvar.setTypeId(Tok.TOK_VOID);

			if (label == null) io.Abort("PL0116: PROC declaration needs LABEL");
			io.Message(tok+"[PROC]");

			tok.scan();
			procvar.nodes = new VarList();

			if (tok.getId() == Tok.TOK_1_LBRACKET) {
				param(procvar.nodes);
				procvar.setNodesType(Var.VAR_PARAM);
			}

			switch (tok.getId()) {
			case Tok.TOK_RETURNS:
				io.Message(tok+"[RETURNS]");
				tok.scan();
				if (tok.getId() != Tok.TOK_1_LBRACKET)	io.Abort("PL0104: ')' expected.");
				io.Message(tok+"[(]");
				tok.scan();
				switch (tok.getId()) {
					case Tok.TOK_FIXED:
					case Tok.TOK_FLOAT:   
					case Tok.TOK_COMPLEX: 
					case Tok.TOK_REAL:    
					case Tok.TOK_BINARY:  
					case Tok.TOK_DECIMAL:
						io.Message(tok+"[Type]");
	                    procvar.setTypeId(tok.getId());
						break;
					default:
						io.Abort("PL0115: type specifier expected");
						break;
				}
				tok.scan();
				if (tok.getId() != Tok.TOK_1_RBRACKET) io.Abort("PL0114: ')' expected");
				io.Message(tok+"[)]");
				tok.scan();
				break;
			case Tok.TOK_RECURSIVE:
            	io.Message(tok+"[RECURSIVE]");
				if (label.ToUpper() == "MAIN") io.Abort("PL0146: MAIN can not be RECURSIVE");
                tok.scan();
				break;

			}

			null_stmt();

            emit.FuncBegin(procvar);

            declarations(procvar.nodes);

            VarList prms = procvar.getParams();
			if (prms != null)
				for (int i = 0; i < prms.Length(); i++)
					if (prms.FindByIndex(i).getTypeId() == 0)
						io.Abort("PL0117: undeclared parameter");

            if (procvar.getLocals() != null) {
	            emit.LocalVars(procvar.getLocals());
	        }

			try
			{
				curtree.add(procvar);
			}
			catch
			{
				io.Abort("PL0120: invalid procedure declaration");
			}

			do {
				stmt(procvar.nodes, label, null);
			} while (tok.getId() != Tok.TOK_END);

			if (tok.getId() != Tok.TOK_END) io.Abort("PL0118: END expected");
			io.Message(tok+"[END]");
			tok.scan();
			
			if (tok.getValue() != label) io.Abort("PL0119: unclosed PROC");
			io.Message(tok+"[Label]");

			tok.scan();

			emit.Ret();
			emit.FuncEnd();

            if (io.getGenList()) emit.LIST();
            emit.IL();

			emit.Finish();
        }

		void assign_stmt(VarList curtree) {	// Should be rewrited
			VarList vars = new VarList();
			int i;
			var_list(vars);

			if (tok.getId() != Tok.TOK_1_EQUALS) io.Abort("PL0121: '=' expected");
			for (i = 0; i < vars.Length(); i ++) {
				if (curtree.FindByName(vars.FindByIndex(i).getName()) == null) io.Abort("PL0122: undeclared variable");
			}
			io.Message(tok+"[Equals]");
			tok.scan();					
			bool_expr(0);
			for (i = 0; i < vars.Length(); i ++) {
				emit.Store(curtree.FindByName(vars.FindByIndex(i).getName()));
				if (i < vars.Length() -1) 
					emit.Load(curtree.FindByName(vars.FindByIndex(0).getName()));
			}
		}

		void do_stmt(VarList curtree)
		{
			bool needBranch = true;

			String label1 = new_label(); // loop start
			String label2 = new_label(); // loop end
			String label3 = new_label();
			String label4 = new_label();

			Var procvar = new Var();
			procvar.setName(curtree.genName());
			procvar.setType(Var.VAR_BLOCK);
			procvar.nodes = new VarList();

			for (int i=0; i<curtree.Length(); i++) 
				if ((curtree.FindByIndex(i).type & (Var.VAR_LOCAL|Var.VAR_PARAM|Var.VAR_LABEL)) != 0)
					procvar.add(curtree.FindByIndex(i));

			io.Message(tok+"[DO]");
			tok.scan();
			switch (tok.getId()) 
			{
				case Tok.TOK_WHILE:
					io.Message(tok+"[WhileStatement]");
                    emit.Label(label1);
					tok.scan();
					bool_expr(0);
					null_stmt();
                    emit.Branch("brfalse", label2);
					break;
				case Tok.TOK_IDENT:				// TODO
					assign_stmt(curtree);
					if (tok.getId() != Tok.TOK_TO) io.Abort("PL0123: TO expected");
					tok.scan();
					bool_expr(0);
					null_stmt();
					break;
				case Tok.TOK_CASE: 
					io.Message(tok+"[CaseStatement]");
					tok.scan();					
					break;                          
				case Tok.TOK_1_SEMI:
					io.Message(tok+"[;]");            
					needBranch = false;
                    tok.scan();
					break;
			}

			do {
				stmt(procvar.nodes, label2, label1);
			} while (tok.getId() != Tok.TOK_END);

			if (needBranch) emit.Branch("br", label1);	
		   
			if (tok.getId() != Tok.TOK_END) io.Abort("PL0124: END expected");
			io.Message(tok+"[END]");
			
			tok.scan();	
			curtree.add(procvar);

            if (needBranch) emit.Label(label2);
		}

		void goto_stmt(VarList curtree)     
		{
			String label1;
			                                          
			if (tok.getId() == Tok.TOK_GO) {
				io.Message(tok+"[GO]");
				tok.scan();
				if (tok.getId() != Tok.TOK_TO) io.Abort("PL0125: TO expected");
			}

			io.Message(tok+"[GOTO]");
			tok.scan();
			switch (tok.getId())
			{
				case Tok.TOK_IDENT:
					label1 = "L@@"+tok.getValue();
					if ((curtree.FindByName(label1) != null) &&
						(curtree.FindByName(label1).getType() == Var.VAR_LABEL)) {
						emit.Branch("br", label1);
						io.Message(tok+"[Label]");
					} else {
						io.Abort("PL0126: undefined label");
					}
					break;
				case Tok.TOK_DIGITS:
					io.Message(tok+"[Source Line]");
					break;
				default: io.Abort("PL0127: GOTO without direction");
					break;
			}
			tok.scan();
		}

		void ret_stmt(String ilabel, String olabel)	{
			int gotType = Tok.TOK_VOID;

			if (ilabel == null) io.Abort("PL0128: illegal RETURN to nowhere");
			io.Message(tok+"[RETURN]");
			tok.scan();

			if (tok.getId() != Tok.TOK_1_SEMI) 
			{
				gotType = bool_expr(0);
				Var e = tree.FindByName(ilabel);
				if (e != null)
	                typeCheckAssign(gotType,  e.getTypeId());
				if (tok.getFirstChar() != ';') io.Abort("PL0129: ';' expected");
			}

			emit.Ret();
		}

		void if_stmt(VarList curtree, String olabel, String ilabel)	{
			String label1;
			String label2;

			io.Message(tok+"[IF]");
  			tok.scan();

  			bool_expr(0);

			label1 = new_label();
			label2 = String.Copy(label1);

			emit.Branch("brfalse", label1);

            if (tok.getId() != Tok.TOK_THEN) io.Abort("PL0130: THEN expected");
			io.Message(tok+"[THEN]");
			tok.scan();

			stmt(curtree, olabel, ilabel);
			
			if (tok.getId() == Tok.TOK_ELSE) {
				io.Message(tok+"[ELSE]");
				label2 = new_label();
				emit.Branch("br", label2);

				emit.Label(label1);

				tok.scan();
				stmt(curtree, olabel, ilabel);
			}
            emit.Label(label2);
  		}

  		void call_stmt(VarList curtree) {

			io.Message(tok+"[CALL]");
            tok.scan();

            if (tok.getId() != Tok.TOK_IDENT) io.Abort("PL0131: ident after CALL expected");
			io.Message(tok+"[Ident]");

			Var e = call_construct(tok.getValue());

            emit.Call(e);
			if (e.getTypeId() != Tok.TOK_VOID)	emit.Insn("pop");

            tok.scan();
  		}

		void ident_stmt(VarList curtree) {
			Tok s = new Tok(null);
			VarList vars = new VarList();
			Var V;
			int gotType = Tok.TOK_BINARY;

			s.setValue(tok.getValue());
			s.setId(tok.getId());
			tok.scan();

			switch (tok.getId()) {
			case Tok.TOK_1_EQUALS:
				if (curtree.FindByName(s.getValue()) == null) io.Abort("PL0137: undeclared variable");
				V = new Var(); V.setName(s.getValue()); vars.add(V);
				io.Message(s+"[Variable]");
				io.Message(tok+"[Equals]");
				tok.scan();					
				gotType = bool_expr(0);
                typeCheckAssign(gotType, curtree.FindByName(vars.FindByIndex(0).getName()).getTypeId());
				null_stmt();
				break;
    		case Tok.TOK_1_COMMA:
				io.Message(s+"[Variable]");
				if (curtree.FindByName(s.getValue()) == null) io.Abort("PL0137: undeclared variable");
                V = new Var(); V.setName(s.getValue()); vars.add(V);
				while (tok.getId() != Tok.TOK_1_EQUALS)	{                   
					if (tok.getFirstChar() != ',') io.Abort("PL0138: ',' or '=' expected");
					io.Message(tok+"[Comma]");
					tok.scan();

					if (tok.getId() != Tok.TOK_IDENT) io.Abort("PL0139: ident expected");
					if (curtree.FindByName(tok.getValue()) == null) io.Abort("PL0137: undeclared variable");
                    V = new Var(); V.setName(tok.getValue()); vars.add(V);
					io.Message(tok+"[Variable]");					
					tok.scan();
				}
				io.Message(tok+"[Assign]");
				tok.scan();
				gotType = bool_expr(0);
                typeCheckAssign(gotType, curtree.FindByName(vars.FindByIndex(0).getName()).getTypeId());
				null_stmt();               
				break;
			case Tok.TOK_1_REL:
				label(s);
				break;
			default:
				io.Abort("PL0140: not found expected token ':', ',' or '='");
				break;
			}

			for (int i = 0; i < vars.Length(); i ++) {
				typeCheckAssign(gotType, curtree.FindByName(vars.FindByIndex(i).getName()).getTypeId());
				emit.Store(curtree.FindByName(vars.FindByIndex(i).getName()));
				if (i < vars.Length() -1) 
					emit.Load(curtree.FindByName(vars.FindByIndex(0).getName()));
			}
		}

		void null_stmt() 
		{
			if (tok.getFirstChar() != ';') io.Abort("PL0141: ';' expected");
			io.Message(tok+"[;]");
			tok.scan();
		}

		void stmt(VarList curtree, String ilabel, String olabel)
		{
			Var e = new Var();
			currenttree = curtree;

			if (io.EOF())
				io.Abort("PL0142: expected statement but end of file encountered");

			if ((tok.getId() != Tok.TOK_PROC) && (tok.getId() != Tok.TOK_PROCEDURE)) 
				if (last_label != null) {
					emit.Label("L@@"+last_label);
					last_label = null;
				}

			switch (tok.getId()) {
				case Tok.TOK_RETURN:
	 				ret_stmt(ilabel, olabel);
					null_stmt();
					break;
				case Tok.TOK_IDENT:
					ident_stmt(curtree);				
					break;
				case Tok.TOK_CALL:
					call_stmt(curtree);
                    null_stmt();
					break;
				case Tok.TOK_DO:
					do_stmt(curtree);
					null_stmt();
					break;					             
				case Tok.TOK_GO:
				case Tok.TOK_GOTO:				
					goto_stmt(curtree);
					null_stmt();
					break;					           
				case Tok.TOK_PROCEDURE:
                case Tok.TOK_PROC:					
					proc_decl(curtree, last_label);
					null_stmt();
					break;		
				case Tok.TOK_IF: 
					if_stmt(curtree, ilabel, olabel); 
					break;
				case Tok.TOK_DCL:
				case Tok.TOK_DECLARE: 
					io.Abort("PL0143: wild declaration found");
					break;			
				case Tok.TOK_UNKNOWN: 
					io.Abort("PL0144: unknown construction");
					break;
				case Tok.TOK_END:
				case Tok.TOK_ELSE:
				case Tok.TOK_THEN:
					break;
				default:
					io.Abort("PL0145: wild symbol found");
					break;
                }			
		}

        public void parse_and_compile()
        {
			prolog();

			tree = new VarList();

            IDictionaryEnumerator libEnum = lib.get_enum();

            while ( libEnum.MoveNext() ) {
				Var procvar = new Var();
				LibFunc lfunc = (LibFunc)libEnum.Value;

				procvar.setName(lfunc.nameShort);
				procvar.setType(Var.VAR_BLOCK);
        	    procvar.setTypeId(Tok.TOK_VOID);
				procvar.nodes = new VarList();

				for (int i = 0; i < lfunc.typeParams.Count; i++ ) {
					Var param = new Var();
					param.setName("PAR_"+i);
					param.setType(Var.VAR_PARAM);
					param.setTypeId((int)lfunc.typeParams[i]);
					procvar.nodes.add(param);
				}

        	    tree.add(procvar);
			}

			io.ReadChar();
			tok.scan();
            declarations(tree);
            while (tok.NotEOF())
            {
				stmt(tree, null, null);
			}
			io.Message("compiled successfuly");
			io.TreeDraw(tree);		
			epilog();
        }

		void prolog() {
			emit = new Emit(io, lib);
			emit.BeginModule();
			emit.BeginClass();
		}

		void epilog() {
			emit.EndClass();
			emit.EndModule();
		}

    }
}
