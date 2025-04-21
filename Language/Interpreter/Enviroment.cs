namespace WALLE;
public class Enviroment
{
 private Dictionary<string,object> values = new Dictionary<string,object>();
 public List<Error> errors {get; private set;}
 public Enviroment(List<Error> errors)
 {
   this.errors = errors;
 }
 public object get (Token name)
 {
    if(values.ContainsKey(name.writing))return values[name.writing];
    errors.Add(new Error(name.line,"Undefined variable ' "+ name.writing + " '" ));
    return null!;
 }
 private void define(string name,object value)
 {
    values.Add(name,value);
 }
 public void assign(Token name , object value)
 {
    if(values.ContainsKey(name.writing))
    {
      values[name.writing] = value;return;
    }
    else define(name.writing,value);
 }
 //Auxiiar
 public void GetValues()
 {
   foreach(KeyValuePair<string,object> item in values )
   {
      Console.WriteLine(item.Value);
   }
 }
}