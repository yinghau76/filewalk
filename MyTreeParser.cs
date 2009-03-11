// $ANTLR 2.7.5 (20050128): "bexp.g" -> "MyTreeParser.cs"$

namespace FileWalk.Schema
{
	// Generate header specific to the tree-parser CSharp file
	using System;
	
	using TreeParser = antlr.TreeParser;
	using Token                    = antlr.Token;
	using IToken                   = antlr.IToken;
	using AST                      = antlr.collections.AST;
	using RecognitionException     = antlr.RecognitionException;
	using ANTLRException           = antlr.ANTLRException;
	using NoViableAltException     = antlr.NoViableAltException;
	using MismatchedTokenException = antlr.MismatchedTokenException;
	using SemanticException        = antlr.SemanticException;
	using BitSet                   = antlr.collections.impl.BitSet;
	using ASTPair                  = antlr.ASTPair;
	using ASTFactory               = antlr.ASTFactory;
	using ASTArray                 = antlr.collections.impl.ASTArray;
	
	
	internal 	class MyTreeParser : antlr.TreeParser
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int LITERAL = 4;
		public const int QLITERAL = 5;
		public const int ESC = 6;
		public const int NEWLINE = 7;
		public const int WS = 8;
		public const int VAR = 9;
		public const int LPAREN = 10;
		public const int RPAREN = 11;
		public const int OR = 12;
		public const int AND = 13;
		public const int EQUAL = 14;
		public const int ASSIGN = 15;
		public const int NOT_EQUAL = 16;
		public const int NOT = 17;
		public const int LT = 18;
		public const int LE = 19;
		public const int GE = 20;
		public const int GT = 21;
		
		public MyTreeParser()
		{
			tokenNames = tokenNames_;
		}
		
	public  String  literal(AST _t) //throws RecognitionException
{
		 String s ;
		
		AST literal_AST_in = (AST)_t;
		AST l = null;
		AST ql = null;
		
		s = null;
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case LITERAL:
			{
				l = _t;
				match(_t,LITERAL);
				_t = _t.getNextSibling();
				
				s = l.getText();
				
				break;
			}
			case QLITERAL:
			{
				ql = _t;
				match(_t,QLITERAL);
				_t = _t.getNextSibling();
				
				s = ql.getText();
				
				break;
			}
			default:
			{
				throw new NoViableAltException(_t);
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return s ;
	}
	
	public  String  value(AST _t) //throws RecognitionException
{
		 String s ;
		
		AST value_AST_in = (AST)_t;
		AST l = null;
		AST ql = null;
		AST v = null;
		
		String variable = null;
		s = null;
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case LITERAL:
			{
				l = _t;
				match(_t,LITERAL);
				_t = _t.getNextSibling();
				
				s = l.getText();
				
				break;
			}
			case QLITERAL:
			{
				ql = _t;
				match(_t,QLITERAL);
				_t = _t.getNextSibling();
				
				s = ql.getText();
				
				break;
			}
			case VAR:
			{
				AST tmp18_AST_in = _t;
				match(_t,VAR);
				_t = _t.getNextSibling();
				AST tmp19_AST_in = _t;
				match(_t,LPAREN);
				_t = _t.getNextSibling();
				v = _t;
				match(_t,LITERAL);
				_t = _t.getNextSibling();
				AST tmp20_AST_in = _t;
				match(_t,RPAREN);
				_t = _t.getNextSibling();
				
				variable = v.getText();
				
				s = (String) environment.get(variable);
				
				if (s == null) {
				throw new RecognitionException("unrecognized variable " + variable);
				}
				
				break;
			}
			default:
			{
				throw new NoViableAltException(_t);
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return s ;
	}
	
	public  boolean  expression(AST _t) //throws RecognitionException
{
		 boolean e ;
		
		AST expression_AST_in = (AST)_t;
		
		boolean a, b;
		String l, r;
		
		e = false;
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case AND:
			{
				AST __t43 = _t;
				AST tmp21_AST_in = _t;
				match(_t,AND);
				_t = _t.getFirstChild();
				a=expression(_t);
				_t = retTree_;
				b=expression(_t);
				_t = retTree_;
				e = a && b;
				_t = __t43;
				_t = _t.getNextSibling();
				break;
			}
			case OR:
			{
				AST __t44 = _t;
				AST tmp22_AST_in = _t;
				match(_t,OR);
				_t = _t.getFirstChild();
				a=expression(_t);
				_t = retTree_;
				b=expression(_t);
				_t = retTree_;
				e = a || b;
				_t = __t44;
				_t = _t.getNextSibling();
				break;
			}
			case EQUAL:
			{
				AST __t45 = _t;
				AST tmp23_AST_in = _t;
				match(_t,EQUAL);
				_t = _t.getFirstChild();
				l=value(_t);
				_t = retTree_;
				r=value(_t);
				_t = retTree_;
				
				e = l.equals(r);
				
				_t = __t45;
				_t = _t.getNextSibling();
				break;
			}
			default:
			{
				throw new NoViableAltException(_t);
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return e ;
	}
	
	
	public static readonly string[] tokenNames_ = new string[] {
		@"""<0>""",
		@"""EOF""",
		@"""<2>""",
		@"""NULL_TREE_LOOKAHEAD""",
		@"""LITERAL""",
		@"""QLITERAL""",
		@"""ESC""",
		@"""NEWLINE""",
		@"""WS""",
		@"""VAR""",
		@"""LPAREN""",
		@"""RPAREN""",
		@"""OR""",
		@"""AND""",
		@"""EQUAL""",
		@"""ASSIGN""",
		@"""NOT_EQUAL""",
		@"""NOT""",
		@"""LT""",
		@"""LE""",
		@"""GE""",
		@"""GT"""
	};
	
}

}
