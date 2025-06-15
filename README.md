This is a windows application to a pixel art by mean of comands in a text editor named Pixel-Walle

📃**Requisites**
+ Have installed .Net 8.0.
+ Add the Avalonias package(in the worst case have a lot of patience to install all the features of avalonia)

🎰**How running the project?**

Go to the direction *PixelWallE\bin\Release\net9.0* and open the application PixelWallE

💻**How is the project inside?**
 
The project have three parts:

+ **Paint**: Have the status of the canvas, the brush and the cursor and all the function to the pixel art

+ **Avalonia**: Implement the visual representation of the project and have a:
+  Canvas
+  Text Editor
+  Errors show window
+  Ejecution button
+  Save and load buttons
+  Text box to change the dimension of the canvas

+ **Interpreter** than with the text editor writing ejecute the three parts to comprove is the introduced text is a valid one:
+ *Lexical*: Scann the text and convet it in tokens and detect semantics errors
+ *Parser*: Transform the tokens in expresions and in statements, and detect sintaxis errors
+ *Interpret*: Ejecute the statements and detected runtime errors

⌨️**How is the project's flow?**

The text editor detected the text, then the interpret ejecute the code, change the paint status and the visual state 

💾**Valible code**

+ *Functions*:
+ Spawn
+ Size
+ Color
+ DrawLine
+ DrawCircle
+ DrawRectangle
+ Fill

+ *Labels* and *GoTo* function

+ *Varibles Assignation*
+ Arithmetic expresions
+ Booleans expresions
+ Strings of valibles colors
+ GetActualX function
+ GetActualY function
+ GetColorCount function
+ GetCanvasSize function
+ IsBrushColor function
+ IsBrushSize function
+ IsCanvasColor function

🧩**Future additions**

If the change if in the operation you only need to now what is the precedence of the operation in compare of the others and if you need to add a new 
function you only need to create the logic in the paint block and added the differents reference in the interpreter.
