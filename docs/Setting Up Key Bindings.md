## Key Bindings
Key bindings allow the user to invoke console commands (macros) without having to manually show the console, enter the command(s), and then hide the console again.  Key bindings are only active while the console is hidden.

A key binding associates a keypress, usually combined with a modifier key like **ALT** or **CTRL**, with a console command or a series of commands.  For example, if you had implemented a command called **ShowDebugInfo** and you wanted to activate this command while your game was running, but did not want to have to type it into the console each time, you could do something like this: {{ bind CTRL+D ShowDebugInfo }}

Then, whenever the console was hidden and you pressed **CTRL+D**, the **ShowDebugInfo** command would be entered and executed just as if you typed it in yourself.

You can use the {{ bindings }} command to show all current bindings, and the {{ resetbindings }} command will clear all bindings.
