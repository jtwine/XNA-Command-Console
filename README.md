# XNA-Command-Console
Is a XNA-based command console like ones from the days of Quake.

## Project Description
XNACC is a component that adds an interactive command console to your XNA project. It supports many built-in commands, as well as custom commands, key bindings, simple functions (macros), console variables and can use functions in external assemblies.  Implemented in C#/VS2010.

Use XNACC, and you will never have to implement an interactive console in your game ever again!

## Origin
The code for the XNA Command Console originated with the XNA Console Component Sample, by Kevin Jurkowski. The code was originally obtained from the ZiggyWare site (http://www.ziggyware.com/readarticle.php?article_id=163) back in '09. Other versions of the original code, or pages that mention it, can be found at the following locations:
   http://3dgamingstuff.blogspot.com/2009/03/xna-console-component.html
   http://coreenginedev.blogspot.com/2009_12_01_archive.html

## Enhancements
Quite a few enhancements have been made to the code since then. Some of the highlights are:
   Abstraction into an extendable base class
   Expanded keyboard support, and encapsulation of keyboard input providers
   Built-in commands
   Colorizing of console output
   Command history and completion
   Scrollable log/output
   Support for "hidden" (secret) commands and functions
   Custom functions (basically macros that support parameters)
   External functions (implemented in a .NET assembly)
   Key bindings
   Addition of dynamic Console Variables (cvars)
   Ability to lock-down functions, bindings and external functions (to prevent tampering with a released game)
   More details are available in the documentation.

The code should be immediately usable in its current state, although I am making enhancements and/or changes as time allows. I plan to create a sample game project that demonstrates what the Command Console can do.
