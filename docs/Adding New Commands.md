The suggested way to add new commands to the console is to derive a class from the {{ CommandConsoleBase }} class and in the {{ InitializeCustomCommands() }} method, which is called during initialization by the base class, add your new commands and connect them to their appropriate command handler.  For example:

{code:c#}
// In Your Derived Class
public override void InitializeCustomCommands()
{
    AddCommand( new CmdObject( "kill", "Immediately kill the player", CommandConsole_Kill ) );
    AddCommand( new CmdObject( "nuke", "Immediately kill all on-screen enemies", CommandConsole_Nuke ) );
    AddCommand( new CmdObject( "degreelessness", "Toggle degreelessness mode", CommandConsole_Degreelessness, true ) );

   return;
}
{code:c#}

The above example adds three new commands to XNACC: 
* **{{ kill }}** -- connected to the **{{ CommandConsole_Kill(...) }}** method
* **{{ nuke }}** -- connected to the **{{ CommandConsole_Nuke(...) }}** method
* **{{ degreelessness }}** -- _a secret/hidden command_ -- connected to the **{{ CommandConsole_Degreelessness(...) }}** method

There are other ways of adding/modifying commands and their behavior, which is covered in more detail on the [Hooking Command Execution](Hooking-Command-Execution) page.