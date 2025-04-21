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
    Literal? x = primary() as Literal;
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal? y = primary() as Literal;
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new Spawn(x!,y!);
  }
  private Stmt SizeStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'Size' ");
    Literal? size = primary() as Literal;
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new Size(size!);
  }
  private Stmt ColorStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'Color' ");
    Literal? color = primary() as Literal;
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new Color(color!);
  }
 private Stmt DrawLineStatement()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'DrawLine' ");
    object dirx = unary();
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    object diry = unary();
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal distance = IntCompFunction(new Literal(" "));
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
    Literal radius = IntCompFunction(new Literal(" "));
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
    Literal distance = IntCompFunction(new Literal(" "));
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal width= IntCompFunction(new Literal(" "));
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal height = IntCompFunction(new Literal(" "));
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
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'GetActualX' ");
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new GetActualX();
  }
  private Expresion GetActualY()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'GetActualY' ");
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new GetActualY();
  }
  private Expresion IsBrushColor()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'IsBrushColor' ");
    Literal? color = new Literal(" ");
    if(IsFun(tokens[current]))errors.Add(new Error(tokens[current].line,"This function can't resive anotherr function like a 'string' paramater"));
    else color = primary() as Literal;
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new IsBrushColor(color!);
  }
  private Expresion IsBrushSize()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'IsBrushSize' ");
    Literal size = IntCompFunction(new Literal(" "));
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new IsBrushSize(size);
  }
  private Expresion IsCanvasColor()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'IsCanvasColor' ");
    Literal? color = new Literal(" ");
    if(IsFun(tokens[current]))errors.Add(new Error(tokens[current].line,"This function can't resive another function like a 'string' paramater"));
    else color = primary() as Literal;
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal vertical = IntCompFunction(new Literal(" "));
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal  horizontal = IntCompFunction(new Literal(" "));
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new IsCanvasColor(color!,vertical,horizontal);
  }
  private Expresion GetColorCount()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'GetColorCount' ");
    Literal? color = new Literal(" ");
    if(IsFun(tokens[current]))errors.Add(new Error(tokens[current].line,"This function can't resive another function like a 'string' paramater"));
    else color = primary() as Literal;
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal x1 = IntCompFunction(new Literal(" "));
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal y1 = IntCompFunction(new Literal(" "));
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal x2 = IntCompFunction(new Literal(" "));
    consume(TokenTypes.COMMA,"Expected ',' between the variables");
    Literal y2 = IntCompFunction(new Literal(" "));
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new GetColorCount(color!,x1,y1,x2,y2);
  }
  private Expresion GetCanvasSize()
  {
    consume(TokenTypes.LEFT_PAREN,"Expected '(' after a 'GetCanvasSize' ");
    consume(TokenTypes.RIGHT_PAREN,"Expected ')' in the end of function");
    return new GetCanvasSize();
  }
  private Expresion assignment()
  {
    Expresion expresion = or();
    List<TokenTypes>type = new List<TokenTypes>(){TokenTypes.ASSIGNED};
    if(match(type))
    {
      Token assing = tokens[current - 1];
      int number = current;
      Expresion value = assignment();
      if(tokens[current -  1].type == TokenTypes.NUMBER  || IsFun(tokens[number]) || tokens[number].type == TokenTypes.NUMBER || tokens[number].type == TokenTypes.FALSE || tokens[number].type == TokenTypes.TRUE )
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
    return new Variable(new Token(TokenTypes.SEMICOLON ," ",null!, 0));

  }
  private Expresion primary()
  {
    List<TokenTypes> types = new List<TokenTypes>(){TokenTypes.STRING,TokenTypes.NUMBER,TokenTypes.FALSE,TokenTypes.TRUE};
    if(match(types)){
      if(tokens[current - 1].type != TokenTypes.STRING || StringRelated())return new Literal(tokens[current - 1].literal);
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
    types.Remove(TokenTypes.GETACTUALX);types.Add(TokenTypes.GETACTUALY);
    if(match(types))return GetActualY();
    else{
    types.Remove(TokenTypes.GETACTUALY);types.Add(TokenTypes.ISBRUSHCOLOR);
    if(match(types))return IsBrushColor();
    else{
    types.Remove(TokenTypes.ISBRUSHCOLOR);types.Add(TokenTypes.ISBRUSHSIZE);
    if(match(types))return IsBrushSize();
    else{
    types.Remove(TokenTypes.ISBRUSHSIZE);types.Add(TokenTypes.ISCANVASCOLOR);
    if(match(types))return IsCanvasColor();  
    else{
    types.Remove(TokenTypes.ISCANVASCOLOR);types.Add(TokenTypes.GETCOLORCOUNT);
    if(match(types))return GetColorCount();  
    else{
    types.Remove(TokenTypes.GETCOLORCOUNT);types.Add(TokenTypes.GETCANVASSIZE);
    if(match(types))return GetCanvasSize();   
    }}}}}}}}}
    errors.Add(new Error(1, "Expect an expresion"));
    return new Variable(new Token(TokenTypes.SEMICOLON ," ",null!, 0));
  }
  private Token consume(TokenTypes type, string message)
  {
    if(current < tokens.Count)
    {
      if (check(type)) return advance();
    }
    errors.Add(new Error (1, message));
    return new Token(TokenTypes.SEMICOLON ," ",null!, 0);
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
  private bool IsFun(Token token)
  {
   switch(token.type)
   {
    case TokenTypes.ISBRUSHCOLOR:
    case TokenTypes.ISBRUSHSIZE:
    case TokenTypes.ISCANVASCOLOR:
    case TokenTypes.GETCOLORCOUNT:
    case TokenTypes.GETCANVASSIZE:
    case TokenTypes.GETACTUALX:
    case TokenTypes.GETACTUALY:return true;
    default:
    return false;
   }
  }
  private bool StringRelated() 
  {
    foreach(Token token in tokens)
    {
      if(token.type == TokenTypes.COLOR)return true;
      if(token.type == TokenTypes.ISBRUSHCOLOR)return true;
      if(token.type == TokenTypes.GETCOLORCOUNT)return true;
      if(token.type == TokenTypes.ISCANVASCOLOR)return true;
    }
    return false;
  }
  private bool IsIntFunction(Token token)
  {
    switch (token.type)
    {
      case TokenTypes.GETACTUALX:
      case TokenTypes.GETACTUALY:
      case TokenTypes.GETCANVASSIZE:
      case TokenTypes.GETCOLORCOUNT:return true;
      default:return false;
    }
  }
  private Literal IntCompFunction(Literal? token)
  {
    if(IsFun(tokens[current]))
    {
     if(!IsIntFunction(tokens[current]))errors.Add(new Error(tokens[current].line,"This function can't resive a no 'int' return's function as paramater"));
    }
    else token = primary() as Literal;
    return token!;
  }
}