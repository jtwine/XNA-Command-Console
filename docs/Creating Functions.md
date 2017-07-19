## Creating (Normal) Functions
The syntax for creating functions, and the code in the console that executes them, is not the most elegant.  So, here are a few examples of how to create functions, and what they can be used for.  Note that if you are not going to be using your function interactively, and parameters are not required, using bindings will be easier and yield better performance.  

Functions can call any other command, function, or exfunc, allowing you to create a function/macro that creates a whole suite of other functions.  But take care not to cause  a recursive loop by having a function call itself -- this is not currently checked for by XNACC's engine.

### Parameters
Functions are unique in that they can accept parameters from the command line, just like normal commands.  But functions can chain the execution of multiple commands, passing the specified parameters as required.

#### Example 1
My console (as used in one of my games) has two commands that affect the appearance of the background stars: **{{starshake}}** and **{{starbright}}**.  The first function causes the Background Stars Manager to cause the stars to shake randomly in a specified X and Y direction, and for a certain duration of time.  The second causes the stars to double their intensity for a duration of time.  If I wanted to create a single function to shake and bright the stars with a single command, I would do something like this:

{{
function ssb starshake %0 %1 %2; starbright %2}}
That would create a single function called **{{ssb}}** that would accept three parameters and pass those parameters to the **{{starshake}}** and **{{starbright}}** commands.  Note the use of a semicolon to separate one command from another.  In this example, calling the function as: **{{ ssb 5 7 2 }}** would activate these command in the console: 

{{ 
starshake 5 7 2
starbright 2}}
#### Validation
XNACC will do minimal validation (i.e. none) to ensure that you are passing enough parameters to the created function.