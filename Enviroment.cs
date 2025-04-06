public class Enviroment
{
 private Dictionary<string,object> values = new Dictionary<string,object>();
 public object get (Token name)
 {
    if(values.ContainsKey(name.writing))return values[name.writing];
    throw new RuntimeError(name,"Undefined variable ' "+ name.writing + "'.");
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
    throw new RuntimeError(name,"undefined variable '" + name.writing + "'.");
 }
}