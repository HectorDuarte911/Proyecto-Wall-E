namespace WALLE;
public class Parser
{
  public List<Error> errors{get;private set;}
  private List<Token> tokens;
  private int current = 0;
  public Parser(List<Token> tokens,List<Error>errors)
  {
    this.tokens = tokens;
    this.errors = errors;
  }
  public List<Stmt> parse()
  {
    List<Stmt> statementslist = new List<Stmt>();
    while(!EOF())
    {
    statementslist.Add(statement());
    if(errors.Count > 0)break;
    }
    return statementslist;
  }
  private Stmt statement()
 {
  List<TokenTypes> matchlist = new List<TokenTypes>(){TokenTypes.GOTO};
  if(match(matchlist))return GoToStatement();
  else{
  matchlist.Remove(TokenTypes.GOTO);matchlist.Add(TokenTypes.LABEL);
  if(match(matchlist))return Label(false);
  else{
  matchlist.Remove(TokenTypes.LABEL);matchlist.Add(TokenTypes.SPAWN);
  if(match(matchlist))return SpawnStatement();
  else{
  matchlist.Remove(TokenTypes.SPAWN);matchlist.Add(TokenTypes.SIZE);
  if(match(matchlist))return SizeStatement();
  else{
  matchlist.Remove(TokenTypes.SIZE);matchlist.Add(TokenTypes.COLOR);
  if(match(matchlist))return ColorStatement();
  else{
  matchlist.Remove(TokenTypes.COLOR);matchlist.Add(TokenTypes.DRAWLINE);
  if(match(matchlist))return DrawLineStatement();  
  else{
  matchlist.Remove(TokenTypes.DRAWLINE);matchlist.Add(TokenTypes.DRAWCIRCLE);
  if(match(matchlist))return DrawCircleStatement();
  else{
  matchlist.Remove(TokenTypes.DRAWCIRCLE);matchlist.Add(TokenTypes.DRAWRECTANGLE);
  if(match(matchlist))return DrawRectangleStatement();  
  else{
  matchlist.Remove(TokenTypes.DRAWRECTANGLE);matchlist.Add(TokenTypes.FILL);
  if(match(matchlist))return FillStatement(); 
  else return expressionStatements();
  }}}}}}}}
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
  private Stmt SpawnStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'Spaw' ");
    Literal x = primary() as Literal;
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal y = primary() as Literal;
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new Spawn(x,y);
  }
  private Stmt SizeStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'Size' ");
    Literal size = primary() as Literal;
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new Size(size);
  }
  private Stmt ColorStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'Color' ");
    Literal color = primary() as Literal;
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new Color(color);
  }
 private Stmt DrawLineStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'DrawLine' ");
    object dirx = unary();
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    object diry = unary();
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal distance = primary() as Literal;
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new DrawLine(dirx,diry,distance);
  }
  private Stmt DrawCircleStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'DrawCircle' ");
    object dirx = unary();
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    object diry = unary() ;
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal radius = primary() as Literal;
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new DrawCircle(dirx,diry,radius);
  }
  private Stmt DrawRectangleStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'DrawRectangle' ");
    object dirx = unary();
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    object diry = unary();
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal distance = primary() as Literal;
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal width = primary() as Literal;
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal height = primary() as Literal;
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new DrawRectangle(dirx,diry,distance,width,height);
  }
  private Stmt FillStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'Fill' ");
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new Fill();
  }
  private Stmt expressionStatements()
  {
    Expresion expresion = assignment();
    return  new Expression(expresion);
  }
  private Stmt Label(bool flag)
  {
    if(flag && !LabelSearch())errors.Add(new Error(tokens[current].line,"The label that is reference don't exist"));
    Token tag = advance();
    if(!flag && tokens.Count != 1)
    errors.Add (new Error(tokens[current - 1].line,"A label can't be before an expresion or statement in the same line"));
    return new Label(tag);
  }
  private Expresion GetActualX()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'GetACtualX' ");
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new GetActualX();
  }
  private Expresion GetActualY()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'GetACtualY' ");
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new GetActualY();
  }
  private Expresion assignment()
  {
    Expresion expresion = or();
    List<TokenTypes>type = new List<TokenTypes>(){TokenTypes.ASSIGNED};
    if(match(type))
    {
      Token assing = tokens[current - 1];
      Expresion value = assignment();
        if(tokens[current -  1].type == TokenTypes.NUMBER  || (current > 3 && IsFun(tokens[current -  3])) )
        {
          if(expresion is Variable variable)
         {
          Token? name = variable.name;
          return new Assign(name , value);
         }
        }
          
      errors.Add(new Error(assing.line,"Invalid assignment target"));
    }
    return expresion; 
  }
  public Expresion or()
  {
    Expresion expresion = and();
    List<TokenTypes>type = new List<TokenTypes>(){TokenTypes.OR};
    while(match(type))
    {
      Token Operator = tokens[current -1];
      if(current  != tokens.Count)
      { 
      Expresion right = and();
      expresion = new Logical(expresion,Operator,right);
      }
      else errors.Add(new Error(tokens[current - 1].line, "Expect an expresion"));
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
      if(current  != tokens.Count)
      {  
      Expresion right  = Equality();
      expresion = new Logical(expresion , Operator,right);
      }
      else errors.Add(new Error(tokens[current - 1].line, "Expect an expresion"));
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
      if(current  != tokens.Count)
      {  
      Expresion right = comparison();
      expresion = new Binary(expresion, Operator, right);
      }
      else errors.Add(new Error(tokens[current - 1].line, "Expect an expresion"));
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
      if(current  != tokens.Count)
      {    
       Expresion right = term();
      expresion = new Binary(expresion, Operator, right);
      }
      else errors.Add(new Error(tokens[current - 1].line, "Expect an expresion"));
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
       if(current  != tokens.Count)
      {    
      Expresion right = factor();
      expresion = new Binary(expresion, Operator, right);
      }
      else errors.Add(new Error(tokens[current - 1].line, "Expect an expresion"));
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
       if(current  != tokens.Count)
      {    
      Expresion right = unary();
      expresion = new Binary(expresion, Operator, right);
      }
      else errors.Add(new Error(tokens[current - 1].line, "Expect an expresion"));
    }
    return expresion;
  }
  private Expresion unary()
  {
    List<TokenTypes> types = new List<TokenTypes>()
    {
    TokenTypes.BANG,TokenTypes.POW,TokenTypes.MINUS
    };
    if (match(types))
    {
      Token Operator = tokens[current - 1];
      Expresion right = unary();
      return new Unary(Operator, right);
    }
    if(current  != tokens.Count)return primary();
    errors.Add(new Error(tokens[current - 1].line, "Expect an expresion"));
    return new Variable(new Token(TokenTypes.SEMICOLON ," ",null, 0));

  }
  private Expresion primary()
  {
    List<TokenTypes> types = new List<TokenTypes>(){TokenTypes.STRING,TokenTypes.NUMBER,};
    if(match(types)){
      if(tokens[current - 1].type != TokenTypes.STRING)return new Literal(tokens[current - 1].literal);
    }
    else{
    types.Remove(TokenTypes.STRING);types.Remove(TokenTypes.NUMBER);types.Add(TokenTypes.IDENTIFIER);
    if(match(types))return new Variable(tokens[current - 1]);
    else{
    types.Remove(TokenTypes.IDENTIFIER);types.Add(TokenTypes.LEFT_PAREN);
    if (match(types)){
      Expresion expresion = assignment();
      if(current > 0){
        if(tokens[current-1].type == TokenTypes.LABEL)errors.Add(new Error(tokens[current - 1].line, "Expect an expresion"));
      }
      if(errors.Count == 0 ){
      consume(TokenTypes.RIGHT_PAREN, "Expect ')' after expresion");
      return new Grouping(expresion);
      }
    }
    else{
    types.Remove(TokenTypes.LEFT_PAREN);types.Add(TokenTypes.GETACTUALX);
    if(match(types))return GetActualX();
    else{
    types.Remove(TokenTypes.LEFT_PAREN);types.Add(TokenTypes.GETACTUALY);
    if(match(types))return GetActualY();
    }}}}
    errors.Add(new Error(1, "Expect an expresion"));
    return new Variable(new Token(TokenTypes.SEMICOLON ," ",null, 0));
  }
  private Token consume(TokenTypes type, string message)
  {
    if(current < tokens.Count)
    {
      if (check(type)) return advance();
    }
    errors.Add(new Error (1, message));
    return new Token(TokenTypes.SEMICOLON ," ",null, 0);
  }
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
  private bool check(TokenTypes type)
  {
    if (EOF()) return false;
    return tokens[current].type == type;
  }
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
  private bool IsSign(TokenTypes type)
  {
    switch(type)
    {
      case TokenTypes.PLUS: return true;
      case TokenTypes.MINUS: return true;
      case TokenTypes.PRODUCT: return true;
      case TokenTypes.MODUL: return true;
      case TokenTypes.DIVIDE: return true;
      case TokenTypes.LESS_EQUAL: return true;
      case TokenTypes.LESS: return true;
      case TokenTypes.GREATER: return true;
      case TokenTypes.GREATER_EQUAL: return true;
      case TokenTypes.BANG_EQUAL: return true;
      default:return false;
    }
  }
  private bool IsBool(TokenTypes type)
  {
    return type == TokenTypes.AND || type == TokenTypes.OR;
  }
  private bool IsFun(Token token)
  {
   switch(token.type)
   {
    case TokenTypes.GETACTUALX:
    case TokenTypes.GETACTUALY:return true;
    default:
    return false;
   }
  } 
}