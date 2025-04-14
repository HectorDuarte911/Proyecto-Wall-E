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
    errors.Add(new Error(name.line,"Undefined variable ' "+ name.writing + "'."));
    throw new Exception("Esperate#");
 }
 public void difine(string name,object value)
 {
    values.Add(name,value);
 }
 public void assign(Token name , object value)
 {
    if(values.ContainsKey(name.writing))
    {
        values.Add(name.writing,value);
        return;
    }
    errors.Add(new Error (name.line,"undefined variable '" + name.writing + "'."));
 }
}