/// <summary>
/// Inspect the sintasix of a line
/// </summary>
public class Parser
{
  /// <summary>
  /// Error type identificator
  /// </summary>
  private class ParseError : Exception { }
  /// <summary>
  /// tokens inspectid
  /// </summary>
  private List<Token> tokens;
  /// <summary>
  /// current token inspected
  /// </summary>
  private int current = 0;
  public Parser(List<Token> tokens)
  {
    this.tokens = tokens;
  }
  /// <summary>
  /// Ejecute the parsing
  /// </summary>
  /// <returns>Expresion parsing</returns>
  public List<Stmt> parse()
  {
    List<Stmt> statementslist = new List<Stmt>();
    while(!IsTheEnd())
    {
      statementslist.Add(statement());
    }
    return statementslist;
  }
  private Expresion assignment()
  {
    Expresion expresion = or();
    List<TokenTypes>type = new List<TokenTypes>(){TokenTypes.ASSIGNED};
    if(match(type))
    {
      Token assing = tokens[current - 1];
      Expresion value = assignment();
      if(expresion is Variable variable)
      {
        Token? name = variable.name;
        return new Assign(name! , value);
      }
      error(assing,"Invalid assignment target");
    }
    return expresion;
  }
  private Expresion or()
  {
    Expresion expresion = and();
    List<TokenTypes>type = new List<TokenTypes>(){TokenTypes.OR};
    while(match(type))
    {
      Token Operator = tokens[current -1];
      Expresion right = and();
      expresion = new Logical(expresion,Operator,right);
    }
    return expresion;
  }
  private Expresion and()
  {
    Expresion expresion = Equality();
    List<TokenTypes>type = new List<TokenTypes>(){TokenTypes.AND};
    while(match(type))
    {
      Token Operator = tokens[current - 1];
      Expresion right  = Equality();
      expresion = new Logical(expresion , Operator,right);
    }
    return expresion;
  }
 private Stmt statement()
 {
  List<TokenTypes> matchlist = new List<TokenTypes>(){TokenTypes.GOTO};
  if(match(matchlist))return GoToStatement();
  return expressionStatements();
 }
  private Stmt GoToStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after 'GoTo' .");
    Expresion condition = Expresion();
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' after the condition .");
    return new GoTo(condition);
  }
  private Stmt expressionStatements()
  {
    Expresion expresion = Expresion();
    return  new Expression(expresion);
  }
  /// <summary>
  /// See is the token in the current position is one of the types of the list
  /// </summary>
  /// <param name="types"></param>
  /// <returns></returns>
  private bool match(List<TokenTypes> types)
  {
    foreach (TokenTypes type in types)
    {
      if (check(type))
      {
        advance();
        return true;
      }
    }
    return false;
  }
  /// <summary>
  /// Comprove if the current token in the list is a token of the introduced type
  /// </summary>
  /// <param name="type"></param>
  /// <returns></returns>
  private bool check(TokenTypes type)
  {
    if (IsTheEnd()) return false;
    return tokens[current].type == type;
  }
  /// <summary>
  /// Advance one in the line  
  /// </summary>
  /// <returns>The current token before this implementation</returns>
  private Token advance()
  {
    if (!IsTheEnd()) current++;
    return tokens[current - 1];
  }
  /// <summary>
  /// Comprove if is the end of the line
  /// </summary>
  /// <returns></returns>
  private bool IsTheEnd()
  {
    return tokens[current].type == TokenTypes.EOF;
  }
  /// <summary>
  /// Directly comprove the order of the implementetion
  /// </summary>
  /// <returns></returns>
  private Expresion Expresion()
  {
    return assignment();
  }
  //This is the order of implimentestion than the Expesion method comprove is brute for time
  private Expresion Equality()
  {
    Expresion expresion = comparison();
    List<TokenTypes> types = new List<TokenTypes>()
         {
         TokenTypes.BANG_EQUAL,
         TokenTypes.EQUAL_EQUAL,
         };
    while (match(types))
    {
      Token Operator = tokens[current - 1];
      Expresion right = comparison();
      expresion = new Binary(expresion, Operator, right);
    }
    return expresion;
  }
  private Expresion comparison()
  {
    Expresion expresion = term();
    List<TokenTypes> types = new List<TokenTypes>()
    {
    TokenTypes.GREATER,TokenTypes.GREATER_EQUAL,
    TokenTypes.LESS,TokenTypes.LESS_EQUAL,
    };
    while (match(types))
    {
      Token Operator = tokens[current - 1];
      Expresion right = term();
      expresion = new Binary(expresion, Operator, right);
    }
    return expresion;
  }
  private Expresion term()
  {
    Expresion expresion = factor();
    List<TokenTypes> types = new List<TokenTypes>()
    {
    TokenTypes.PLUS,TokenTypes.MINUS
    };
    while (match(types))
    {
      Token Operator = tokens[current - 1];
      Expresion right = factor();
      expresion = new Binary(expresion, Operator, right);
    }
    return expresion;
  }
  private Expresion factor()
  {
    Expresion expresion = unary();
    List<TokenTypes> types = new List<TokenTypes>()
    {
    TokenTypes.PRODUCT,TokenTypes.MODUL,TokenTypes.DIVIDE
    };
    while (match(types))
    {
      Token Operator = tokens[current - 1];
      Expresion right = unary();
      expresion = new Binary(expresion, Operator, right);
    }
    return expresion;
  }
  private Expresion unary()
  {
    Expresion expresion = primary();
    List<TokenTypes> types = new List<TokenTypes>()
    {
    TokenTypes.BANG,TokenTypes.MINUS
    };
    if (match(types))
    {
      Token Operator = tokens[current - 1];
      Expresion right = unary();
      return new Unary(Operator, right);
    }
    return primary();
  }
  private Expresion primary()
  {
    List<TokenTypes> types = new List<TokenTypes>()
    {
    TokenTypes.STRING,TokenTypes.NUMBER,
    };
    if(match(types)) return new Literal(tokens[current - 1].literal);
    types.Remove(TokenTypes.STRING);
    types.Remove(TokenTypes.NUMBER);
    types.Add(TokenTypes.IDENTIFIER);
    if(match(types))return new Variable(tokens[current - 1]);
    types.Remove(TokenTypes.IDENTIFIER);
    types.Add(TokenTypes.LEFT_PAREN);
    if (match(types))
    {
      Expresion expresion = Expresion();
      consume(TokenTypes.RIGHT_PAREN, "Expect ')' after expression");
      return new Grouping(expresion);
    }
    throw error(tokens[current], "Expect expression");
  }
 //This is the end of these methods
 /// <summary>
 /// Comprove is the token is of the same type indroduced 
 /// </summary>
 /// <param name="type"></param>
 /// <param name="message">Message that is show if the type is not the same</param>
 /// <returns>throw an error if the type is not the same </returns>
  private Token consume(TokenTypes type, string message)
  {
    if (check(type)) return advance();
    throw error(tokens[current], message);
  }
  /// <summary>
  /// Detect the error in the parsing
  /// </summary>
  /// <param name="token"></param>
  /// <param name="message"></param>
  /// <returns></returns>
  private ParseError error(Token token, string message)
  {
    Language.error(token, message);
    return new ParseError();
  }
}