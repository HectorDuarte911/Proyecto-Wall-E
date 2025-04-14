using System.Text.RegularExpressions;

/// <summary>
/// Inspect the sintasix of a line
/// </summary>
public class Parser
{
  /// <summary>
  /// Error type identificator
  /// </summary>
  public List<Error> errors{get;private set;}
  /// <summary>
  /// tokens inspectid
  /// </summary>
  private List<Token> tokens;
  /// <summary>
  /// current token inspected
  /// </summary>
  private int current = 0;
  public Parser(List<Token> tokens,List<Error>errors)
  {
    this.tokens = tokens;
    this.errors = errors;
  }
  /// <summary>
  /// Ejecute the parsing
  /// </summary>
  /// <returns>Expresion parsing</returns>
  public List<Stmt> parse()
  {
    List<Stmt> statementslist = new List<Stmt>();
    while(!EOF())
    {
    statementslist.Add(statement());
    }
    return statementslist;
  }
  private Stmt statement()
 {
  List<TokenTypes> matchlist = new List<TokenTypes>(){TokenTypes.GOTO};
  if(match(matchlist))return GoToStatement();
  matchlist.Remove(TokenTypes.GOTO);matchlist.Add(TokenTypes.LABEL);
  if(match(matchlist))return Label(false);
  return expressionStatements();
 }
  private Stmt GoToStatement()
  {
    consume(TokenTypes.LEFT_BRACE,"Expected '[' after 'GoTo' .");
    Stmt label = Label(true);
    consume(TokenTypes.RIGHT_BRACE,"Expected ']' after the label .");
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after label .");
    Expresion condition = assignment();
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' after the condition .");
    return new GoTo(condition,label as Label);
  }
  private Stmt expressionStatements()
  {
    Expresion expresion = assignment();
    return  new Expression(expresion);
  }
  private Stmt Label(bool flag)
  {
    if(flag && !LabelSearch())errors.Add(new Error(tokens[current].line,"The label that is reference don't exist"));  
    Token tag = consume(TokenTypes.LABEL,"Expect a label");
    if(!flag && (tokens[current + 1].line == tokens[current].line || tokens[current - 1].line == tokens[current].line ))
    errors.Add (new Error(tokens[current].line,"A label can't be before and expresion or statement in the same line"));
    return new Label(tag);
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
      errors.Add(new Error(assing.line,"Invalid assignment target"));
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
    List<TokenTypes> types = new List<TokenTypes>()
    {
    TokenTypes.BANG,TokenTypes.MINUS,TokenTypes.POW
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
    types.Remove(TokenTypes.STRING);types.Remove(TokenTypes.NUMBER);types.Add(TokenTypes.IDENTIFIER);
    if(match(types))return new Variable(tokens[current - 1]);
    types.Remove(TokenTypes.IDENTIFIER);types.Add(TokenTypes.LEFT_PAREN);
    if (match(types)){
      Expresion expresion = assignment();
      consume(TokenTypes.RIGHT_PAREN, "Expect ')' after expresion");
      return new Grouping(expresion);
    }
    errors.Add(new Error(tokens[current].line, "Expect expresion"));
    throw new Exception("Espera@");
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
    errors.Add(new Error (tokens[current].line, message));
    throw new Exception("Espera");
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
    if (EOF()) return false;
    return tokens[current].type == type;
  }
  /// <summary>
  /// Advance one in the line  
  /// </summary>
  /// <returns>The current token before this implementation</returns>
  private Token advance()
  {
    if (!EOF()) current++;
    return tokens[current - 1];
  }
   private bool EOF()
  {
    return current >= tokens.Count ;
  }
  private bool LabelSearch()
  {
    foreach (Label label in Lexical.labels)
    {
      if(new Label(tokens[current]) == label)return true;
    }
    return false;
  }
}