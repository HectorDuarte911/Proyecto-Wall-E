/// <summary>
/// Types of tokens
/// </summary>
public enum TokenTypes
{
  //Simbols
  LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE, COMMA, ASSIGNED, SEMICOLON,
 //Operations
 PLUS, MINUS, PRODUCT, POW, MODUL, DIVIDE,
 //Booleans expresions
 AND, OR, LESS, LESS_EQUAL, GREATER, GREATER_EQUAL, EQUAL_EQUAL, BANG, BANG_EQUAL,
 //Lierals
 IDENTIFIER, STRING, NUMBER, LABEL, EOF
}