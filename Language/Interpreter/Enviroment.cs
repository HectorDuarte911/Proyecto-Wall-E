namespace WALLE;
/// <summary>
/// Determinate the state of the variables declarated
/// </summary>
public class Enviroment
{
   /// <summary>
   /// Put all the vvariables whith the value
   /// </summary>
   private Dictionary<string, object> values = new Dictionary<string, object>();
   /// <summary>
   /// Determinate the errors in the declaration or the value assigned proses
   /// </summary>
   public List<Error> errors { get; private set; }
   public Enviroment(List<Error> errors) => this.errors = errors;
   /// <summary>
   /// Get the value of the variable
   /// </summary>
   public object get(Token name)
   {
      if (values.ContainsKey(name.writing)) return values[name.writing];
      errors.Add(new Error(name.line, "Undefined variable ' " + name.writing + " '"));
      return null!;
   }
   /// <summary>
   /// Add a new variable to the enviroment
   /// </summary>
   private void define(string name, object value) => values.Add(name, value);
   /// <summary>
   /// Assign a new value to the variable in case of a old variable or define one in case of a new one 
   /// </summary>
   public void assign(Token name, object value)
   {
      if (values.ContainsKey(name.writing))
      {
         values[name.writing] = value; return;
      }
      else define(name.writing, value);
   }
}