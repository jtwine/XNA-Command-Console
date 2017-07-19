## Security Features
One of the more powerful features of XNACC is its support for (simple) functions, bindings and external functions (ExFuncs).  But one of the things to watch out for in a released product is the ability of end-users to manipulate the game's behavior or environment using these features.  

In some cases, you may expose this functionality to your users so that they can do things like manipulate the game in single-player mode.  For example, so that they could adjust the game's behavior making it easier, harder, or just more interesting.

But in a multi-player game, you would not want a player to adjust the game to give themselves an unfair advantage.  

The ability to list and create custom functions, bindings and load externally-implemented functions poses a security risk to the game's internals, so XNACC supports the ability to disable these features.  There are three built-in commands in XNACC that help out here:

**{{nofunctions}}** -- disables the ability to view, add, export, or erase functions
**{{nobindings}}** -- disables the ability to view, add, export, or erase bindings
**{{noexfuncs}}** -- disables the ability to view, add, export, or erase ExFuncs

The above command also prevent the related objects from being output when the console state is exported.

Note that this does not prevent someone from **invoking** a function, binding or ExFunc.