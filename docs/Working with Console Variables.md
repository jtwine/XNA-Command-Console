## Console Variables
Console Variables, or _cvars_, are user-defined variables that exist within the console's domain.  When used interactively via the console, they can be used to store values of any type that can be safely converted to and from a simple string.  And by simple string, I mean a contiguous sequence of non-whitespace characters.  (N.b. the underlying type only needs this conversion ability if the cvar is going to be manipulated interactively from within the console -- if it is only going to be manipulated behind the scenes, then string conversion is not required.)

### Cvar Underlying Types
By default, cvars have a type of **{{ System.String }}** and this is actually the easiest way to manipulate them from within XNACC.  However, behind the scenes, they are implemented as a type of **{{ System.Object }}** with the actual value type associated with it.  When being manipulated, the value is coerced to the actual type.  

### Creating and Modifying Cvars
Cvars can be created and modified from the command line.  If the type is omitted when creating a new cvar, the underlying type defaults to **{{ System.String. }}**  If the type is omitted when modifying a cvar, the cvar will attempt to convert the specified value to its current underlying type.  If conversion fails, the type reverts to **{{ System.String }}** and the value is applied as a string.  

When specifying a type, a conversion is attempted from **{{ System.String }}** to the specified type.  If conversion fails, the type reverts to **{{ System.String }}** and the value is applied.  So try to use types that have a conversion vehicle to/from **{{ System.String. }}**  Some examples:
# {{ cvar P1 test }}
# {{ cvar P2 Single blah }}
# {{ cvar P4 Single 104.5 }}
# {{ cvar P3 Int32 22 }}
After entering the above commands, the output from the {{ cvars }} command will be:

{{
hh:mm:ss.mmm-p1 (System.String) = test
hh:mm:ss.mmm-p2 (System.String) = blah
hh:mm:ss.mmm-p3 (System.Int32) = 22
hh:mm:ss.mmm-p4 (System.Single) = 104.5 }}
Notice how we tried to create **P2** with a type of **{{ System.Single, }}** but it ended up as a type of**{{ System.String. }}**  Why?  Because we tried to convert "blah" to a **{{ System.Single, }}** and the conversion failed, so the cvar took on the default type of **{{ System.String }}** with the specified value.  The values for **P3** and **P4** were successfully converted to the target types of **{{ System.Int32 }}** and **{{ System.Single, }}** so those are the underlying types of those cvars.

Now, cvars  may not seem very useful from within XNACC, but remember that cvars are the primary vehicle of variable data exchange between XNACC and external functions, as well as the game's implementation itself.  External functions accept cvar types as parameters, and your game's code can receive notification via events whenever a cvar is created or modified.

This allows your game to implement behavior that depends on the presence or value contained with a cvar, which can be manipulated via the console.  For example, you can hook the speed of an enemy projectile into a cvar, allowing someone to change the speed interactively via the console.  Or, with enough effort, you can implement enemy behavior in an external assembly, and you could use cvars to pass information between the two, allowing interactive selection of behaviors from within XNACC.

### Using Custom Types With Cvar
If you need to exchange your own custom object types with cvars, you can generally do so as long as you do not try to modify them interactively from within the console.  Your type should override {{ ToString() }} so that it can be shown in the console.  If you want to interactively manipulate the object, you need to provide a {{ TypeConverter }} for your object that implements either {{ TypeConverter.ConvertFromString( String ) }} or {{ TypeConverter.ConvertFromInvariantString( String ) }}.