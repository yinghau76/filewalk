options
{
    language  =  "CSharp";
    namespace =  "FileWalk.Schema";
}

class BoolExprLexer extends Lexer;
options 
{
     k=2;
     charVocabulary='\u0000'..'\uFFFE';
     classHeaderPrefix = "internal";
}

LITERAL:
  ('a'..'z'|'A'..'Z'|'0'..'9'|'.'|'_')+
  ;

QLITERAL:
  '\''! (ESC | ~('\\'|'"'))* '\''!
  ;

protected
ESC:
  '\\' ('\\'|'t'|'n'|'r'|'"') ;
  
NEWLINE : ('\r''\n') => '\r''\n' //DOS
        | '\r'                   //MAC
        | '\n'                   //UNIX
        { newline(); }
        ;
WS      : (' '|'\t') { $setType(Token.SKIP); } ;

VAR       : '$'  ;
LPAREN    : '('  ;
RPAREN    : ')'  ;
OR        : "||" ;
AND       : "&&" ;
EQUAL     : "==" ;
ASSIGN    : "="  ;
NOT_EQUAL : "!=" ;
NOT       : '!'  ;
LESSTHAN  : '<'  ;
LESSEQUAL : "<=" ;
GE        : ">=" ;
GT        : '>'  ;

class BoolExprParser extends Parser;
options 
{
     k=4;
     buildAST = true;
     classHeaderPrefix = "internal";
}

variable
  : VAR LPAREN LITERAL RPAREN
  ;

value
  : LITERAL
  | QLITERAL
  | variable 
  ;

expression
  : expression1 (OR^ expression1)*
  ;

expression1
  : expression2 (AND^ expression2)*
  ;

expression2
  : expression3 ((EQUAL^ | NOT_EQUAL^ | LESSTHAN^ | LESSEQUAL^ | GE^ | GT^) expression3)*
  ;

expression3
  : value
  | LPAREN! expression RPAREN!
  | NOT^ expression3
  ;
  
/////////////////////////////////////////////////////////////////////////////
// ConditionEvaluator to evaluate a specified boolean expression

class ConditionEvaluator extends TreeParser;
options
{
	classHeaderPrefix = "internal";
}
{
		private FileWalk.Schema.ElementInstance _inst;
		public FileWalk.Schema.ElementInstance Instance
		{
		    get { return _inst; }
		    set { _inst = value; }
		}
}

literal returns [ string s ]
  {
      s = null;
  }
  : l:LITERAL
    {
       s = l.getText();
    }
  | ql:QLITERAL
    {
       s = ql.getText();
    }
  ;
  
value returns [object obj]
  {
     obj = null;
  }
  : l:LITERAL
    {
       obj = Convert.ToInt32(l.getText());
    }
  | ql:QLITERAL
    {
       obj = ql.getText();
    }
  | VAR! LPAREN! v:LITERAL RPAREN!
    {
       string var = v.getText();
 
       obj = _inst.Eval(var);
 
       if (obj == null) {
          throw new RecognitionException("unrecognized variable " + var);
       }
    }
  ;
  
expression returns [ bool e ]
  {
     bool a, b;
     object l, r;
 
     e = false;
  }
  : #(AND a=expression b=expression { e = a && b; } )
  | #(OR  a=expression b=expression { e = a || b; } )
  | #(EQUAL l=value r=value
       {
          e = ((l as IComparable).CompareTo(r) == 0);
       }
     )
  | #(NOT_EQUAL l=value r=value
       {
          e = ((l as IComparable).CompareTo(r) != 0);
       }
     )
  | #(LESSTHAN l=value r=value
       {
          e = ((l as IComparable).CompareTo(r) < 0);
       }
     )
  | #(LESSEQUAL l=value r=value
       {
          e = ((l as IComparable).CompareTo(r) <= 0);
       }
     )
  | #(GE l=value r=value
       {
          e = ((l as IComparable).CompareTo(r) >= 0);
       }
     )
  | #(GT l=value r=value
       {
          e = ((l as IComparable).CompareTo(r) > 0);
       }
     )
  ;
