This is a windows develoment project of a pixel art by mean of commands in a text editor named Pixel-Walle :

📃**Requisites**

+Avalonias Packages(a good patience to install this package in the worst case)
+Net.8
🎰**How run the project?**

Go to the direction *PixelWallE\bin\Release\net9.0* and open the aplication named *PixeWallE* there you can ejecute the project

💻**How is the project flow?**

The project is diviide in three parts of funcionality:
 
 + **Interpret**:Is the logic interpret code and the must important part of the project , is divide in other three parts :
 
 + *Lexical*:Scanned the text introduced, transform it to diferents tokens and detected gramars errors
 + *Parser*:Go to all the scanned tokens and try to convert it to valid expresions and next to valid statements and detect the sintax errors in the proces
 + *Interpret*:Ejecute the statements detected in the *Parser* and detected the ejeutions errors

 + **Paint**:Is the class that contains the canvas of the colors and all the valid methods to the paint of the canvas

 + **Avalonia**:The visual representation of the pixel art , have a:
 
 + Text editor to enter commands to the interpret 
 + Visual canvas whith the cursor
 + Changed box of the dimensions of the canvas
 + Ejecution button to run the code
 + A save and load buttons to save a pixel art or load a save one
 + Autocompleted

 The visal part detected the text introduced in the text editor , then the interpret ejecute the function introduced than change th *Paint* part status, then
 all the change are rreport to the visual part to ajust the representation.

 ⌨️**What commands are allowed now?**
 (!Important! we going to named the cursor of the pixel art Walle)
 + **Spawn**: Change the position of Walle in the canvas
 + **Color**: Change the actual color of the pincel
 + **Size**: Change the size of the cursor(only change to odds numbers)
 + **DrawLine**: Draw a line whith the actual color and size of the pincel in the introduced direction in the number of cells introduced
 + **DrawCircle**:  Draw a circle whith the actual color and size of the pincel in the introduced direction whith the radius introduced(radius only accept odds numbers)
 + **DrawRectangle**:  Draw a rectangle whith the actual color and size of the pincel in the introduced direction whith the width and height introduced(whidth and height only accept odds numbers)
 + **Fill**: Draw whith the actual color of the pincel all the conected cells whith the same color of the actual color 
 + **Labels**
 + **GoTo**: See if the expresion is true and in afimative case ejecute the code from the label introduced
 + **Variables**Than soport:
 + *GetActualX*: Function than get the actual row position of the Walle
 + *GetActualY*: Function than get the actual column position of the Walle
 + *GetCanvasSize*: Function than get the actual canvas 
 + *GetColorCount*: Get the number of cells of the introduced color in the rectangle formed whith the in introduced positions
 + *IsBrushColor*: Get if the brush color is the introduced
 + *IsBrushSize*: Get if the brush size is the introduced
 + *IsCanvasColor: Get the result cell have the same color than the introduced
 + *Valids Colors*: Colors than the canvas detected in form of string
 + *Booleans expresion*
 + *Arithmetic expresion*  

 🧩**Extended language?**
 Yes is not complicated add new functions, colors and signs to the project and that is why this project is open to news features










 
 
