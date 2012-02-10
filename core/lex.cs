/*	$Id: lex.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;
using System.Text;
using System.Collections;

namespace Compiler {
    public class Tok {

		public const int TOK_1_DOLLAR	= 10001; // $
		public const int TOK_1_EQUALS	= 10002; // =
		public const int TOK_1_MINUS	= 10003; // -
		public const int TOK_1_PLUS		= 10004; // +
		public const int TOK_1_MUL		= 10005; // *
		public const int TOK_1_DIV		= 10006; // /
		public const int TOK_1_QUOTE	= 10007; // '
		public const int TOK_1_REL		= 10008; // :
		public const int TOK_1_SEMI		= 10009; // ;
		public const int TOK_1_DOT		= 10010; // .
		public const int TOK_1_COMMA	= 10011; // ,
		public const int TOK_1_LBRACKET	= 10012; // (
		public const int TOK_1_RBRACKET	= 10013; // )
		public const int TOK_1_PERCENT	= 10014; // %
		public const int TOK_1_AND		= 10015; // &
		public const int TOK_1_XOR		= 10016; // ^
		public const int TOK_1_SHARP	= 10017; // #
		public const int TOK_1_OR		= 10018; // |
		public const int TOK_1_NOT		= 10019; // !
		public const int TOK_1_G		= 10020; // >
		public const int TOK_1_L		= 10021; // <
		public const int TOK_1_NEAR		= 10022; // ~
		public const int TOK_1_BQUOTE	= 10023; // `
		public const int TOK_1_BSLASH	= 10024; // \
		public const int TOK_1_SQRBR	= 10025; // ]
		public const int TOK_1_SQLBR	= 10026; // [
		public const int TOK_1_TRRBR	= 10027; // }
		public const int TOK_1_TRLBR	= 10028; // {
		public const int TOK_1_DQUOTE	= 10029; // "
		public const int TOK_1_WHAT		= 10030; // ?
		public const int TOK_1_UNDER	= 10031; // _

		public const int TOK_2_LE		= 20001; // <=
		public const int TOK_2_POWER	= 20002; // **
		public const int TOK_2_GE		= 20003; // >=
		public const int TOK_2_NE		= 20004; // !=
		public const int TOK_2_PTR		= 20005; // ->
		public const int TOK_2_LREMARK	= 20006; // /*
		public const int TOK_2_RREMARK	= 20007; // */
		public const int TOK_2_NL		= 20008; // !<
		public const int TOK_2_NG		= 20009; // !>
		public const int TOK_2_CAT		= 20010; // ||
		public const int TOK_2_DOTS		= 20011; // ..

		public const int TOK_GT			= 30001; // GT
		public const int TOK_LT			= 30002; // LT
		public const int TOK_GE			= 30003; // GE
		public const int TOK_LE			= 30004; // LE
		public const int TOK_NG			= 30005; // NG
		public const int TOK_NL			= 30006; // NL
		public const int TOK_NE			= 30007; // NE
		public const int TOK_EQU		= 30008; // EQU
		public const int TOK_CAT		= 30009; // CAT
		public const int TOK_PT			= 30010; // PT
		public const int TOK_NOT		= 30011; // NOT
		public const int TOK_OR			= 30012; // OR
		public const int TOK_AND		= 30013; // AND
		public const int TOK_XOR		= 30014; // XOR
		public const int TOK_MOD		= 30015; // MOD
		public const int TOK_DIV		= 30016; // DIV
		public const int TOK_MUL		= 30017; // MUL
		public const int TOK_PLUS		= 30018; // PLUS
		public const int TOK_MINUS		= 30019; // MINUS

		public const int TOK_ASSIGN		= 40010; // ASSIGN
		public const int TOK_BY			= 40020; // BY
		public const int TOK_CALL		= 40030; // CALL
		public const int TOK_DECLARE	= 40040; // DECLARE
		public const int TOK_DCL		= 40050; // DCL
		public const int TOK_DO			= 40060; // DO
		public const int TOK_ELSE		= 40070; // ELSE
		public const int TOK_END		= 40080; // END
		public const int TOK_GO			= 40100; // GO
		public const int TOK_GOTO		= 40110; // GOTO
		public const int TOK_IF			= 40120; // IF
		public const int TOK_INITIAL	= 40130; // INITIAL
		public const int TOK_LABEL		= 40140; // LABEL
		public const int TOK_LITERALLY	= 40150; // LITERALLY
		public const int TOK_OPTIONS	= 40160; // OPTIONS
		public const int TOK_PROCEDURE	= 40170; // PROCEDURE
		public const int TOK_PROC		= 40180; // PROC
		public const int TOK_RECURSIVE	= 40190; // RECURSIVE
		public const int TOK_RETURNS	= 40200; // RETURNS
		public const int TOK_RETURN		= 40210; // RETURN
		public const int TOK_THEN		= 40220; // THEN
		public const int TOK_TO			= 40230; // TO
		public const int TOK_WHILE		= 40240; // WHILE
		public const int TOK_CASE		= 40250; // CASE

        public const int TOK_IDENT      = 50001;
        public const int TOK_DIGITS     = 50002;
        public const int TOK_PARAM      = 50003;
        public const int TOK_UNKNOWN    = 99999;
        public const int TOK_EOF        = -1;

		public const int TOK_S_BIN      = 21000;
		public const int TOK_S_OCT      = 21010;
		public const int TOK_S_DEC      = 21020;
		public const int TOK_S_HEX      = 21030;

        public const int TOK_BINARY     = 70001; // int16
        public const int TOK_DECIMAL    = 70002; // int32
        public const int TOK_FIXED      = 70003; // int32
        public const int TOK_FLOAT      = 70004; // float
        public const int TOK_REAL       = 70005; // float
        public const int TOK_COMPLEX    = 70006; // double float
		public const int TOK_VOID		= 70007;
        public const int TOK_DEFTYPE	= 70008;

		public const int T_EXTERN		= 80001; // CRL Specific
		public const int T_STATIC		= 80002;
		public const int T_AUTO			= 80003;
		public const int T_SIGNED		= 80004;
		public const int T_UNSIGNED		= 80005;
		public const int T_DEFCLASS		= 80006;
		public const int T_STORAGE_MIN	= T_EXTERN;
		public const int T_STORAGE_MAX	= T_DEFCLASS;
		public const int T_PARAM		= 80007	/* special for tagging parameters */;

        static Hashtable tokens;
        StringBuilder value;
        int token_id;
        Io io;

        public void InitHash()
        {
            tokens = new Hashtable();

            add_tok(TOK_1_DOLLAR,   "$");
            add_tok(TOK_1_EQUALS,   "=");
            add_tok(TOK_1_MINUS,    "-");
            add_tok(TOK_1_PLUS,     "+");
            add_tok(TOK_1_MUL,      "*");
            add_tok(TOK_1_DIV,      "/");
            add_tok(TOK_1_QUOTE,	"'");
            add_tok(TOK_1_REL,      ":");
            add_tok(TOK_1_SEMI,	    ";");
            add_tok(TOK_1_DOT,	    ".");
            add_tok(TOK_1_COMMA,    ",");
            add_tok(TOK_1_LBRACKET, "(");
            add_tok(TOK_1_RBRACKET, ")");
            add_tok(TOK_1_PERCENT,  "%");
            add_tok(TOK_1_AND,	    "&");
            add_tok(TOK_1_XOR,	    "^");
            add_tok(TOK_1_SHARP,    "#");
            add_tok(TOK_1_OR,	    "|");
            add_tok(TOK_1_NOT,	    "!");
            add_tok(TOK_1_G,	    ">");
            add_tok(TOK_1_L,	    "<");
			add_tok(TOK_1_NEAR,     "~");
           	add_tok(TOK_1_UNDER,    "_");

			add_tok(TOK_2_LE,       "<=");
           	add_tok(TOK_2_POWER,    "**");
           	add_tok(TOK_2_GE,       ">=");
           	add_tok(TOK_2_NE,       "!=");
           	add_tok(TOK_2_PTR,      "->");
           	add_tok(TOK_2_LREMARK,  "/*");
           	add_tok(TOK_2_RREMARK,  "*/");
           	add_tok(TOK_2_NL,       "!<");
           	add_tok(TOK_2_NG,       "!>");
           	add_tok(TOK_2_CAT,      "||");
           	add_tok(TOK_2_DOTS,     "..");

           	add_tok(TOK_GT,         "GT");
           	add_tok(TOK_LT,         "LT");
           	add_tok(TOK_GE,         "GE");
           	add_tok(TOK_LE,         "LE");
           	add_tok(TOK_NG,         "NG");
           	add_tok(TOK_NL,         "NL");
           	add_tok(TOK_NE,         "NE");
           	add_tok(TOK_EQU,        "EQU");
           	add_tok(TOK_CAT,        "CAT");
           	add_tok(TOK_PT,         "PT");
           	add_tok(TOK_NOT,        "NOT");
           	add_tok(TOK_OR,         "OR");
           	add_tok(TOK_AND,        "AND");
           	add_tok(TOK_XOR,        "XOR");
           	add_tok(TOK_MOD,        "MOD");
			add_tok(TOK_DIV,        "DIV");
			add_tok(TOK_MUL,        "MUL");
			add_tok(TOK_PLUS,       "PLUS");
            add_tok(TOK_MINUS,      "MINUS");

			add_tok(TOK_ASSIGN,		"ASSIGN");
			add_tok(TOK_BY,			"BY");
           	add_tok(TOK_CALL,		"CALL");
           	add_tok(TOK_DECLARE,	"DECLARE");
           	add_tok(TOK_DCL,		"DCL");
           	add_tok(TOK_DO,			"DO");
           	add_tok(TOK_ELSE,		"ELSE");
			add_tok(TOK_END,		"END");
			add_tok(TOK_EOF,		"EOF");
			add_tok(TOK_GO,			"GO");
            add_tok(TOK_GOTO,		"GOTO");
            add_tok(TOK_IF,			"IF");
			add_tok(TOK_INITIAL,	"INITIAL");
			add_tok(TOK_LABEL,		"LABEL");
           	add_tok(TOK_LITERALLY,	"LITERALLY");
           	add_tok(TOK_OPTIONS,	"OPTIONS");
           	add_tok(TOK_PROCEDURE,	"PROCEDURE");
           	add_tok(TOK_PROC,		"PROC");
           	add_tok(TOK_RECURSIVE,	"RECURSIVE");
			add_tok(TOK_RETURNS,	"RETURNS");
			add_tok(TOK_RETURN,		"RETURN");
			add_tok(TOK_THEN,		"THEN");
            add_tok(TOK_TO,			"TO");
            add_tok(TOK_WHILE,		"WHILE");
            add_tok(TOK_CASE,		"CASE");

			add_tok(TOK_FIXED,      "FIXED");
			add_tok(TOK_FLOAT,      "FLOAT");
			add_tok(TOK_COMPLEX,    "COMPLEX");
			add_tok(TOK_REAL,       "REAL");
			add_tok(TOK_BINARY,		"BINARY");
			add_tok(TOK_DECIMAL,	"DECIMAL");

			add_tok(TOK_S_HEX,		"HEX");
			add_tok(TOK_S_OCT,		"OCT");
			add_tok(TOK_S_BIN,		"BIN");
			add_tok(TOK_S_DEC,		"DEC");
		}   

		public static void add_tok(int i, string s)
		{
			tokens.Add(s, i);
			tokens.Add(i, s);
		}
            
        public Tok(Io i)
        {
            io = i;
            InitHash();
        }

        int lookup_id()
        {
            String s = value.ToString();
            Object k = tokens[s];
            if (k == null) return 0;
            return (int) k;
        }

        bool is_op(char c)
        {
            return (
				c == '+' || c == '-' || c == '*' || c == '/' ||
                c == '<' || c == '>' || c == '=' || c == '&' ||
				c == '|' || c == '^' || c == '!' || c == ',' ||
				c == '~' || c == ':' || c == ';' || c == '.' ||
				c == '#' || c == '/' || c == '"' || c == '\\'
			 );
        }

        bool is_const_spec(char c)
        {
            return (
				c == 'B' || c == 'O' || c == 'D' || c == 'H'
            );
        }

		bool is_bin(char c)
		{
			return (
				c == '0' || c == '1'
				);
		}

		bool is_oct(char c)
		{
			return (
				c == '2' || c == '3' || c == '4' || c == '5' ||	c == '6' || c == '7'
				);
		}

		bool is_dec(char c)
		{
			return (
				c == '8' || c == '9'
				);
		}

		bool is_bracket(char c)
		{
			return (c == '(' || c == ')');
		}

		bool is_hex(char c)
		{
			return (
				c == 'A' || c == 'a' ||	c == 'B' || c == 'b' || c == 'C' || c == 'c' || 
				c == 'D' || c == 'd' ||	c == 'E' || c == 'e' || c == 'F' || c == 'f'
				);
		}

		bool is_const(char c)
		{
			return (
				is_hex(c) || is_bin(c) || is_dec(c) || is_oct(c)
			);
		}

        bool is_complex_op(char c)
        {
            return (
				c == '*' || c == '!' || c == '<' || c == '>' ||
				c == '-' || c == '|' || c == '.'
			);
        }

        void skipWhite()
        {
            while (Char.IsWhiteSpace(io.getNextChar())) io.ReadChar();
        }

        void load_ident_or_keyword()
        {
            value = new StringBuilder(Io.MAXSTR);
            skipWhite();
            while (Char.IsLetterOrDigit(io.getNextChar()) || (io.getNextChar() == '$') || (io.getNextChar() == '_'))
            {
				if (io.getNextChar() != '$') value.Append(io.getNextChar());
                io.ReadChar();
            }
            token_id = lookup_id();
            if (token_id <= 0) token_id = TOK_IDENT;
            skipWhite();
        }

        void load_digits() {
			char last, c;
			bool O, H, B, D, got;
			O = H = B = D = false;
			got = false;
            value = new StringBuilder(Io.MAXSTR);
            int number = 0;

            skipWhite();
			last = '0';

			while (is_const(c = io.getNextChar())) {
				if (is_bin(last)) B = true; else
				if (is_oct(last)) O = true; else
				if (is_dec(last)) D = true; else
				if (is_hex(last)) H = true;
				value.Append(c);
				io.ReadChar();
				last = c;
			}
			if (io.getNextChar() == 'H' || io.getNextChar() == 'h')
			{
				got = true;
				token_id = TOK_S_HEX;
				io.ReadChar();
			} 
			if ((!got) && (io.getNextChar() == 'O' || io.getNextChar() == 'o'))
			{
				if ((O || B) && (!H) && (!D))
				{
					got = true;
					token_id = TOK_S_OCT;
					io.ReadChar();
				} else
					io.Abort("PL0201: invalid octal constant");
			} 
			if ((!got) && (last == 'D' || last == 'd')) {
				if ((O || B || D) && (!H)) {
					got = true;
					token_id = TOK_S_DEC;
				} 
				else
					io.Abort("PL0202: invalid decimal constant");
			}
			if ((!got) && (last == 'B' || last == 'b')) {
				if ((B) && (!H) && (!O) && (!D)) {
					got = true;
					token_id = TOK_S_BIN;
					value.Remove(value.Length-1, 1);
				} 
				else
					io.Abort("PL0203: invalid binary constant");
			}
			if ((!got)) {
				if ((O || B || D) && (!H)) {
					got = true;
					token_id = TOK_S_DEC;
				} else
					io.Abort("PL0204: invalid decimal constant");
			}
            skipWhite();

            switch (token_id) {
            case TOK_S_HEX:		number = Number.FromRadix(value.ToString(), 16); break;
            case TOK_S_OCT:		number = Number.FromRadix(value.ToString(),  8); break;
            case TOK_S_DEC:		number = Number.FromRadix(value.ToString(), 10); break;
            case TOK_S_BIN:		number = Number.FromRadix(value.ToString(),  2); break;
            }
            value = new StringBuilder();
            value.Append(number.ToString());

			token_id = TOK_DIGITS;
        }

        void load_op() {
            value = new StringBuilder(Io.MAXSTR);
            skipWhite();
			while (is_op(io.getNextChar())) {
				value.Append(io.getNextChar());
				io.ReadChar();
			}
			token_id = lookup_id();
			if (token_id <= 0) token_id = TOK_UNKNOWN;
            skipWhite();
        }

		void load_bracket() {
			value = new StringBuilder(Io.MAXSTR);
			skipWhite();
			value.Append(io.getNextChar());
			io.ReadChar();
			token_id = lookup_id();
			if (token_id <= 0) token_id = TOK_UNKNOWN;
			skipWhite();
		}

		void load_eof() {
			value = null;
			token_id = TOK_EOF;
		}

        public void scan() {
            skipWhite();
			if (Char.IsLetter(io.getNextChar())) load_ident_or_keyword();
            else if (Char.IsDigit(io.getNextChar())) load_digits();
            else if (is_op(io.getNextChar())) load_op();
			else if (is_bracket(io.getNextChar())) load_bracket();
			else if (io.EOF()) load_eof();
            else {
                value = new StringBuilder(Io.MAXSTR);
                value.Append(io.getNextChar());
                token_id = TOK_UNKNOWN;
                io.ReadChar();
            }
            skipWhite();
        }

        public char getFirstChar() {
            if (value == null) return '\0';
            return value[0];
        }

        public String getValue() {
            if (value == null) return "";
            return value.ToString();
        }

		public void setValue(string s) {
			StringBuilder sb = new StringBuilder();
			sb.Append(s);
			value = sb;
		}

		public int getId() {
			return (token_id);
		}

		public void setId(int i) {
			token_id = i;
		}

        public bool NotEOF() {
            return (token_id != TOK_EOF);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
			sb.Append("token{value:('"+getValue()+"');id("+token_id+")}");
            return sb.ToString();
        }

    }
}
