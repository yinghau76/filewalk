// $ANTLR 2.7.5 (20050128): "boolexpr.g" -> "BoolExprParser.cs"$

namespace FileWalk.Schema
{
	// Generate the header common to all output files.
	using System;
	
	using TokenBuffer              = antlr.TokenBuffer;
	using TokenStreamException     = antlr.TokenStreamException;
	using TokenStreamIOException   = antlr.TokenStreamIOException;
	using ANTLRException           = antlr.ANTLRException;
	using LLkParser = antlr.LLkParser;
	using Token                    = antlr.Token;
	using IToken                   = antlr.IToken;
	using TokenStream              = antlr.TokenStream;
	using RecognitionException     = antlr.RecognitionException;
	using NoViableAltException     = antlr.NoViableAltException;
	using MismatchedTokenException = antlr.MismatchedTokenException;
	using SemanticException        = antlr.SemanticException;
	using ParserSharedInputState   = antlr.ParserSharedInputState;
	using BitSet                   = antlr.collections.impl.BitSet;
	using AST                      = antlr.collections.AST;
	using ASTPair                  = antlr.ASTPair;
	using ASTFactory               = antlr.ASTFactory;
	using ASTArray                 = antlr.collections.impl.ASTArray;
	
	internal 	class BoolExprParser : antlr.LLkParser
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
		public const int LESSTHAN = 18;
		public const int LESSEQUAL = 19;
		public const int GE = 20;
		public const int GT = 21;
		
		
		protected void initialize()
		{
			tokenNames = tokenNames_;
			initializeFactory();
		}
		
		
		protected BoolExprParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
		{
			initialize();
		}
		
		public BoolExprParser(TokenBuffer tokenBuf) : this(tokenBuf,4)
		{
		}
		
		protected BoolExprParser(TokenStream lexer, int k) : base(lexer,k)
		{
			initialize();
		}
		
		public BoolExprParser(TokenStream lexer) : this(lexer,4)
		{
		}
		
		public BoolExprParser(ParserSharedInputState state) : base(state,4)
		{
			initialize();
		}
		
	public void variable() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		AST variable_AST = null;
		
		try {      // for error handling
			AST tmp1_AST = null;
			tmp1_AST = astFactory.create(LT(1));
			astFactory.addASTChild(currentAST, tmp1_AST);
			match(VAR);
			AST tmp2_AST = null;
			tmp2_AST = astFactory.create(LT(1));
			astFactory.addASTChild(currentAST, tmp2_AST);
			match(LPAREN);
			AST tmp3_AST = null;
			tmp3_AST = astFactory.create(LT(1));
			astFactory.addASTChild(currentAST, tmp3_AST);
			match(LITERAL);
			AST tmp4_AST = null;
			tmp4_AST = astFactory.create(LT(1));
			astFactory.addASTChild(currentAST, tmp4_AST);
			match(RPAREN);
			variable_AST = currentAST.root;
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_0_);
		}
		returnAST = variable_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void value() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		AST value_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LITERAL:
			{
				AST tmp5_AST = null;
				tmp5_AST = astFactory.create(LT(1));
				astFactory.addASTChild(currentAST, tmp5_AST);
				match(LITERAL);
				value_AST = currentAST.root;
				break;
			}
			case QLITERAL:
			{
				AST tmp6_AST = null;
				tmp6_AST = astFactory.create(LT(1));
				astFactory.addASTChild(currentAST, tmp6_AST);
				match(QLITERAL);
				value_AST = currentAST.root;
				break;
			}
			case VAR:
			{
				variable();
				astFactory.addASTChild(currentAST, returnAST);
				value_AST = currentAST.root;
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_0_);
		}
		returnAST = value_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void expression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		AST expression_AST = null;
		
		try {      // for error handling
			expression1();
			astFactory.addASTChild(currentAST, returnAST);
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==OR))
					{
						AST tmp7_AST = null;
						tmp7_AST = astFactory.create(LT(1));
						astFactory.makeASTRoot(currentAST, tmp7_AST);
						match(OR);
						expression1();
						astFactory.addASTChild(currentAST, returnAST);
					}
					else
					{
						goto _loop32_breakloop;
					}
					
				}
