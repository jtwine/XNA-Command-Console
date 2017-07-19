## Documentation
This page contains the Introduction and FAQ for the XNACC as well as links to the detailed documentation pages.  We will start with a list of documentation pages, followed by Screenshots, an Introduction and a FAQ.

# [Built-In Command Reference](Built-In-Command-Reference)
	* [Creating Functions](Creating-Functions)
# [Adding XNACC to your Project](Adding-XNACC-to-your-Project)
# [Navigating the Console](Navigating-the-Console)
# [Adding New Commands](Adding-New-Commands)
# [Hooking Command Execution](Hooking-Command-Execution)
# [Setting Up Key Bindings](Setting-Up-Key-Bindings)
# [Working with Console Variables](Working-with-Console-Variables)
# [Using External Functions](Using-External-Functions)
# [Security Features](Security-Features)
# [Object Reference](Object-Reference) 
# [Tips And Tricks](Tips-And-Tricks)

## Screenshots
Screenshots are available on a separate page: [Screenshots](Screenshots).

## Introduction
XNACC is a full-featured yet easy-to-use command console that can be used by your XNA application.  It can be used to provide a new way to interact with and extend the functionality of your application, making it more attractive to end users.  It also provides a simple logging system, as anything that can get to the XNACC object can add output to its log.

It can also be used purely as a debugging aid, allowing you to change the environment and/or behavior of a running XNA application, even in release builds with no debugger present.
## FAQ
Below are some common questions and answers.  If your question is not answered in the FAQ, please try the links above or the Discussion forum.
#### How is the code licensed?
The code is licensed under the Microsoft Public License (Ms-PL).  However, if this code is used, acknowledgement in the resulting product would be appreciated, but is not required.  Something to the effect of **Portions of this product are (C) 2009-2011 JRTwine Software, LLC** in the product documentation and credits (if applicable) would be sufficient.
#### Is XNACC Case Sensitive?
No.  Internally, commands, functions, exfuncs, and cvar identifiers are all converted to lowercase before being processed.  Note that future versions of XNACC may switch to uppercase, in order to comply with .NET design guidelines.
#### Can XNACC execute commands from a file?
Yes.  See the **EXEC** command in the [Built-In Command Reference](Built-In-Command-Reference).
#### What's with the key translation string arrays?
They provide a way for you to customize the characters that a keyboard (or user's input device) generates for the console.  They also allow you to "hard-bind" a key to a string/macro without doing so through the console's command interface.