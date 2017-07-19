External Functions (**{{exfuncs}}**) are function implementations that are created as compiled .NET code and accessed through .NET assemblies that are loaded on-demand from within XNACC.

The source code archive contains an example project for creating an external function, and that function's code is replicated here:

{code:c#}
using System;
using JRTS.XNA.Console.BaseTypes;
using JRTS.XNA.Console;

namespace CCExtFuncTest
{
    public class ExternalFuncs
    {
        /// <summary>External function that can be called by the
        /// CommandConsole.</summary>
        /// <param name="p1">First parameter passed from the Console</param>
        /// <param name="p2">Second parameter passed from the Console</param>
        /// <param name="p3">Third parameter passed from the Console</param>
        /// <returns>A string, or void</returns>
        public static string ExtFunc1( CVar p1, CVar p2, CVar p3 )
        {
            string	output = String.Empty;

            p1.Value = "New Value P1";
            p2.Value = 42;
            p3.Value = "Yo ho!";
            output = "We Be Jammin'!  We Be Jammin', yeah, mon!";

            return( output );
        }
    }
}
{code:c#}

Some important things to note about implementing **exfuncs**:
# The namespace (**CCExtFuncTest**) **must** match the (file) name of the assembly; in this case, the assembly is {{ CCExtFuncText.dll }}
# Parameters are always treated as reference parameters (they are reference types behind the scenes), so in the above code, **{{P1}}**, **{{P2}}**, and **{{P3}}** all take on the new values as set in the code
# If a String is returned from the function, it will be output to the console
# XNACC's **{{CVarModifiedEvent}}** will fire once for each parameter passed to the exfunc, **even if no real changes took place**

For performance reasons, if multiple **exfuncs** are loaded which exist in the same class, only a single instance of that containing class will be instantiated.  This has a hidden benefit -- you can store state within the containing class and all of that class' **exfuncs** will be able to access/share that state.  The instantiated class will not be destroyed until the XNACC instance is destroyed.  This also allows **exfuncs** to call other methods within their containing class, and pass/share data within the class instance.