using System.Text.RegularExpressions;
using Microsoft.CSharp.RuntimeBinder;
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
  public Expresion parse()
  {
  try{
    return Expresion();
  }catch (ParseError error){
    return null;
    }
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
    return Equality();
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
      expresion = new Expresion.Binary(expresion, Operator, right);
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
      expresion = new Expresion.Binary(expresion, Operator, right);
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
      expresion = new Expresion.Binary(expresion, Operator, right);
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
      expresion = new Expresion.Binary(expresion, Operator, right);
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
      return new Expresion.Unary(Operator, right);
    }
    return primary();
  }
  private Expresion primary()
  {
    List<TokenTypes> types = new List<TokenTypes>()
    {
    TokenTypes.STRING,TokenTypes.NUMBER,
    };
    if (match(types)) return new Expresion.Literal(tokens[current - 1].literal);
    List<TokenTypes> typesparen = new List<TokenTypes>()
    {
    TokenTypes.LEFT_PAREN,
    };
    if (match(typesparen))
    {
      Expresion expresion = Expresion();
      consume(TokenTypes.RIGHT_PAREN, "Expect ')' after expression");
      return new Expresion.Grouping(expresion);
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