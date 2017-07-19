## Navigating the Console
The console supports simple editing and completion commands -- essentially typing and backspacing.  If the {{ IConsoleKeyboard }} interface was implemented correctly, auto key-repeat should work as well.  Note that space characters are treated specially by XNACC, and it prevents you from doing things like creating a line of spaces.

### Command Completion
Partial commands can be automatically completed on the console by pressing **<TAB>**.  If multiple commands match, pressing **<TAB>** some more will iterate circularly through them.  Commands, functions and exfuncs that are marked as _secret_ are excluded from the command matching code, so your secrets stay secret.

### Command History
The up and down arrow keys are used to navigate backwards and forwards in your command history.  Console command lines are added to the command history unless they are prefixed with _bang_ (an exclamation point - **!**), which indicates that they are **not** to be added to the command history.  

This allows you to create script files that can be **exec**-ed without showing the interactive user what was done.  Note that if a script file's _first line_ is a double-bang (**!!**), **all** of the lines in the script file will excluded from the command history.
 