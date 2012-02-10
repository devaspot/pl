/*	$Id: num.cs,v 1.1 2004/06/03 09:02:42 master Exp $

	PL/IL
	Copyright (c) 2003-2004

	Maxim E. Sokhatsky (mes@ua.fm)
   	Oleg V. Smirnov (_straycat@ukr.net)
*/

using System;

namespace Compiler {

	public class Number {

		public static char [] Hex = new char [] { '0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F' };

		public static string ToRadix(int i, int radix) {
			string s = null;
			while (i > 0) {	s = Number.Hex[i % radix].ToString() + s; i /= radix; }
			return s;
		}

		public static int FromRadix(string s, int radix) {
			int i = 0;
			while (s.Length > 0) { i = i * radix + Array.IndexOf(Number.Hex, Char.ToUpper(s[0])); s = s.Remove(0, 1); }
			return i;
		}
	}
}