_loop32_breakloop:				;
			}    // ( ... )*
			expression_AST = currentAST.root;
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_1_);
		}
		returnAST = expression_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void expression1() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		AST expression1_AST = null;
		
		try {      // for error handling
			expression2();
			astFactory.addASTChild(currentAST, returnAST);
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==AND))
					{
						AST tmp8_AST = null;
						tmp8_AST = astFactory.create(LT(1));
						astFactory.makeASTRoot(currentAST, tmp8_AST);
						match(AND);
						expression2();
						astFactory.addASTChild(currentAST, returnAST);
					}
					else
					{
						goto _loop35_breakloop;
					}
					
				}
_loop35_breakloop:				;
			}    // ( ... )*
			expression1_AST = currentAST.root;
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_2_);
		}
		returnAST = expression1_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void expression2() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		AST expression2_AST = null;
		
		try {      // for error handling
			expression3();
			astFactory.addASTChild(currentAST, returnAST);
			{    // ( ... )*
				for (;;)
				{
					if ((tokenSet_3_.member(LA(1))))
					{
						{
							switch ( LA(1) )
							{
							case EQUAL:
							{
								AST tmp9_AST = null;
								tmp9_AST = astFactory.create(LT(1));
								astFactory.makeASTRoot(currentAST, tmp9_AST);
								match(EQUAL);
								break;
							}
							case NOT_EQUAL:
							{
								AST tmp10_AST = null;
								tmp10_AST = astFactory.create(LT(1));
								astFactory.makeASTRoot(currentAST, tmp10_AST);
								match(NOT_EQUAL);
								break;
							}
							case LESSTHAN:
							{
								AST tmp11_AST = null;
								tmp11_AST = astFactory.create(LT(1));
								astFactory.makeASTRoot(currentAST, tmp11_AST);
								match(LESSTHAN);
								break;
							}
							case LESSEQUAL:
							{
								AST tmp12_AST = null;
								tmp12_AST = astFactory.create(LT(1));
								astFactory.makeASTRoot(currentAST, tmp12_AST);
								match(LESSEQUAL);
								break;
							}
							case GE:
							{
								AST tmp13_AST = null;
								tmp13_AST = astFactory.create(LT(1));
								astFactory.makeASTRoot(currentAST, tmp13_AST);
								match(GE);
								break;
							}
							case GT:
							{
								AST tmp14_AST = null;
								tmp14_AST = astFactory.create(LT(1));
								astFactory.makeASTRoot(currentAST, tmp14_AST);
								match(GT);
								break;
							}
							default:
							{
								throw new NoViableAltException(LT(1), getFilename());
							}
							 }
						}
						expression3();
						astFactory.addASTChild(currentAST, returnAST);
					}
					else
					{
						goto _loop39_breakloop;
					}
					
				}
_loop39_breakloop:				;
			}    // ( ... )*
			expression2_AST = currentAST.root;
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_4_);
		}
		returnAST = expression2_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void expression3() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		AST expression3_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LITERAL:
			case QLITERAL:
			case VAR:
			{
				value();
				astFactory.addASTChild(currentAST, returnAST);
				expression3_AST = currentAST.root;
				break;
			}
			case LPAREN:
			{
				match(LPAREN);
				expression();
				astFactory.addASTChild(currentAST, returnAST);
				match(RPAREN);
				expression3_AST = currentAST.root;
				break;
			}
			case NOT:
			{
				AST tmp17_AST = null;
				tmp17_AST = astFactory.create(LT(1));
				astFactory.makeASTRoot(currentAST, tmp17_AST);
				match(NOT);
				expression3();
				astFactory.addASTChild(currentAST, returnAST);
				expression3_AST = currentAST.root;
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_0_);
		}
		returnAST = expression3_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	private void initializeFactory()
	{
		if (astFactory == null)
		{
			astFactory = new ASTFactory();
		}
		initializeASTFactory( astFactory );
	}
	static public void initializeASTFactory( ASTFactory factory )
	{
		factory.setMaxNodeType(21);
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
		@"""LESSTHAN""",
		@"""LESSEQUAL""",
		@"""GE""",
		@"""GT"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 4028416L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 2048L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 6144L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 4014080L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 14336L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	
}
}
