## Pre-Execution Event Handling
Another way to customize the handling of command is to hook into the **{{ PreCommandExecutedEvent }}** event.  This event is fired before processing of a command line.  The event is passed an **{{ CommandConsoleEventArgs }}** object that contains the command line that is about to be processed and a flag indicating if processing should continue.  

The command line can also be modified by the event handler, and when the event handler returns, command processing will use the modified command line.

Some of the things this allows you to do are:
* Augment the built-in commands by providing new functionality for them
* Ignore certain commands entirely
* Implement new commands (although adding them through a derived class may be better), keeping their implementation out of XNACC-related code
* Add, remove or change a command's parameters by re-writing the command line

Note that if multiple subscriptions exist on the event, trying to use an event handler-modified command line will result in undefined behavior.  Depending on the order by which event delegates are called, you may get a modified command line, or you may get the original.