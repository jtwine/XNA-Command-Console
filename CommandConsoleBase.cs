#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using JRTS.XNA.Console.BaseTypes;
#endregion

/*
 *  CommandConsoleBase.cs
 *  (C) 2009-2011, James R. Twine of JRTwine Software, LLC
 *
 * *************************************************************************
 * * THIS CODE IS PROVIDED WITH NO WARRANTY - YOU USE IT AT YOUR OWN RISK! *
 * *************************************************************************
 *  
 *  This Code Originated With Code From the "XNA Console Component Sample" by Kevin Jurkowski.
 *  At The Time Of This Writing, The Original Code Was Available 
 *  From: http://www.ziggyware.com/readarticle.php?article_id=163
 *  (But, Ziggyware Is No Longer There! :<)
 *
 *  Changes, (C) 2009, James R. Twine of JRTwine Software, LLC
 *	Changes:
 *      Converted to XNA 3.0
 *      More input keys supported (Symbols, digits, etc.)
 *      Better(?) Way of extending supported commands and their handlers, parameters, help, etc.
 *      Slight Q&D colorizing of output to differenate commands and output
 *      Extracted keyboard-specific functionality into an iterface
 *      Command history
 *      Command completion
 *      Scrollable log/output
 *      Base class supports "standard" commands
 *      Support for "hidden" commands
 *      Customizable scalable drawing 
 *      Custom functions (basically macros that support parameters)
 *      Binding commands/functions to <ALT>-Key combinations (when the console is closed)
 * 
 *      And possibly some other fun stuff...!
 *
 *  More Changes, (C) 2010, James R. Twine of JRTwine Software, LLC
 *	Changes:
 *      Expanded key bindings to support <ALT/SHIFT/CTRL> modifiers
 *		Converted to XNA 4.0
 *      Graphics optimizations
 * 
 *  Still MORE Changes, (C) 2011, James R. Twine of JRTwine Software, LLC
 *	Changes:
 *      Additional ways to hook into command execution/handling
 *      Addition of dynamic Console Variables (cvars)
 *		Support for external managed functions that can manipulate cvars and do... just about anything
 *		Ability to lock-down functions, bindings and external functions (to prevent tampering with a released game)
 *		TODO: Wanna wrap command handlers into an "ICCCommand" interface...
 * 
 * 
 * 
 * 
 * 
*/

/// <summary>Namespace that contains code related to the XNACC (CommandConsole) component</summary>
namespace JRTS.XNA.Console
{
	/// <summary>Base functionality of the XNACC (CommandConsole) component</summary>
	public class CommandConsoleBase : DrawableGameComponent
    {
        #region EventArgs Object
        /// <summary>EA object for the CVarModified event</summary>
        public class CVarModifiedEventArgs : EventArgs
        {
            /// <summary>The actual CVar for this event</summary>
			public CVar Value
            {
                get;
                protected set;
            }
            /// <summary>Constructor for this object</summary>
            /// <param name="value">The cvar that has been modified</param>
            public CVarModifiedEventArgs(CVar value)
            {
                Value = value;
                return;
            }
        }
        /// <summary>EA object for handling console command execution</summary>
        public class CommandConsoleEventArgs : EventArgs
	    {
            /// <summary>The actual command line being executed</summary>
            public string CmdLine
            {
                get;
                set;
            }
            /// <summary>Flag indicating if the command has been handled by an event handler; if true, no further processing is done on the command</summary>
            public bool Handled
            {
                get;
                set;
            }
            /// <summary>Constructor for this object</summary>
            /// <param name="cmdLine">The command line being processed</param>
            public CommandConsoleEventArgs( string cmdLine )
            {
                CmdLine = cmdLine;
                Handled = false;
                return;
            }
	    }

        #endregion

        #region Command Object
        /// <summary>This object contains information on a single Command that the console understands.</summary>
		protected class CmdObject : IComparable< CmdObject >
        {
			/// <summary>Default constructor for the Command Object</summary>
			public CmdObject()
				: this( String.Empty, String.Empty, null, false, String.Empty, 0, 0 )
			{
				return;
			}
			/// <summary>Constuct an instance of the Command Object</summary>
			/// <param name="command">The command identifier (no whitespace, please)</param>
			/// <param name="help">The (brief) help for this command</param>
			/// <param name="cmdAction">The delegate method that fires when the command is encountered</param>
			public CmdObject( string command, string help, Action<string []> cmdAction )
				: this( command, help, cmdAction, false, String.Empty, 0, 0 )
			{
				return;
			}
			/// <summary>Constuct an instance of the Command Object</summary>
			/// <param name="command">The command identifier (no whitespace, please)</param>
			/// <param name="help">The (brief) help for this command</param>
			/// <param name="cmdAction">The delegate method that fires when the command is encountered</param>
			/// <param name="isSecret">Is this a secret command (not shown in the help information)</param>
			public CmdObject( string command, string help, Action<string []> cmdAction, bool isSecret )
				: this( command, help, cmdAction, isSecret, String.Empty, 0, 0 )
			{
				return;
			}
			/// <summary>Constuct an instance of the Command Object</summary>
			/// <param name="command">The command identifier (no whitespace, please)</param>
			/// <param name="help">The (brief) help for this command</param>
			/// <param name="cmdAction">The delegate method that fires when the command is encountered</param>
			/// <param name="isSecret">Is this a secret command (not shown in the help information)</param>
			/// <param name="detailedHelp">The detailed help for this command</param>
			public CmdObject( string command, string help, Action<string []> cmdAction, bool isSecret, string detailedHelp )
				: this( command, help, cmdAction, isSecret, detailedHelp, 0, 0 )
			{
				return;
			}
			/// <summary>Constuct an instance of the Command Object</summary>
			/// <param name="command">The command identifier (no whitespace, please)</param>
			/// <param name="help">The (brief) help for this command</param>
			/// <param name="cmdAction">The delegate method that fires when the command is encountered</param>
			/// <param name="isSecret">Is this a secret command (not shown in the help information)</param>
			/// <param name="detailedHelp">The detailed help for this command</param>
			/// <param name="minParams">The minimum number of parameters for this command (0 == no minimum)</param>
			/// <param name="maxParams">The maximum number of parameters for this command (0 == no maximum)</param>
			public CmdObject( string command, string help, Action<string []> cmdAction, bool isSecret, string detailedHelp, int minParams, int maxParams )
			{
				Command = command;
				CommandHelp = help;
				if( cmdAction != null )
				{
					CmdEvent += cmdAction;
				}
				IsSecret = isSecret;
				CommandHelpDetailed = detailedHelp;
				MinParameters = minParams;
				MaxParameters = maxParams;

				return;

			}
            /// <summary>Required - The command (single word, no spaces, and all commands will be converted to lowercase)</summary>
            public String Command				{ get; set; }
            
			/// <summary>Optional - Summary help information that briefly describes the command</summary>
            public String CommandHelp			{ get; set; }
            
			/// <summary>Optional - Detailed help information that explains the command and its usage</summary>
            public String CommandHelpDetailed	{ get; set; }
            
			/// <summary>The minimum number of parameters the command expects (set to zero for no minimum)</summary>
            public int MinParameters			{ get; set; }
            
			/// <summary>The maximum number of parameters the command expects (set to zero for no maximum)</summary>
			public int MaxParameters			{ get; set; }
            
			/// <summary>Indicates if this command is secret -- if so, it will not appear when the user types 
			///	"help" and will not participate in command completion</summary>
            public bool IsSecret				{ get; set; }

			/// <summary>The event fired when the associated command is received</summary>
			public event Action<string[]> CmdEvent;
            
			/// <summary>This method fires the associated Delegate</summary>
            /// <param name="cmdLine">The complete command line.  Not just the command's parameters!</param>
			/// <remarks>This method is public to allow for artificial triggering of commands.</remarks>
            public void TriggerEvent( String[] cmdLine )
            {
                if( CmdEvent != null )
                {
                    CmdEvent.Invoke( cmdLine );
                }
            }
			#region IComparable<CmdObject> Members
			
			/// <summary>Compare this object with another object instance</summary>
			/// <param name="other">The object to compare to</param>
			/// <returns>The result of comparing the object's Command strings</returns>
			public int CompareTo( CmdObject other )
			{
				return( Command.CompareTo( other.Command ) );
			}
			#endregion
		}
        #endregion

        #region Function Object
        /// <summary>This object contains information on a custom function that the console understands.</summary>
        protected class FuncObject : IComparable< FuncObject >
        {
            /// <summary>The function name</summary>
			public String Function
			{
				get;
				protected set;
			}
			/// <summary>The code to execute when the function executes</summary>
            public IList<String> FunctionImpl
			{
				get;
				protected set;
			}
			/// <summary>Constructor for the Function Object</summary>
			/// <param name="funcName">The name of the Function</param>
			/// <param name="funcImpl">The implementation of the Function</param>
			public FuncObject( string funcName, string[] funcImpl )
			{
				if( String.IsNullOrWhiteSpace( funcName ) )
				{
					throw new ArgumentException( "The function name must not be whitespace.", "funcName" );	
				}
				Function = funcName.Trim().ToLowerInvariant();
				FunctionImpl = new List<string>( funcImpl );				

				return;
			}
            /// <summary>Indicates if this function is secret -- it will not appear when the user types "help"</summary>
			public bool IsSecret
			{
				get;
				set;
			}
            /// <summary>Provide a string representation of this object</summary>
            /// <returns>A string containing a representation of the object</returns>
			public override string ToString()
            {
                StringBuilder	sbCmdLine = new StringBuilder( FunctionImpl.Count * 32 );

                foreach( string line in FunctionImpl )
                {
					if( sbCmdLine.Length > 0 )
					{
						sbCmdLine.Append( "; " );
					}
					sbCmdLine.Append( line );
                }
				return ( String.Format( "{0} -> {1}", Function, sbCmdLine.ToString() ) );
            }

			#region IComparable<FuncObject> Members

			/// <summary>Compare this object with another object instance</summary>
			/// <param name="other">The object to compare to</param>
			/// <returns>The result of comparing the object's Function strings</returns>
			public int CompareTo( FuncObject other )
			{
				return( Function.CompareTo( other.Function ) );
			}

			#endregion
		}
        #endregion

		#region External Function Object
		/// <summary>This object contains information on an external function (implemented in a .NET assembly) that the console understands.</summary>
		protected class ExternalFuncObject : IComparable<ExternalFuncObject>
		{
			/// <summary>Collection Of Known Class Instances</summary>
			static protected Dictionary< string, object >	ms_classInstances = new Dictionary<string,object>( 8 );
			
			/// <summary>The method name</summary>
			public String		MethodName
			{
				get;
				set;
			}
			/// <summary>Is this external function secret?</summary>
			public bool IsSecret
			{
				get;
				protected set;
			}
			/// <summary>Command line used to create the function</summary>
			public String CommandLine
			{
				get;
				set;
			}
			/// <summary>The actual method metadata</summary>
			protected	MethodInfo	m_method;
			/// <summary>The actual class instance</summary>
			protected	object		m_classInstance;
			/// <summary>Construct an instance of this object, storing information on the specified method</summary>
			/// <param name="assembly">The name/path of the assembly/DLL to load from</param>
			/// <param name="className">The fully qualified, case-sensitive class name</param>
			/// <param name="methodName">The case-sensitive name of the method to get</param>
			public ExternalFuncObject( string assembly, string className, string methodName )
				: this( assembly, className, methodName, false )
			{
				return;
			}
			/// <summary>Construct an instance of this object, storing information on the specified method</summary>
			/// <param name="assembly">The name/path of the assembly/DLL to load from</param>
			/// <param name="className">The fully qualified, case-sensitive class name</param>
			/// <param name="methodName">The case-sensitive name of the method to get</param>
			/// <param name="isSecret">Is this a secret/hidden ExFunc?</param>
			public ExternalFuncObject( string assembly, string className, string methodName, bool isSecret )
			{
				Type	classType = null;
					
				// See If We Already Have An Instance Of This Class
				if( ms_classInstances.TryGetValue( className, 
						out m_classInstance ) == false )
				{	// We Do Not, So Create And Store One

					Assembly	asm = null;

					try
					{
						asm = Assembly.LoadFrom( assembly );
					}
					catch( FileNotFoundException )
					{
						asm = Assembly.LoadFrom( assembly + ".dll" );
					}
					classType = asm.GetType( className, false );
					if( classType == null )
					{
						classType = asm.GetType( assembly + "." + className, true );
					}
					m_classInstance = Activator.CreateInstance( classType );
					ms_classInstances[ methodName ] = m_classInstance;
				}
				// Store Name Of Method And 
				MethodName = methodName;
				m_method = classType.GetMethod( methodName );
				IsSecret = isSecret;

				return;
			}

			/// <summary>Attempt to invoke the previously loaded method dynamically</summary>
			/// <param name="parameters">Parameters that should be passed to the method, or null if none</param>
			/// <returns>A string return value from the method, or String.Empty if a null is returned or if the method returns void</returns>
			public string Invoke( params object[] parameters )
			{
				string ret;
				
				try
				{
					object returnVal = m_method.Invoke( m_classInstance, parameters );
					
					ret = ( returnVal != null ) ? returnVal.ToString() : String.Empty;
				}
				catch( Exception ex )
				{
					ret = ex.Message;
				}
				return( ret );
			}
			/// <summary>Provide a string representation of this object</summary>
			/// <returns>A string containing a representation of the object</returns>
			public override string ToString()
			{
				return ( String.Format( "{0}::{1}", m_classInstance, MethodName ) );
			}
			#region IComparable<FuncObject> Members

			/// <summary>Compare this object with another object instance</summary>
			/// <param name="other">The object to compare to</param>
			/// <returns>The result of comparing the object's MethodName strings</returns>
			public int CompareTo( ExternalFuncObject other )
			{
				return ( MethodName.CompareTo( other.MethodName ) );
			}

			#endregion
		}
		#endregion
		
		#region Binding Object
        /// <summary>This object stores information on a bound key</summary>
        protected class BindingObject : IComparable< BindingObject >
        {
            /// <summary>Modifier keys for this key-binding</summary>
            [Flags]
            public enum EModifier
            {
                /// <summary>No modifier keys associated with this Binding</summary>
				None = 0,
				/// <summary>CTRL modifier key is associated with this Binding</summary>
				Ctrl = 1,
				/// <summary>SHIFT modifier key is associated with this Binding</summary>
				Shift = 2,
				/// <summary>ALT modifier key is associated with this Binding</summary>
				Alt = 4,
            }
            /// <summary>The command/function to execute when the bound key is hit</summary>
            public string _text;
            /// <summary>The key that the command/function is bound to</summary>
            public Keys _key;
            /// <summary>The modifier key for _key (Ctrl, Alt, Shift) - modifiers use the LEFT version of the key identifier</summary>
            public EModifier _modifierKeys;
            /// <summary>Gets the modifier key(s) in human readable form</summary>
            /// <returns>A string representing this binding's modifiers, or an empty string if none</returns>
            public string GetModifierString()
            {
                StringBuilder sb = new StringBuilder( 32 );
                bool usesCtrl = ( ( _modifierKeys & EModifier.Ctrl ) == EModifier.Ctrl );
                bool usesShift = ( ( _modifierKeys & EModifier.Shift ) == EModifier.Shift );
                bool usesAlt = ( ( _modifierKeys & EModifier.Alt ) == EModifier.Alt );
                bool first = false;

                if( usesCtrl )
                {
                    sb.Append( "CTRL" );
                    first = false;
                }
                if( usesAlt )
                {
                    if( !first )
                    {
                        sb.Append( '+' );
                    }
                    sb.Append( "ALT" );
                }
                if( usesShift )
                {
                    if( !first )
                    {
                        sb.Append( '+' );
                    }
                    sb.Append( "SHIFT" );
                }
                if( sb.Length > 0 )
                {
                    sb.Append( '+' );
                }
                return( sb.ToString() );
            }
            /// <summary>Convert this binding object into an informational string</summary>
            /// <returns>The informational string</returns>
            public override string ToString()
            {
                return ( String.Format( "<{0}{1}> -> {2}", GetModifierString(), _key, _text ) );
            }

			#region IComparable<BindingObject> Members

			/// <summary>Compare this object with another object instance</summary>
			/// <param name="other">The object to compare to</param>
			/// <returns>Nothing - Not Implemented Yet</returns>
			public int CompareTo( BindingObject other )
			{
				throw new NotImplementedException();
			}

			#endregion
		}
        #endregion

        #region Fields
			#region Command Processing 
		// -- Command Processing-Related Fields...
		/// <summary>Collection Of Command Objects.</summary>
		protected static SortedDictionary<string,CmdObject> ms_commands = new SortedDictionary<string, CmdObject>();
        /// <summary>Collection Of function Objects.</summary>
		protected static SortedDictionary<string, FuncObject> ms_functions = new SortedDictionary<string, FuncObject>();
		/// <summary>Collection Of External Functions.</summary>
		protected static SortedDictionary<string, ExternalFuncObject> ms_externalFunctions = new SortedDictionary<string, ExternalFuncObject>();
		/// <summary>Collection Of Binding Objects.</summary>
		protected static List<BindingObject> ms_bindings = new List<BindingObject>( 8 );
		
		/// <summary>Colleciton Of Partial Command Matches</summary>
		protected static List<string>		ms_partialCmdMatches = new List<string>( 8 );
		/// <summary>Symbol table for the console variables</summary>
		protected Dictionary<string, CVar> m_cVars = new Dictionary<string, CVar>( 8 );
		/// <summary>Match index for the last partial command</summary>
		protected int m_cmdMatchIndex = 0;
		/// <summary>The last command match found</summary>
		protected string m_lastCmdMatch = String.Empty;
		/// <summary>The command line itself</summary>
		protected string m_commandLine = String.Empty;

		/// <summary>Function-related functions are locked out</summary>
		protected bool	m_functionsLocked = false;
		/// <summary>Binding-related functions are locked out</summary>
		protected bool	m_bindingsLocked = false;
		/// <summary>External-Function-related functions are locked out</summary>
		protected bool	m_exfunsLocked = false;
			#endregion

			#region Drawing and Graphics
		// -- Drawing/Graphics-Related Fields...
		/// <summary>Drawing object</summary>
		protected SpriteBatch m_spriteBatch;
		/// <summary>Font used for drawing the console's text</summary>
		protected SpriteFont m_consoleFont;
		/// <summary>Rectangle for the console area</summary>
		protected Rectangle m_consoleRect;
		/// <summary>Location of the command line</summary>
		protected Vector2 m_commandPos;
		/// <summary>Hight for the current console font</summary>
		protected Vector2 m_stringHeight;
		/// <summary>Offset for the command line</summary>
		protected float m_commandStringHeightOffset;
		/// <summary>Number of characters that can fit in the console area's width</summary>
		protected int m_screenCharWidth;
		/// <summary>Width of the console area</summary>
		protected int m_width;
		/// <summary>Height of the console area</summary>
		protected int m_height;
		/// <summary>Scale for drawing the console</summary>
		protected float m_scale = 1.0f;
		#endregion 

			#region Scrolling and Logging
		// -- Scrolling-Related Fields...
		/// <summary>The number of lines that are visible in the console area</summary>
		protected int m_linesVisibleOnScreen;
		/// <summary>Used for drawing an indicator to show that lines are scrolled above</summary>
		protected bool m_linesAbove;
		/// <summary>Used for drawing an indicator to show that lines are scrolled below</summary>
		protected bool m_linesBelow;
		/// <summary>Where the log starts at (for scrolling)</summary>
		protected int m_logStart = 0;
		/// <summary>Specifies the limit of the log buffer</summary>
		protected int m_logLimit = 512;

		// -- Logging-Related Fields...
		/// <summary>The path to the log shadow (copy of all logged lines)</summary>
        protected string m_logShadowFilePath = String.Empty;
        /// <summary>Flag indicating that the log is being shadowed</summary>
        protected bool m_logShadowEnabled = false;
        /// <summary>The stream for shadowing the log to</summary>
		protected StreamWriter m_logShadowFile = null;
		/// <summary>Storage for the log</summary>
		protected List<string> m_log = new List<string>( 256 );
		#endregion

			#region Command History
		// Command History-Related Fields
        /// <summary>Specifies the limit of the command history</summary>
        protected int m_cmdHistoryLimit = 512;
		/// <summary>Collection of command history</summary>
		protected List<string> m_cmdHistory = new List<string>( 128 );
		/// <summary>Current index in the command history</summary>
		protected int m_cmdHistoryIndex = 0;
		/// <summary>Scanning (up/down) index in of the command history</summary>
		protected int m_cmdHistoryScanIndex = 0;
		#endregion 

			#region Input Processing
		// Input related fields
		/// <summary>Time for key repeat</summary>
		protected DateTime		m_keyRepeatTime = DateTime.MinValue;
		/// <summary>Character translation for ALL keyboard keys - contains an element for all values in the Keys enumeration</summary>
		protected List<String>	m_xlateAllKeys;
		/// <summary>Character translation for ALL keyboard keys, shifted - contains an element for all values in the Keys enumeration</summary>
		protected List<String>	m_xlateAllKeysShifted;
		#endregion
		#endregion

		#region Events
		/// <summary>Event fired right before handling/execution of a command.  Allows interception of normal command processing.</summary>
		public event    EventHandler<CommandConsoleEventArgs>	PreCommandExecutedEvent;
		/// <summary>Event fired when a console variable has been created/modified</summary>
		public event    EventHandler<CVarModifiedEventArgs>		CVarModifiedEvent;
		#endregion

        #region Properties
		/// <summary>Interface to the keyboard used for console input</summary>
		public IConsoleKeyboard Keyboard
		{
			get;
			protected set;
		}
		/// <summary>Image/Texture used to fade the text underneath the console's text area</summary>
		public Texture2D FadeImage
		{
			get;
			set;
		}
		/// <summary>The color used to produce the fade</summary>
		public Color FadeColor
		{
			get;
			set;
		}
		/// <summary>Drawing scale for the console</summary>
		public float Scale
		{
			get
			{
				return m_scale;
			}
			set
			{
				m_scale = value;
				m_stringHeight = Vector2.Zero;
			}
		}
		/// <summary>Indicates if the console is active or not</summary>
        public bool Active
        {
            get;
			set;
        }
		/// <summary>The prompt shown for the command line</summary>
		public string Prompt
		{
			get;
			set;
		}
		/// <summary>The color used to display entered commands</summary>
		public Color CommandColor
		{
			get;
			set;
		}
        /// <summary>The color used to display error messages</summary>
        public Color ErrorColor
        {
            get;
            set;
        }
        /// <summary>The color used to display output from executed commands</summary>
        public Color OutputColor
        {
            get;
            set;
        }
        /// <summary>The color used to display "normal" output messages</summary>
        public Color NormalColor
        {
            get;
            set;
        }
        /// <summary>The font used for text display</summary>
        public SpriteFont Font
        {
            get { return( m_consoleFont ); }
            set 
			{ 
				m_consoleFont = value;
                m_stringHeight = Vector2.Zero;
            }
        }
        /// <summary>Sets/Gets the height of the console window</summary>
        public int Height
        {
            get
            {
                return( m_height );
            }
            set
            {
                m_height = value;
                m_stringHeight = Vector2.Zero;
            }
        }
        /// <summary>Sets/Gets the width of the console window</summary>
        public int Width
        {
            get
            {
                return( m_width );
            }
            set
            {
                m_width = value;
                m_stringHeight = Vector2.Zero;
            }
        }
        #endregion

        #region Initialization
        /// <summary>Constructor for this base class</summary>
		/// <param name="game">The Game object for the owning/managing game</param>
		public CommandConsoleBase( Game game )
            : this( game, null )
        {
            return;
        }
		/// <summary>Constructor for this base class</summary>
		/// <param name="game">The Game object for the owning/managing game</param>
		/// <param name="fontToUse">The font that the console should use to draw its text</param>
		public CommandConsoleBase( Game game, SpriteFont fontToUse )
            : base( game )
        {
            m_consoleFont = fontToUse;
			Prompt = "_>";
			CommandColor = Color.SteelBlue;
			ErrorColor = Color.Firebrick;
			OutputColor = Color.Yellow;
			NormalColor = Color.Silver;

            return;
        }
		/// <summary>Primary initialization function</summary>
		public override void Initialize()
        {
            base.Initialize();
			// OK - This Seems Very Heavyweight, And It Kinda Is.  But It Allows For
			// Easy Mapping Of A "Keys" Value To Any String, Which Makes It Possible
			// To Create Single Key Macros, Like Binding On Steroids, And Also Makes
			// It Easy To Handle Non-US Or Gaming Keyboards.
			
			// The Format Of The Lines Below Is A Comment Line That Identifies The Array
			// Indices, Which Correspond To Values In The "Keys" Enumeration, And Then
			// The Actual Values For Those Indices.
			m_xlateAllKeys = new List<String>( new[] {
			//   0    1    2    3    4    5    6    7    8    9   10   11   12   13   14   15
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//  16   17   18   19   20   21   22   23   24   25   26   27   28   29   30   31	
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//  32 	 33   34   35   36   37   38   39   40   41   42   43   44   45   46   47
				" ", "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	48   49   50   51   52   53   54   55   56   57   58   59   60   61   63   63
				"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "" , "" , "" , "" , "" , "" ,
			//	64	 65	  66   67   68	 69	  70   71	72	 73	  74   75	76	 77	  78   79
				"" , "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o",
			//	80   81   82   83   84   85   86   87   88   89   90   91   92   93   94   95
				"p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "" , "" , "" , "" , "" ,
			//	96   97   98   99   100  101  102  103  104  105  106  107  108  109  110  111
				"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "*", "+", ",", "-", ".", "/",
			//	112  113  114  115  116  117  118  119  120  121  122  123  124  125  126  127
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	128  129  130  131  132  133  134  135  136  137  138  139  140  141  142  143
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	144  145  146  147  148  149  150  151  152  153  154  155  156  157  159  159
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	160  161  162  163  164  165  166  167  168  169  170  171  172  173  174  175
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	176  177  178  179  180  181  182  183  184  185  186  187  188  189  190  191
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , ";", "=", ",", "-", ".", "/",
			//	192  193  194  195  196  197  198  199  200  201  202  203  204  205  206  207
				"", "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	208  209  210  211  212  213  214  215  216  217  218  219  220  221  222  223
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "[", "\\","]", "'", "8",
			//	224  225  226  227  228  229  230  231  232  233  234  235  236  237  238  239
				"" , "" ,"|",  "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	240  241  242  243  244  245  246  247  248  249  250  251  252  253  254  255
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
				} );
			
			// Create Collection Of Characters (Strings) For All Possible SHIFTED Key Values...
			m_xlateAllKeysShifted = new List<String>( new[] {
			//   0    1    2    3    4    5    6    7    8    9   10   11   12   13   14   15
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//  16   17   18   19   20   21   22   23   24   25   26   27   28   29   30   31	
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//  32 	 33   34   35   36   37   38   39   40   41   42   43   44   45   46   47
				" ", "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	48   49   50   51   52   53   54   55   56   57   58   59   60   61   63   63
				")", "!", "@", "#", "$", "%", "^", "&", "*", "(", "" , "" , "" , "" , "" , "" ,
			//	64	 65	  66   67   68	 69	  70   71	72	 73	  74   75	76	 77	  78   79
				"" , "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",
			//	80   81   82   83   84   85   86   87   88   89   90   91   92   93   94   95
				"P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "" , "" , "" , "" , "" ,
			//	96   97   98   99   100  101  102  103  104  105  106  107  108  109  110  111
				"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "*", "+", ",", "-", ".", "/",
			//	112  113  114  115  116  117  118  119  120  121  122  123  124  125  126  127
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	128  129  130  131  132  133  134  135  136  137  138  139  140  141  142  143
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	144  145  146  147  148  149  150  151  152  153  154  155  156  157  159  159
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	160  161  162  163  164  165  166  167  168  169  170  171  172  173  174  175
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	176  177  178  179  180  181  182  183  184  185  186  187  188  189  190  191
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , ":", "+", "<", "_", ">", "?",
			//	192  193  194  195  196  197  198  199  200  201  202  203  204  205  206  207
				"", "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	208  209  210  211  212  213  214  215  216  217  218  219  220  221  222  223
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "{", "|", "}","\"", "*",
			//	224  225  226  227  228  229  230  231  232  233  234  235  236  237  238  239
				"" , "" ,"\\", "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			//	240  241  242  243  244  245  246  247  248  249  250  251  252  253  254  255
				"" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" ,
			} );
			return;
        }
        #endregion

        #region Graphics Content
		/// <summary>Load content for this component</summary>
		/// <param name="content">The ContentManager that should be used </param>
		public virtual void LoadContent( ContentManager content )
        {
			m_spriteBatch = new SpriteBatch( GraphicsDevice );

			m_width = GraphicsDevice.Viewport.Width;
            m_height = ( GraphicsDevice.Viewport.Height / 3 );

			InitializeCommands();
            base.LoadContent();

            return;
        }
        #endregion

        #region Update and Draw
        /// <summary>
        /// Recalculates measurements required for text display and wrapping.  Sets the m_consoleRect,
        /// m_commandPos and m_linesVisibleOnScreen fields.
        /// </summary>
        protected virtual void RecalculateHeightSettings()
        {
            m_consoleRect = new Rectangle( 0, 0, m_width, m_height );
            m_commandPos = new Vector2( 10.0f, m_height - m_commandStringHeightOffset );
            m_linesVisibleOnScreen = ( (int)( m_height / m_stringHeight.Y ) - 1 );

            return;
        }
        /// <summary>Update this component</summary>
        /// <param name="gameTime">Game's time</param>
		public override void Update( GameTime gameTime )
        {
            CheckInput();

            base.Update( gameTime );
        }
        /// <summary>Draw the console</summary>
        /// <param name="gameTime">Game's time</param>
		public override void Draw( GameTime gameTime )
        {
            m_linesAbove = m_linesBelow = false;

			if( Active == true )
			{
				m_spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend );

				if( FadeImage != null )
				{
					// Fade The Console Area With The Specified Texture/Image...
					m_spriteBatch.Draw( FadeImage, m_consoleRect, FadeColor );
				}
				//
				// Cache Pre-Calculatable Values...
				//
				if( m_stringHeight == Vector2.Zero )
				{
					m_stringHeight = Vector2.Multiply( m_consoleFont.MeasureString( Prompt ), Scale );
					m_screenCharWidth = (int)( ( m_width / m_stringHeight.X ) * 2 );
					m_commandStringHeightOffset = ( ( m_stringHeight.Y * 2.0f ) - 4.0f );
					RecalculateHeightSettings();
				}
				// Draw command string
				m_spriteBatch.DrawString( m_consoleFont, Prompt + m_commandLine,
						m_commandPos, Color.Yellow, 0.0f, Vector2.Zero,
						Scale, SpriteEffects.None, 0.0f );

				// Draw log
				Vector2 linePos = new Vector2( m_consoleRect.Left + 10.0f, ( m_commandPos.Y - m_stringHeight.Y ) );

				int endLine = Math.Max( 0, ( ( m_log.Count - m_logStart ) - 1 ) );

				if( m_logStart != 0 )
				{
					m_linesBelow = true;
				}
				for( int i = endLine; i > 0; i-- )
				{
					if( linePos.Y <= m_consoleRect.Top )
					{
						m_linesAbove = true;
						break;
					}
					String line = m_log[ i ];

					if( line.Length == 0 )
					{
						continue;
					}
					Char firstChar = line[ 0 ];

					if( firstChar <= '\x03' )
					{
						Color outColor = OutputColor;

						if( firstChar == '\x01' )
						{
							outColor = CommandColor;
						}
						else if( firstChar == '\x02' )
						{
							outColor = ErrorColor;
						}
						else if( firstChar == '\x03' )
						{
							outColor = OutputColor;
						}
						m_spriteBatch.DrawString( m_consoleFont, line.Substring( 1 ),
								linePos, outColor, 0.0f, Vector2.Zero, Scale,
								SpriteEffects.None, 0.0f );
					}
					else
					{
						m_spriteBatch.DrawString( m_consoleFont, line, linePos,
								NormalColor, 0.0f, Vector2.Zero, Scale,
								SpriteEffects.None, 0.0f );
					}
					linePos.Y -= m_stringHeight.Y;
				}
				if( m_linesAbove )
				{
					Vector2 starPos = new Vector2( m_width - 10.0f, m_consoleRect.Top );

					m_spriteBatch.DrawString( m_consoleFont, "^", starPos,
							ErrorColor, 0.0f, Vector2.Zero, Scale,
							SpriteEffects.None, 0.0f );
				}
				if( m_linesBelow )
				{
					Vector2 starPos = new Vector2( m_width - 10.0f, ( m_commandPos.Y - m_stringHeight.Y ) );

					m_spriteBatch.DrawString( m_consoleFont, "V", starPos,
							ErrorColor, 0.0f, Vector2.Zero, Scale,
							SpriteEffects.None, 0.0f );
				}
				m_spriteBatch.End();
			}
			base.Draw( gameTime );

            return;
        }
        #endregion

        #region Input Handling
		/// <summary>Process a keyboard key.  Assumes that the Keyboard property is not null!</summary>
		/// <param name="key">The key to process</param>
		protected virtual void ProcessKey( Keys key )
		{
			// Space - Special Handling To Eliminate Leading Whitespace
			if( ( key == Keys.Space ) && ( !String.IsNullOrWhiteSpace( m_commandLine ) ) )
			{
				m_commandLine += " ";
				ms_partialCmdMatches.Clear();
				m_lastCmdMatch = String.Empty;
			}
			// Command History...
			else if( key == Keys.Up )
			{
				if( m_cmdHistory.Count == 0 )
				{
					return;
				}
				m_cmdHistoryScanIndex--;
				if( m_cmdHistoryScanIndex < 0 )
				{
					m_cmdHistoryScanIndex = 0;
				}
				m_commandLine = m_cmdHistory[ m_cmdHistoryScanIndex ];
				ms_partialCmdMatches.Clear();
			}
			else if( key == Keys.Down ) 
			{
				if( m_cmdHistory.Count == 0 )
				{
					return;
				}
				m_cmdHistoryScanIndex++;
				if( m_cmdHistoryScanIndex >= m_cmdHistory.Count )
				{
					m_cmdHistoryScanIndex = ( m_cmdHistory.Count - 1 );
				}
				m_commandLine = m_cmdHistory[ m_cmdHistoryScanIndex ];
				ms_partialCmdMatches.Clear();
			}
			// Log Scrolling...
			else if( ( key == Keys.PageUp ) && ( m_linesVisibleOnScreen < m_log.Count ) )
			{
				m_logStart += ( m_linesVisibleOnScreen - 1 );
				if( m_logStart >= ( m_log.Count - m_linesVisibleOnScreen ) )
				{
					m_logStart = ( m_log.Count - m_linesVisibleOnScreen + 1 );
				}
			}
			else if( ( key == Keys.PageDown ) && ( m_linesVisibleOnScreen < m_log.Count ) )
			{
				m_logStart -= ( m_linesVisibleOnScreen - 1 );
				if( m_logStart < 0 )
				{
					m_logStart = 0;
				}
			}
			// Check input for Escape
			else if( key == Keys.Escape )
			{
				m_commandLine = String.Empty;
				ms_partialCmdMatches.Clear();
				m_lastCmdMatch = String.Empty;
			}
			// Check input for Backspace
			else if( ( key == Keys.Back ) && ( !String.IsNullOrEmpty( m_commandLine ) ) && 
						( m_commandLine != Prompt ) )
			{
				m_commandLine = m_commandLine.Remove( m_commandLine.Length - 1, 1 );
				ms_partialCmdMatches.Clear();
				m_lastCmdMatch = String.Empty;
			}
			// Check input for Tab
			else if( ( key == Keys.Tab ) && ( !String.IsNullOrEmpty( m_commandLine ) ) && 
						( m_commandLine != Prompt ) )
			{
				// Rebuild Partial Command Matches If List Is Empty/Reset
				if( ms_partialCmdMatches.Count == 0 )
				{
					BuildCommandMatches( m_commandLine );
					m_cmdMatchIndex = 0;
				}
				// If We Have Partial Matches, Circularly Iterate Over Them And Show Them
				if( ms_partialCmdMatches.Count > 0 )
				{
					if( m_cmdMatchIndex >= ms_partialCmdMatches.Count )
					{
						m_cmdMatchIndex = 0;
					}
					m_commandLine = ms_partialCmdMatches[ m_cmdMatchIndex++ ];
				}
			}
			// Enter - Commit Command Line
			else if( ( key == Keys.Enter ) && ( !String.IsNullOrWhiteSpace( m_commandLine ) ) )
			{
				AddToLog( "\x01" + m_commandLine );
				m_logStart = 0;
				ExecuteCommandLine( true );
				m_commandLine = String.Empty;
			}
			// Any Other Key, Process As Character (Even If Not A Real Character Key)
			else
			{
				// Handle Shifted Chars
				if( ( Keyboard.CurrentKeyboardState.IsKeyDown( Keys.LeftShift ) ) ||
						( Keyboard.CurrentKeyboardState.IsKeyDown( Keys.RightShift ) ) )
				{
					m_commandLine += m_xlateAllKeysShifted[ (int)key ];
				}
				else
				{
					m_commandLine += m_xlateAllKeys[ (int)key ];
				}
				ms_partialCmdMatches.Clear();
				m_lastCmdMatch = String.Empty;
			}
			return;
		}
		
		/// <summary>Check input from the provided keyboard interface</summary>
		protected virtual void CheckInput()
        {
			//InputManager	im = InputManager.Instance;

			// If No Interface To The Keyboard, Stop Here
			if( Keyboard == null )
			{
				return;
			}
			// Toggle the console menu on or off
			if( Keyboard.NewlyPressedKeys.Contains( Keys.OemTilde ) )
            {
                Active = !Active;
				return;
            }
            // Only Check Bindings If The Console Is Not Active
			if( Active == false )
            {
                CheckBindings();
                return;
            }
			if( Keyboard.HeldKeys.Count > 0 )
			{
				if( DateTime.Now > m_keyRepeatTime )
				{
					ProcessKey( Keyboard.HeldKeys[ 0 ] );
					m_keyRepeatTime = DateTime.Now.AddMilliseconds( 100 );
				}
			}
			if( Keyboard.NewlyPressedKeys.Count > 0 )
			{
				ProcessKey( Keyboard.NewlyPressedKeys[ 0 ] );
				m_keyRepeatTime = DateTime.Now.AddMilliseconds( 250 );
			}
			return;
        }

        /// <summary>Get binding information from a "binding string" input by the user</summary>
        /// <param name="bindString">The binding string to parse</param>
        /// <param name="bindModifiers">ref - the modifier keys for the binding</param>
        /// <param name="bindKey">ref - the key for the binding</param>
        /// <returns>true if the binding string was understood, false if not</returns>
        bool GetKeyBindingInfo( string bindString, ref BindingObject.EModifier bindModifiers, ref Keys bindKey )
        {
            bindString = bindString.ToUpper();
            if( bindString.Length == 1 )
            {
                char keyChar = ( bindString.Trim().ToUpper()[ 0 ] );
                int keyOffset = keyChar - 'A';
                bindKey = ( Keys.A + keyOffset );
                // Default To ALT Binding...
                // bindModifiers = BindingObject.EModifier.Alt;
                
                if( ( bindKey >= Keys.A ) && ( bindKey <= Keys.Z ) )
                {
                    return( true );
                }
                else
                {
                    return( false );
                }
            }
            string[] keyTokens = bindString.Split( '+' );
            bool parsed = false;

            foreach( string ktoken in keyTokens )
            {
                if( ktoken == "ALT" )
                {
                    bindModifiers |= BindingObject.EModifier.Alt;
                    continue;
                }
                if( ktoken == "CTRL" )
                {
                    bindModifiers |= BindingObject.EModifier.Ctrl;
                    continue;
                }
                if( ktoken == "SHIFT" )
                {
                    bindModifiers |= BindingObject.EModifier.Shift;
                    continue;
                }
                if( ktoken.Length == 1 )
                {
                    int keyOffset = ktoken[ 0 ] - 'A';
                    bindKey = ( Keys.A + keyOffset );
                    if( ( bindKey >= Keys.A ) && ( bindKey <= Keys.B ) )
                    {
                        parsed = true;
                    }
                    else
                    {
                        parsed = false;
                    }
                    break;
                }
                else if( ( ( ktoken.Length == 2 ) || ( ktoken.Length == 3 ) ) && ( ktoken[ 0 ] == 'F' ) )
                {
                    // F-Key Binding...
                    int fNumber = int.Parse( ktoken.Substring( 1 ) );
                    Keys fKey = ( Keys.Divide + fNumber );
                    
                    if( ( fKey >= Keys.F1 ) && ( fKey <= Keys.F24 ) )
                    {
                        bindKey = fKey;
                        parsed = true;
                    }
                    else
                    {
                        parsed = false;
                    }
                    break;
                }
                else 
                {
                    break;
                }
            }
            return ( parsed );
        }
        /// <summary>Check for bound keys - bound keys use the &lt;ALT&gt; modifier</summary>
        protected virtual void CheckBindings()
        {
            // Stop Here If No Bindings Are Set, Or No Interface To The Keyboard
			if( ( ms_bindings.Count == 0 ) || ( Keyboard == null ) )
            {
                return;
            }
			//InputManager	im = InputManager.Instance;

			BindingObject.EModifier modifierKeys = BindingObject.EModifier.None;
			bool altDown = ( ( Keyboard.CurrentKeyboardState.IsKeyDown( Keys.LeftAlt ) ) || ( Keyboard.CurrentKeyboardState.IsKeyDown( Keys.RightAlt ) ) );
			bool ctrlDown = ( ( Keyboard.CurrentKeyboardState.IsKeyDown( Keys.LeftControl ) ) || ( Keyboard.CurrentKeyboardState.IsKeyDown( Keys.RightControl ) ) );
			bool shiftDown = ( ( Keyboard.CurrentKeyboardState.IsKeyDown( Keys.LeftShift ) ) || ( Keyboard.CurrentKeyboardState.IsKeyDown( Keys.RightShift ) ) );
                                                                
            if( altDown ) modifierKeys |= BindingObject.EModifier.Alt;
            if( ctrlDown ) modifierKeys |= BindingObject.EModifier.Ctrl;
            if( shiftDown ) modifierKeys |= BindingObject.EModifier.Shift;

            foreach( BindingObject bs in ms_bindings )
            {
                // See if the Binding's modifier keys are down, and its command key.  If so, execute the command.
                if( ( ( bs._modifierKeys & modifierKeys ) == modifierKeys ) &&
						( Keyboard.NewlyPressedKeys.Contains( bs._key ) ) )
                {
                    m_commandLine = bs._text;
                    ExecuteCommandLine( false );
                    m_commandLine = String.Empty;
                }
            }
            return;
        }

		/// <summary>Build command matches for the specified (sub)string.</summary>
		/// <param name="cmdMatch">The partial command to try to match</param>
		/// <returns>Nothing</returns>
		/// <remarks>Resets the ms_partialCmdMatches collection</remarks>
		protected virtual void BuildCommandMatches( string cmdMatch )
		{
			ms_partialCmdMatches.Clear();

			// Iterate Over Commands And Store Non-Secret Commands That Match Substring
			foreach( CmdObject cmd in ms_commands.Values )
			{
				if( ( !cmd.IsSecret ) && ( cmd.Command.StartsWith( cmdMatch ) ) )
				{
					ms_partialCmdMatches.Add( cmd.Command );
				}
			}
			// Iterate Over Funcitons And Store Non-Secret Functions That Match Substring
			foreach( FuncObject func in ms_functions.Values )
			{
				if( ( !func.IsSecret ) && ( func.Function.StartsWith( cmdMatch ) ) )
				{
					ms_partialCmdMatches.Add( func.Function );
				}
			}
			// Iterate Over External Funcitons And Store Non-Secret Functions That Match Substring
			foreach( ExternalFuncObject extFunc in ms_externalFunctions.Values )
			{
				if( ( extFunc.IsSecret ) && ( extFunc.MethodName.StartsWith( cmdMatch ) ) )
				{
					ms_partialCmdMatches.Add( extFunc.MethodName );
				}
			}
			return;
		}

		#endregion

		#region Log Output
		/// <summary>Add executed command output to the log.</summary>
        /// <param name="text">The output text to add</param>
        public void AddOutputToLog( string text )
        {
            if( String.IsNullOrEmpty( text ) )
            {
                return;
            }
            AddToLog( "\x03" + DateTime.Now.ToString( "hh:mm:ss.fff" ) + "-" + text );

            return;
        }
        
        /// <summary>Add error output to the log.</summary>
        /// <param name="text">The error text to add</param>
        public void AddErrorToLog( string text )
        {
            if( String.IsNullOrEmpty( text ) )
            {
                return;
            }
            AddToLog( "\x02" + DateTime.Now.ToString( "hh:mm:ss.fff" ) + "-" + text );

            return;
        }
        
        /// <summary>Add executed command to the log.</summary>
        /// <param name="text">The command text to add</param>
        public void AddCommandToLog( string text )
        {
            if( String.IsNullOrEmpty( text ) )
            {
                return;
            }
            AddToLog( "\x01" + DateTime.Now.ToString( "hh:mm:ss.fff" ) + "-" + text );
            
            return;
        }
        
        /// <summary>Method to add generic output text to the log</summary>
        /// <param name="text">The text to add</param>
        public void AddToLog( string text )
        {
            if( String.IsNullOrEmpty( text ) )
            {
                return;
            }
            if( m_logShadowEnabled )
            {
                m_logShadowFile.WriteLine( text );
            }
            lock( m_log )
            {
                if( ( m_screenCharWidth == 0 ) || ( text.Length < m_screenCharWidth ) )
                {
                    m_log.Add( text );
                    return;
                }
                List<string> lines = BreakTextIntoList( text, m_consoleFont, ( m_consoleRect.Width - 10 ), Scale );

				foreach( String line in lines )
                {
					m_log.Add( line );
                }
                if( m_log.Count > m_logLimit )
                {
                    int remove = Math.Max( 32, ( m_log.Count - m_logLimit ) );

                    remove--;
                    m_log.RemoveRange( 0, remove );
                }
            }
            return;
        }

		// JRT	Only Evaluate This Once...
		private static readonly	char[] DELIMITERS = " .:,/\\-+=_[]{}()?!><|".ToCharArray();

		/// <summary>
        /// Break text up into separate lines to make it fit.
        /// (Originally From MS Provided Code - RBG Starter Kit/Template - Text Helper Methods)
        /// </summary>
        /// <param name="text">The text to be broken up.</param>
        /// <param name="font">The font used to measure the width of the text.</param>
        /// <param name="rowWidth">The maximum width of each line, in pixels.</param>
		/// <param name="scale">Scale that should be used for the lines.</param>
		protected static List<string> BreakTextIntoList( string text, SpriteFont font,
                int rowWidth, float scale )
        {
            // check parameters
			if( String.IsNullOrEmpty( text ) )
			{
				return( new List<string>( new[] { String.Empty } ) );
			}
			if( font == null )
            {
                throw new ArgumentNullException( "font" );
            }
            if( rowWidth <= 0 )
            {
                throw new ArgumentOutOfRangeException( "rowWidth" );
            }
            // create the list
            List<string> lines = new List<string>( text.Length / rowWidth );

            // check for trivial text
            if( String.IsNullOrEmpty( text ) )
            {
                lines.Add( String.Empty );
                return lines;
            }
            char cLead = text[ 0 ];

            //
			// JRT	Q&D Way To Get Rid Of The Coloring Characters - They Are Not Written
            //		Out To The Screen, Anyway...
            //
            if( ( cLead == '\x01' ) || ( cLead == '\x02' ) || ( cLead == '\x03' ) )
            {
                text = text.Substring( 1 );
            }
            else
            {
                cLead = '\x00';
            }
            // check for text that fits on a single line
            if( ( font.MeasureString( text ).X * scale ) <= rowWidth )
            {
                if( cLead != '\x00' )
                {
                    text = text.Insert( 0, cLead.ToString() );
                }
                return lines;
            }

            StringBuilder sb = new StringBuilder( 256 );
            int textIndex = 0;
            int currentDelimiterIndex = 0;

            // JRT	This Loop Will Cause The Occasional Leading Delimiter Character If The
			//		Line Break Occurs At A Place Where To Delimiters Are Next To Each
			//		Other, But I Consider This An Acceptable Tradeoff.
			//
			//		Lines That Cannot Be Broken-Up Will Wrap, But The Wrapping Will Not
			//		Be Optimal.
			while( textIndex < text.Length )
            {
                String appendedString;
                int lastLen = textIndex;

                currentDelimiterIndex = text.IndexOfAny( DELIMITERS, textIndex );
                if( currentDelimiterIndex == -1 )
                {
					appendedString = text.Substring( textIndex );
                }
                else
                {
                    appendedString = text.Substring( textIndex, ( currentDelimiterIndex - textIndex + 1 ) );
                }
                sb.Append( appendedString );
				if( ( font.MeasureString( sb.ToString() ).X * scale ) > rowWidth )
				{
					sb.Remove( ( sb.Length - appendedString.Length ), appendedString.Length );
					if( cLead != '\x00' )
					{
						sb.Insert( 0, cLead );
					}
					lines.Add( sb.ToString() );
					sb.Length = 0;
					text = text.Substring( textIndex );
					textIndex = 0;
				}
				else
				{
					textIndex += appendedString.Length;
				}
            }
            if( sb.Length > 0 )
            {
                lines.Add( sb.ToString() );
            }
            return( lines );
        }
        #endregion 

        #region Built-In Commands
        /// <summary>Sets up the standard/built-in commands.</summary>
        protected void InitializeStandardCommands()
        {
            CmdObject cmdStruct = new CmdObject();

            cmdStruct.Command = "help";
            cmdStruct.CommandHelp = "show game console help, or help for a specific command";
            cmdStruct.CommandHelpDetailed = "help [command]";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_Help );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "quit";
            cmdStruct.CommandHelp = "immediately quit the game";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_Quit );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "close";
            cmdStruct.CommandHelp = "close the command console.";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_Close );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "consoleheight";
            cmdStruct.MinParameters = 0;
            cmdStruct.MaxParameters = 1;
            cmdStruct.CommandHelp = "set the height of the console window in pixels";
            cmdStruct.CommandHelpDetailed = "consoleheight <heightvalue>";
            cmdStruct.CmdEvent +=new Action<string[]>(CommandConsoleBase_ConsoleHeight);
			ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "consolescale";
            cmdStruct.MinParameters = 0;
            cmdStruct.MaxParameters = 1;
            cmdStruct.CommandHelp = "set the scale of the console window (1.0=100%, 0.75=75%, 0.5=50%, etc.)";
            cmdStruct.CommandHelpDetailed = "consolescale<scale>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ConsoleScale );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "cvar";
			cmdStruct.MinParameters = 1;
			cmdStruct.MaxParameters = 3;
			cmdStruct.CommandHelp = "add a new console variable (cvar), or modify an existing one";
			cmdStruct.CommandHelpDetailed = "cvar <name> [Type] <value>  -- examples: cvar myVar MyValue, cvar myInt Int32 42";
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_CVar );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "cvars";
			cmdStruct.MinParameters = 0;
			cmdStruct.MaxParameters = 0;
			cmdStruct.CommandHelp = "view the available console variables (cvars)";
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_CVars );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
            cmdStruct.Command = "gc";
            cmdStruct.CommandHelp = "immediately perform full garbage collection";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_GC );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "shadowlog";
            cmdStruct.MinParameters = 0;
            cmdStruct.MaxParameters = 2;
            cmdStruct.CommandHelp = "shadows the log to an external file, optionally truncating the external file first";
            cmdStruct.CommandHelpDetailed = "shadowlog <filepath> <true/false>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ShadowLog );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "noshadowlog";
            cmdStruct.CommandHelp = "turns off shadowing of the log to an external file";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_NoShadowLog );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "clear";
            cmdStruct.CommandHelp = "clears the console log";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_Clear );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "loglimit";
            cmdStruct.MaxParameters = 1;
            cmdStruct.CommandHelp = "sets the console log (buffer) limit";
            cmdStruct.CommandHelpDetailed = "loglimit <numlines>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_LogLimit );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "memoryinfo";
            cmdStruct.CommandHelp = "display memory info";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_MemInfo );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "exfunc";
			cmdStruct.CommandHelp = "load external function from an assembly";
			cmdStruct.CommandHelpDetailed = "exfunc <assemblypath> <classname> <functionname>";
			cmdStruct.MinParameters = 3;
			cmdStruct.MaxParameters = 3;
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ExFunc );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
            cmdStruct.Command = "systeminfo";
            cmdStruct.CommandHelp = "display system info";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_SystemInfo );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "sleep";
            cmdStruct.MinParameters = 1;
            cmdStruct.MaxParameters = 1;
            cmdStruct.CommandHelp = "eause execution of the command console";
            cmdStruct.CommandHelpDetailed = "sleep <durationms>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_Sleep );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "exec";
            cmdStruct.MinParameters = 1;
            cmdStruct.MaxParameters = 1;
            cmdStruct.CommandHelp = "execute contents of a file as commands in this console";
            cmdStruct.CommandHelpDetailed = "exec <filepath>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_Exec );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "savelog";
            cmdStruct.MinParameters = 1;
            cmdStruct.MaxParameters = 1;
            cmdStruct.CommandHelp = "save the console log to a file";
            cmdStruct.CommandHelpDetailed = "savelog <filepath>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_SaveLog );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "functions";
            cmdStruct.CommandHelp = "display all current functions";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_Functions );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "nofunctions";
			cmdStruct.CommandHelp = "disable the ability to add/remove functions";
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_NoFunctions );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "exfuncs";
			cmdStruct.CommandHelp = "display all current external functions";
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ExFuncs );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "noexfuncs";
			cmdStruct.CommandHelp = "disable the ability to add/remove exfuncs";
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_NoExFunctions );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
            cmdStruct.Command = "bindings";
            cmdStruct.CommandHelp = "display all current key bindings";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_Bindings );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "nobindings";
			cmdStruct.CommandHelp = "disable the ability to add/remove bindings";
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_NoBindings );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "resetbindings";
			cmdStruct.CommandHelp = "clear all key bindings";
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ResetBindings );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "resetexfuncs";
			cmdStruct.CommandHelp = "clear all external functions";
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ResetExFuncs );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "resetfunctions";
			cmdStruct.CommandHelp = "clear all functions";
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ResetFunctions);
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
            cmdStruct.Command = "bind";
            cmdStruct.MinParameters = 2;
            cmdStruct.MaxParameters = 0;
            cmdStruct.CommandHelp = "bind an key to a command or function";
            cmdStruct.CommandHelpDetailed = "bind <key> <commandtext/functionname> -- examples: 'bind ALT+G gc', 'bind CTRL+SHIFT+Q quit'";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_Bind );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "unbind";
            cmdStruct.MinParameters = 1;
            cmdStruct.MaxParameters = 1;
            cmdStruct.CommandHelp = "remove a previous key binding";
            cmdStruct.CommandHelpDetailed = "unbind <key>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_UnBind );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "function";
            cmdStruct.MinParameters = 1;
            cmdStruct.MaxParameters = 0;
            cmdStruct.CommandHelp = "create a new function";
            cmdStruct.CommandHelpDetailed = "function <functionname> <line>;<line>;<...>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_Function );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "exportfunctions";
            cmdStruct.MinParameters = 1;
            cmdStruct.MaxParameters = 1;
            cmdStruct.CommandHelp = "export functions to a file that can be EXEC-ed";
            cmdStruct.CommandHelpDetailed = "exportfunctions <filepath>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ExportFunctions );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
			cmdStruct.Command = "exportexfuncs";
			cmdStruct.MinParameters = 1;
			cmdStruct.MaxParameters = 1;
			cmdStruct.CommandHelp = "export exfuncs to a file that can be EXEC-ed";
			cmdStruct.CommandHelpDetailed = "exportexfuncs <filepath>";
			cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ExportExFuncs );
			ms_commands[ cmdStruct.Command ] = cmdStruct;

			cmdStruct = new CmdObject();
            cmdStruct.Command = "exportbindings";
            cmdStruct.MinParameters = 1;
            cmdStruct.MaxParameters = 1;
			cmdStruct.CommandHelp = "export binding information to a file that can be EXEC-ed";
            cmdStruct.CommandHelpDetailed = "exportbindings <filepath>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ExportBindings );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

            cmdStruct = new CmdObject();
            cmdStruct.Command = "exportstate";
            cmdStruct.MinParameters = 1;
            cmdStruct.MaxParameters = 1;
			cmdStruct.CommandHelp = "export the console's complete state information to a file that can be EXEC-ed, so that the state can be restored later";
            cmdStruct.CommandHelpDetailed = "exportstate <filepath>";
            cmdStruct.CmdEvent += new Action<string[]>( CommandConsoleBase_ExportState );
            ms_commands[ cmdStruct.Command ] = cmdStruct;

            return;
        }

        /// <summary>
        /// Overridable method used to add custom commands to the console.
        /// </summary>
        public virtual void InitializeCustomCommands()
        {
            return;
        }

		/// <summary>Initialize the Console's Commands -- is normally called via <see cref="LoadContent"/></summary>
		public void InitializeCommands()
        {
            InitializeStandardCommands();

            InitializeCustomCommands();

            return;
        }
        
        /// <summary>
        /// Add a command to the internal collection of commands.  Throws an exception if duplicate identifiers are detected.
        /// </summary>
        /// <param name="cmdStruct">Command object to add</param>
        protected void AddCommand( CmdObject cmdStruct )
        {
            cmdStruct.Command = cmdStruct.Command.Trim().ToLowerInvariant();

			if( ms_commands.ContainsKey( cmdStruct.Command ) )
			{
				throw( new InvalidOperationException( String.Format( 
						"Command {0} already exists in the command list.", 
						cmdStruct.Command ) ) );
			}
			if( ms_functions.ContainsKey( cmdStruct.Command ) )
			{
				throw ( new InvalidOperationException( String.Format(
						"Command {0} already exists as a function.",
						cmdStruct.Command ) ) );
			}
			ms_commands[ cmdStruct.Command ] = cmdStruct;
        
            return;
        }

        /// <summary>
        /// Add a function to the internal collection of functions.  Throws an exception if duplicate identifiers are detected.
        /// </summary>
        /// <param name="fnStruct">Function object to add</param>
        protected void AddFunction( FuncObject fnStruct )
        {
			if( ms_commands.ContainsKey( fnStruct.Function ) )
			{
				throw ( new InvalidOperationException( String.Format(
						"Function {0} already exists as a command.",
						fnStruct.Function ) ) );
			}
			if( ms_functions.ContainsKey( fnStruct.Function ) )
			{
				throw ( new InvalidOperationException( String.Format(
						"Function {0} already exists in the function list.",
						fnStruct.Function ) ) );
			}
            ms_functions[ fnStruct.Function ] = fnStruct;

            return;
        }
        #endregion

        #region Command Processing
        /// <summary>Execute the specified Function</summary>
		/// <param name="fnStruct">Function to execute</param>
		/// <param name="parameters">Parameters for the function</param>
        protected void ExecuteFunction( FuncObject fnStruct, string[] parameters )
        {
            foreach( string line in fnStruct.FunctionImpl )
            {
                m_commandLine = line;
                // If Function Parameters Exist, And Function Takes Parameters
                if( ( parameters != null ) && ( m_commandLine.IndexOf( '%' ) != -1 ) )
                {
                    int count = 0;

                    // Build The Resulting Command Line
                    foreach( string param in parameters )
                    {
                        m_commandLine = m_commandLine.Replace( "%" + count, parameters[ count ] );
                        count++;
                    }
                }
                AddToLog( "\x01-> " + m_commandLine );
                ExecuteCommandLine( false );
                m_commandLine = String.Empty;
            }
            return;
        }

        /// <summary>Evaluates and executes the command line stored in the "_command" field.</summary>
        public virtual void ExecuteCommandLine( bool addToHistory )
        {
			string trimmedCmdLine = m_commandLine.Trim();

			// Ignore Empty And Whitespace Command Lines...
			if( String.IsNullOrWhiteSpace( trimmedCmdLine ) )
			{
				return;
			}
			// Ignore Comments...
			if( trimmedCmdLine[ 0 ] == '#' )
			{
				return;
			}
			// Bang Indicates That This Line Should NOT Be Added To Command History
			if (trimmedCmdLine[0] == '!')
            {
                addToHistory = false;
                trimmedCmdLine = trimmedCmdLine.Substring(1);
            }
            else if( addToHistory )
            {
                m_cmdHistory.Add(trimmedCmdLine);
                if( m_cmdHistory.Count > m_cmdHistoryLimit )
                {
                    m_cmdHistory.RemoveAt(m_cmdHistoryLimit);
                }
                m_cmdHistoryIndex = m_cmdHistory.Count;
                // m_cmdHistoryIndex = ( m_cmdHistory.Count - 1 );
                m_cmdHistoryScanIndex = m_cmdHistoryIndex;
            }
            // If A PreCommand Execution Handling Delegate Has Been Set,
			// Execute The Delegate And See If The Command Was Handled
			// Externally.
			if( PreCommandExecutedEvent != null )
			{
				CommandConsoleEventArgs ea = new CommandConsoleEventArgs( trimmedCmdLine );

				try
				{
					PreCommandExecutedEvent( this, ea );
					trimmedCmdLine = ea.CmdLine;
				}
				catch( Exception ex )
				{
					AddErrorToLog( "** PreCommandExecuted delegate(s) thrown an exception: " + ex.Message );
				}
				if( ea.Handled )
				{
					return;
				}
			}
			try
			{
				CmdObject	cs;
				String[]	cmds = trimmedCmdLine.Split( ' ' );
				String		cmd = cmds[ 0 ].ToLowerInvariant();
				bool		found = false;

				// Commands Are more Common Then Functions, So Check them First.
				if( ms_commands.TryGetValue( cmd, out cs ) )
				{
					found = ExecuteCommand( cs, cmds, cmd );
				}
				// Next, Try External Functions...
				if( ( !found ) && ( ms_externalFunctions.Count > 0 ) )
				{
					found = ExecuteExternalFunc( cmds, cmd );
				}
				// Next, Try Internal Functions...
				if( ( !found ) && ( ms_functions.Count > 0 ) )
				{
					found = ExecuteInternalFunc( cmds, cmd );
				}
				// Lastly, See If A Console Variable...
				if( ( !found ) && ( m_cVars.Count > 0 ) )
				{
					found = HandleCVar( cmds, cmd );
				}
				// If STILL Not Found...
				if( !found )
				{
					AddErrorToLog( "** Unknown command: " + trimmedCmdLine );
					AddErrorToLog( "** type help for commands and usage." );
				}
			}
			catch( Exception e )
			{
				AddErrorToLog( String.Format( "Exception while processing \"{0}\" : {1}", trimmedCmdLine, e.Message ) );
			}
            return;
        }

		/// <summary>Handle set/get cvar</summary>
		/// <param name="cmds">Command Line</param>
		/// <param name="cmd">Pre-processed command</param>
		/// <returns>true if command was handled by this function</returns>
		private bool HandleCVar( String[] cmds, String cmd )
		{
			CVar	cvar;
			bool	found = false;

			lock( m_cVars )
			{
				if( m_cVars.TryGetValue( cmd, out cvar ) )
				{
					found = true;
					if( cmds.Length == 1 )
					{
						AddOutputToLog( String.Format( "{0} ({1}) = {2}",
								cvar.Name, cvar.ValueType.ToString(), cvar.Value.ToString() ) );
					}
					else
					{
						cvar.Value = GetObjFromString( cvar.ValueType, cmds[ 1 ] );
						if( CVarModifiedEvent != null )
						{
							CVarModifiedEvent.Invoke( this, new CVarModifiedEventArgs( cvar ) );
						}
					}
				}
			}
			return( found );
		}

		/// <summary>Execute an internal function</summary>
		/// <param name="cmds">Command Line</param>
		/// <param name="cmd">Pre-processed command</param>
		/// <returns>true if command was handled by this function</returns>
		private bool ExecuteInternalFunc( String[] cmds, String cmd )
		{
			FuncObject	fs;
			bool found = false;

			if( ms_functions.TryGetValue( cmd, out fs ) )
			{
				string[] parameters = new string[ cmds.Length - 1 ];
				for( int param = 1; param < cmds.Length; param++ )
				{
					parameters[ param - 1 ] = cmds[ param ];
				}
				ExecuteFunction( fs, parameters );
				found = true;
			}
			return( found );
		}

		/// <summary>Execute a previously loaded external function</summary>
		/// <param name="cmds">Command Line</param>
		/// <param name="cmd">Pre-processed command</param>
		/// <returns>true if command was handled by this function</returns>
		private bool ExecuteExternalFunc( String[] cmds, String cmd )
		{
			ExternalFuncObject	efs;
			bool found = false;

			// If Not Found,  Try To Match Against External Functions
			if( ms_externalFunctions.TryGetValue( cmd, out efs ) )
			{
				string[] parameters = new string[ cmds.Length - 1 ];
				List<object>	cVarParams = new List<object>( 2 );
				bool			doInvoke = true;

				for( int param = 1; param < cmds.Length; param++ )
				{
					try
					{
						cVarParams.Add( m_cVars[ cmds[ param ].ToLowerInvariant() ] );
					}
					catch( Exception ex )
					{
						doInvoke = false;
						AddErrorToLog( "Exception while building parameters for external function: " + ex.Message );
					}
				}
				if( doInvoke )
				{
					string ret = efs.Invoke( cVarParams.ToArray() );

					if( CVarModifiedEvent != null )
					{
						foreach( CVar invokedVar in cVarParams )
						{
							CVarModifiedEvent.Invoke( this,
									new CVarModifiedEventArgs( invokedVar ) );
						}
					}
					if( String.IsNullOrWhiteSpace( ret ) == false )
					{
						AddOutputToLog( ret );
					}
				}
				found = true;
			}
			return found;
		}

		/// <summary>Execute a normal command</summary>
		/// <param name="cs">Command Object</param>
		/// <param name="cmds">Command Line</param>
		/// <param name="cmd">Pre-processed command</param>
		/// <returns>true if command was handled by this function</returns>
		private bool ExecuteCommand( CmdObject cs, String[] cmds, String cmd )
		{
			int parameters = ( cmds.Length - 1 );

			// Validate the parameter counts
			if( ( cs.MinParameters != 0 ) && ( parameters < cs.MinParameters ) )
			{
				AddErrorToLog( "Not enough parameters specified for the " + cmd + " command." );
				if( !String.IsNullOrEmpty( cs.CommandHelpDetailed ) )
				{
					AddToLog( cs.CommandHelpDetailed );
				}
			}
			else if( ( cs.MaxParameters != 0 ) && ( parameters > cs.MaxParameters ) )
			{
				AddErrorToLog( "Too many parameters specified for the " + cmd + " command." );
				if( !String.IsNullOrEmpty( cs.CommandHelpDetailed ) )
				{
					AddToLog( cs.CommandHelpDetailed );
				}
			}
			else
			{
				// Fire The Delegate!
				cs.TriggerEvent( cmds );
			}
			return( true );
		}
        #endregion

		#region Parsing Helpers
		/// <summary>Try to extract a double/Double value from the string</summary>
		/// <param name="value">Value to parse</param>
		/// <param name="parsedValue">out - receives the value parsed from the string</param>
		/// <param name="errorIfParsingFails">Error msg to log if parsing fails</param>
		/// <returns>True if parsed successfully, false otherwise</returns>
		protected bool TryParseDouble( string value, out double parsedValue, string errorIfParsingFails )
		{
			if( !Double.TryParse( value, out parsedValue ) )
			{
				AddErrorToLog( errorIfParsingFails );
				return ( false );
			}
			return ( true );
		}
		/// <summary>Try to extract a float/Single value from the string</summary>
		/// <param name="value">Value to parse</param>
		/// <param name="parsedValue">out - receives the value parsed from the string</param>
		/// <param name="errorIfParsingFails">Error msg to log if parsing fails</param>
		/// <returns>True if parsed successfully, false otherwise</returns>
		protected bool TryParseFloat( string value, out float parsedValue, string errorIfParsingFails )
		{
			if( !Single.TryParse( value, out parsedValue ) )
			{
				AddErrorToLog( errorIfParsingFails );
				return ( false );
			}
			return ( true );
		}
		/// <summary>Try to extract an int/Int32 value from the string</summary>
		/// <param name="value">Value to parse</param>
		/// <param name="parsedValue">out - receives the value parsed from the string</param>
		/// <param name="errorIfParsingFails">Error msg to log if parsing fails</param>
		/// <returns>True if parsed successfully, false otherwise</returns>
		protected bool TryParseInt( string value, out int parsedValue, string errorIfParsingFails )
		{
			if( !Int32.TryParse( value, out parsedValue ) )
			{
				AddErrorToLog( errorIfParsingFails );
				return ( false );
			}
			return ( true );
		}
		#endregion
		#region Standard Command Handlers
        void CommandConsoleBase_Help( string[] cmdLine )
        {
            if( cmdLine.Length == 1 )
            {
                StringBuilder sb = new StringBuilder( 512 );
                bool first = true;

                sb.Append( "Commands: " );
                foreach( CmdObject cs in ms_commands.Values )
                {
                    if( ( String.IsNullOrEmpty( cs.Command ) ) ||
                            ( cs.IsSecret ) )
                    {
                        continue;
                    }
                    if( !first )
                    {
                        sb.Append( ", " );
                    }
                    first = false;
                    sb.Append( cs.Command );
                }
                AddToLog( sb.ToString() );
            }
            else if( cmdLine.Length == 2 )
            {
                CmdObject	cs;
				String		helpCommand = cmdLine[ 1 ];
                bool		found = false;

				if( ms_commands.TryGetValue( helpCommand, out cs ) )
                {
                    AddToLog( cs.Command + " - " + cs.CommandHelp );
                    if( !String.IsNullOrEmpty( cs.CommandHelpDetailed ) )
                    {
                        AddToLog( cs.CommandHelpDetailed );
                    }
                    found = true;
                }
                if( !found )
                {
                    AddErrorToLog( "Unknown command: " + helpCommand );
                }
            }
            else
            {
                AddErrorToLog( "Too many parameters specified.  Try help or help <command>." );
            }
            return;
        }
        void CommandConsoleBase_Quit( string[] cmdLine )
        {
			// Stop -- no questions asked!
			Game.Exit();

            return;
        }
        void CommandConsoleBase_Exec( string[] cmdLine )
        {
			string	scriptFile = cmdLine[ 1 ];

			if( !File.Exists( scriptFile ) )
			{
				AddErrorToLog( "Unable to execute " + scriptFile + ", the file cannot be found." );
				return;
			}
			string[] commandLines = File.ReadAllLines( cmdLine[ 1 ] );

			if( commandLines.Length == 0 )
			{
				AddErrorToLog( "File: " + scriptFile + " contains no lines." );
				return;
			}
			AddOutputToLog( "<--- Starting execution of commands from: " + scriptFile + " at " + DateTime.Now.ToString( "hh:mm:ss.fff" ) );

			bool	bAddToHistory = commandLines[ 0 ].StartsWith( "!!" );
			
			foreach( string cmd in commandLines )
            {
                string cmdFixed = cmd.Trim();

                AddToLog( "\x01-> " + cmd );
                if( cmdFixed.Length == 0 )
                {
                    continue;
                }
                m_logStart = 0;
                m_commandLine = cmdFixed;
				ExecuteCommandLine( bAddToHistory );
                m_commandLine = String.Empty;
            }
            AddOutputToLog( "<--- Ended execution of commands from: " + scriptFile + " at " + DateTime.Now.ToString( "hh:mm:ss.fff" ) );

            return;
        }
        void CommandConsoleBase_SaveLog( string[] cmdLine )
        {
            string[] lines = new String[ m_log.Count ];

            m_log.CopyTo( lines );
            File.WriteAllLines( cmdLine[ 1 ], lines ); 
            AddOutputToLog( "<--- Log has been written to: " + cmdLine[ 1 ] );

            return;
        }
        void CommandConsoleBase_GC( string[] cmdLine )
        {
            AddOutputToLog( "Forcing Garbage Collection..." );
            DateTime now = DateTime.Now;

            GC.Collect( 3, GCCollectionMode.Forced );
            GC.Collect( 2, GCCollectionMode.Forced );
            GC.Collect( 1, GCCollectionMode.Forced );

            TimeSpan delta = DateTime.Now.Subtract( now );
            AddOutputToLog( "Garbage Collection took ~" + delta.TotalMilliseconds + "ms" );

            return;
        }
        void CommandConsoleBase_MemInfo( string[] cmdLine )
        {
            AddOutputToLog( "WSMem: " + Environment.WorkingSet );
            AddOutputToLog( "GCC1 : " + GC.CollectionCount( 0 ) );
            AddOutputToLog( "GCC2 : " + GC.CollectionCount( 1 ) );
            AddOutputToLog( "GCC3 : " + GC.CollectionCount( 2 ) );
            AddOutputToLog( "Total: " + GC.GetTotalMemory( false ) );

            return;
        }

		void CommandConsoleBase_ExFunc( string[] cmdLine )
        {
			if( m_exfunsLocked )
			{
				AddErrorToLog( "ExFunc-related actions have been disabled." );
				return;
			}
			string	assembly = cmdLine[ 1 ];
			string	className = cmdLine[ 2 ];
			string	funcName = cmdLine[ 3 ];
			bool	isSecret = false;

			isSecret = ( ( cmdLine.Length == 5 ) && 
					( cmdLine[ 4 ].ToLowerInvariant() == "secret" ) );
			try
			{
				ExternalFuncObject	efo = new ExternalFuncObject( assembly, className, funcName );

				efo.CommandLine = String.Format(
						"exfunc {0} {1} {2}{3}",
						assembly, className, funcName,
						isSecret ? "secret" : String.Empty );
				ms_externalFunctions.Add( funcName.ToLowerInvariant(), efo );
				AddOutputToLog( String.Format( 
						"Added external function {0}::{1}::{2}", 
						assembly, className, funcName ) );
			}
			catch( Exception ex )
			{
				AddErrorToLog( String.Format( 
						"Unable to load/add {0}::{1}::{2} = {3}", 
						assembly, className, funcName, ex.Message ) );
			}
            return;
        }

		void CommandConsoleBase_SystemInfo( string[] cmdLine )
        {
			AddOutputToLog( "Name         : " + Environment.MachineName );
			AddOutputToLog( "OSVer        : " + Environment.OSVersion );
			AddOutputToLog( "64BitOS      : " + Environment.Is64BitOperatingSystem );
			AddOutputToLog( "64BitProcess : " + Environment.Is64BitProcess );
			AddOutputToLog( "PageFile     : " + Environment.SystemPageSize );
            AddOutputToLog( "CPUs         : " + Environment.ProcessorCount );
			AddOutputToLog( "CLRVer       : " + Environment.Version );

            return;
        }

		/// <summary>Type converter helper - tries to convert the specified string to the specified type</summary>
		/// <param name="type">The target type to convert to</param>
		/// <param name="mystring">The source string to try to convert</param>
		/// <returns>A reference to the successfully converted object, or null if conversion failed</returns>
		public static object GetObjFromString( Type type, string mystring )
		{
			var foo = TypeDescriptor.GetConverter( type );
			
			// Try To Convert From String To The Target Type Using Both Culture-Sensitive
			// And The Culture-Insensitive Conversion.  If Both Fail, Default To String
			// Type.
			try
			{
				return ( foo.ConvertFromString( mystring ) );
			}
			catch( Exception )
			{
				try
				{
					return ( foo.ConvertFromInvariantString( mystring ) );
				}
				catch( Exception )
				{
					// Just Eat This One
				}
			}
			// If Conversion Fails, Fall Back To String Type
			return ( mystring );
		}
		
		void CommandConsoleBase_CVar( string[] cmdLine )
		{
			CVar	cvar = null;

			lock( m_cVars )
			{
				string	cvarName = cmdLine[ 1 ].ToLowerInvariant();
				
				if( m_cVars.TryGetValue( cvarName, out cvar ) )
				{
					AddErrorToLog( "CVar " + cmdLine[ 1 ] + 
							" already exists with a value of: " + cvar.Value ?? "(null" );
					return;
				}
				if( cmdLine.Length == 4 )
				{
					string	typeName = cmdLine[ 2 ];
					Type	cvarType = Type.GetType( typeName, false );

					if( cvarType == null )
					{
						cvarType = Type.GetType( "System." + typeName, false );
					}
					if( cvarType == null )
					{
						AddErrorToLog( "Cannot resolve type " + typeName + 
								" -- try a fully qualified type name" );
						return;
					}
					object	objValue = GetObjFromString( cvarType, cmdLine[ 3 ] );

					cvar = new CVar( cvarName, objValue );
				}
				else if( cmdLine.Length == 3 )
				{
					cvar = new CVar( cvarName, cmdLine[ 2 ] );
				}
				else
				{
					cvar = new CVar( cvarName );
				}
				m_cVars[ cvarName ] = cvar;

				AddOutputToLog( "CVar " + cvarName + " has been added" );
			}
			if( ( cvar != null ) && ( CVarModifiedEvent != null ) )
			{
                CVarModifiedEvent.Invoke(this, new CVarModifiedEventArgs(cvar));
			}
			return;
		}
		void CommandConsoleBase_CVars( string[] cmdLine )
        {
			AddOutputToLog( "cvars:" );
			lock( m_cVars )
			{
				foreach( CVar cvar in m_cVars.Values )
				{
					if( cvar.Value != null )
					{
						AddOutputToLog( String.Format( "{0} ({1}) = {2}",
								cvar.Name, cvar.ValueType.ToString(), cvar.Value.ToString() ) );
					}
					else
					{
						AddOutputToLog( cvar.Name + " (null) = null" );
					}
				}
			}
            return;
        }
		void CommandConsoleBase_ConsoleScale( string[] cmdLine )
        {
            if( cmdLine.Length < 2 )
            {
                AddOutputToLog( "Current ConsoleScale is: " + Scale );
                return;
            }
            float newScale;

			if( !TryParseFloat( cmdLine[ 1 ], out newScale, 
					"Invalid scale - the specified value cannot be parsed" ) )
			{
				return;
			}
            if( ( newScale < 0.10f ) || ( newScale > 4.0f ) )
            {
                AddErrorToLog( "Invalid scale - must be between 0.10 (10%) and 4.0 (400%)" );
            }
            else
            {
                Scale = newScale;
                m_stringHeight = Vector2.Zero;
            }
            return;
        }
		void CommandConsoleBase_ConsoleHeight( string[] cmdLine )
        {
            if( cmdLine.Length < 2 )
            {
                AddOutputToLog( "Current ConsoleHeight value is: " + m_height );
                return;
            }
            int newHeight;

			if( !TryParseInt( cmdLine[ 1 ], out newHeight, 
					"Invalid height - the specified value cannot be parsed" ) )
			{
				return;
			}
            if( ( newHeight < 100 ) || ( newHeight > ( GraphicsDevice.Viewport.Height - 100 ) ) )
            {
                AddErrorToLog( "Invalid height - must be between 100 and " + ( GraphicsDevice.Viewport.Height - 100 ) );
            }
            else
            {
                m_height = newHeight;
                m_stringHeight = Vector2.Zero;
            }
            return;
        }
        void CommandConsoleBase_Sleep( string[] cmdLine )
        {
            int sleepms;
			
			if( !TryParseInt( cmdLine[ 1 ], out sleepms, 
					"Invalid sleep duration - the specified value cannot be parsed" ) )
			{
				return;
			}
            Thread.Sleep( sleepms );

            return;
        }
        void CommandConsoleBase_LogLimit( string[] cmdLine )
        {
            if( cmdLine.Length < 2 )
            {
                AddOutputToLog( "Current LogLimit value is: " + m_logLimit );
                return;
            }
            int lines;

			if( !TryParseInt( cmdLine[ 1 ], out lines,
					"Invalid line count - the specified value cannot be parsed" ) )
			{
				return;
			}
			if( ( lines < 32 ) || ( lines > 10240 ) )
            {
                AddErrorToLog( "Invalid line count - must be between 32 and 10240" );
            }
            else
            {
                m_logLimit = lines;
            }
            return;
        }
        void CommandConsoleBase_Close( string[] cmdLine )
        {
            Active = false;
            
            return;
        }

        void CommandConsoleBase_ShadowLog( string[] cmdLine )
        {
            if( cmdLine.Length == 1 )
            {
                if( m_logShadowEnabled )
                {
                    AddOutputToLog( "Log shadowing is enabled, writing to: " + m_logShadowFilePath );
                    m_logShadowFile.Flush();
                }
                else
                {
                    AddOutputToLog( "Log shadowing is disabled." );
                }
                return;
            }
            string filePath = cmdLine[ 1 ];
            bool truncate = false;

            if( cmdLine.Length == 3 )
            {
                if( ( bool.TryParse( cmdLine[ 2 ], out truncate ) ) && ( truncate ) )
                {
                    File.Delete( cmdLine[ 2 ] );
                }
            }
            m_logShadowFile = new StreamWriter( filePath, !truncate, Encoding.ASCII );
            m_logShadowFilePath = filePath;
            m_logShadowEnabled = true;

            AddOutputToLog( "Log shadowing has been enabled." );

            return;
        }
        void CommandConsoleBase_NoShadowLog( string[] cmdLine )
        {
            if( m_logShadowEnabled )
            {
                m_logShadowFile.Close();
                m_logShadowFile = null;
                m_logShadowEnabled = false;
                AddOutputToLog( "Log shadowing has been disabled." );
            }
            else
            {
                AddOutputToLog( "Log shadowing was not enabled." );
            }
            return;
        }
        void CommandConsoleBase_Clear( string[] cmdLine )
        {
            m_log.Clear();

            return;
        }

		void CommandConsoleBase_NoFunctions( string[] cmdLine )
		{
			m_functionsLocked = true;
			return;
		}

		void CommandConsoleBase_NoBindings( string[] cmdLine )
		{
			m_bindingsLocked = true;
			return;
		}

		void CommandConsoleBase_NoExFunctions( string[] cmdLine )
		{
			m_exfunsLocked = true;
			return;
		}

		void CommandConsoleBase_Bindings( string[] cmdLine )
        {
            if( ms_bindings.Count == 0 )
            {
                AddOutputToLog( "No bindings have been set." );
				return;
            }
			StringBuilder sb = new StringBuilder( 255 );
			bool first = true;

			sb.Append( "Bindings: " );
			foreach( BindingObject bs in ms_bindings )
			{
				if( !first )
				{
					sb.Append( ", " );
				}
				first = false;
				sb.Append( bs.ToString() );
			}
			AddToLog( sb.ToString() );
            return;
        }

		void CommandConsoleBase_ResetBindings( string[] cmdLine )
		{
			if( m_bindingsLocked )
			{
				AddErrorToLog( "Binding-related actions have been disabled." );
				return;
			}
			if( ms_bindings.Count == 0 )
			{
				AddOutputToLog( "No bindings have been set." );
				return;
			}
			ms_bindings.Clear();

			AddOutputToLog( "All bindings have been cleared." );
				return;
		}

		void CommandConsoleBase_ResetFunctions( string[] cmdLine )
		{
			if( m_functionsLocked )
			{
				AddErrorToLog( "Function-related actions have been disabled." );
				return;
			}
			if( ms_functions.Count == 0 )
			{
				AddOutputToLog( "No functions have been created." );
				return;
			}
			ms_functions.Clear();

			AddOutputToLog( "All functions have been cleared." );
			return;
		}

		void CommandConsoleBase_ResetExFuncs( string[] cmdLine )
		{
			if( m_exfunsLocked )
			{
				AddErrorToLog( "ExFunc-related actions have been disabled." );
				return;
			}
			if( ms_externalFunctions.Count == 0 )
			{
				AddOutputToLog( "No external functions have been added." );
				return;
			}
			ms_externalFunctions.Clear();

			AddOutputToLog( "All external functions have been cleared." );
			return;
		}

		void CommandConsoleBase_ExFuncs( string[] cmdLine )
		{
			if( m_exfunsLocked )
			{
				AddErrorToLog( "ExFunc-related actions have been disabled." );
				return;
			}
			if( ms_externalFunctions.Count == 0 )
			{
				AddOutputToLog( "No external functions have been added." );
				return;
			}
			StringBuilder sb = new StringBuilder( 255 );
			bool first = true;

			sb.Append( "ExFuncs: " );
			foreach( ExternalFuncObject ef in ms_externalFunctions.Values )
			{
				if( !first )
				{
					sb.Append( ", " );
				}
				first = false;
				sb.Append( ef.ToString() );
			}
			AddToLog( sb.ToString() );
			return;
		}

		void CommandConsoleBase_Functions( string[] cmdLine )
        {
			if( ms_functions.Count == 0 )
            {
                AddOutputToLog( "No functions have been created." );
				return;
			}
			StringBuilder sb = new StringBuilder( 255 );
			bool first = true;

			sb.Append( "Functions: " );
			foreach( FuncObject fs in ms_functions.Values )
			{
				if( !first )
				{
					sb.Append( ", " );
				}
				first = false;
				sb.Append( fs.ToString() );
			}
			AddToLog( sb.ToString() );
            return;
        }

		private List<string> GetAllExFuncs()
		{
			List<string> exFuncs = new List<string>( ms_externalFunctions.Count );

			foreach( ExternalFuncObject efo in ms_externalFunctions.Values )
			{
				exFuncs.Add( efo.CommandLine );
			}
			return( exFuncs );
		}

		private List<string> GetAllFunctions()
        {
            List<string> functions = new List<string>( ms_functions.Count );

            foreach( FuncObject fs in ms_functions.Values )
            {
                string fnText = "function " + 
							fs.ToString().Replace( "-> ", String.Empty );

				//foreach( string fnline in fs.FunctionImpl )
				//{
				//    fnText += fnline;
				//    fnText += ";";
				//}
				//functions.Add( "function " + fs.Function + " " + fnText );
				functions.Add( fnText );
            }
            return( functions );
        }

        private List< string > GetAllBindings()
        {
            List<string> bindings = new List<string>( ms_bindings.Count );

            foreach( BindingObject bo in ms_bindings )
            {
                bindings.Add( "bind " + bo.GetModifierString() + bo._key + " " + bo._text );
            }
            return( bindings );
        }

        void CommandConsoleBase_ExportBindings( string[] cmdLine )
        {
			if( m_bindingsLocked )
			{
				AddErrorToLog( "Binding-related actions have been disabled." );
				return;
			}
			if( ms_bindings.Count == 0 )
            {
                AddErrorToLog( "No bindings have been set." );
                return;
            }
            string filePath = cmdLine[ 1 ];

            File.WriteAllLines( filePath, GetAllBindings().ToArray() );

            AddOutputToLog( "Bindings have been saved to: " + filePath );

            return;
        }

        void CommandConsoleBase_ExportFunctions( string[] cmdLine )
        {
			if( m_functionsLocked )
			{
				AddErrorToLog( "Function-related actions have been disabled." );
				return;
			}
			if( ms_functions.Count == 0 )
            {
                AddErrorToLog( "No functions have been set." );
                return;
            }
            string filePath = cmdLine[ 1 ];

            File.WriteAllLines( filePath, GetAllFunctions().ToArray() );
            AddOutputToLog( "Functions have been saved to: " + filePath );

            return;
        }

		void CommandConsoleBase_ExportExFuncs( string[] cmdLine )
		{
			if( m_exfunsLocked )
			{
				AddErrorToLog( "ExFunc-related actions have been disabled." );
				return;
			}
			if( ms_externalFunctions.Count == 0 )
			{
				AddErrorToLog( "No ExFuncs have been set." );
				return;
			}
			string filePath = cmdLine[ 1 ];

			File.WriteAllLines( filePath, GetAllExFuncs().ToArray() );
			AddOutputToLog( "ExFunc defs have been saved to: " + filePath );

			return;
		}

		/// <summary>Special command handler for ExportState command</summary>
		/// <param name="cmdLine">The command line for the command</param>
		/// <remarks>This one is protected virtual so additional state can be stored if necessary by derived classes</remarks>
		protected virtual void CommandConsoleBase_ExportState( string[] cmdLine )
        {
            string filePath = cmdLine[ 1 ];
            StreamWriter outFile = new StreamWriter( filePath, false, Encoding.ASCII );

            if( ms_bindings.Count > 0 )
            {
                lock( ms_bindings )
                {
                    List<string> bindings = GetAllBindings();

                    foreach( string line in bindings )
                    {
                        outFile.WriteLine( line );
                    }
                }
            }
            if( ms_functions.Count > 0 )
            {
                lock( ms_functions )
                {
                    List<string> functions = GetAllFunctions();

                    foreach( string line in functions )
                    {
                        outFile.WriteLine( line );
                    }
                }
            }
            outFile.Write( "consoleheight " + m_height );
            outFile.Write( "consolescale " + Scale );
            outFile.Write( "loglimit " + m_logLimit );
            outFile.Close();
            outFile.Dispose();


            AddOutputToLog( "State has been saved to: " + filePath );

            return;
        }

        void CommandConsoleBase_UnBind( string[] cmdLine )
        {
            string bindString = cmdLine[ 1 ];
            Keys bindKey = Keys.None;
            BindingObject.EModifier bindKeyModifiers = BindingObject.EModifier.None;
            bool parsed = GetKeyBindingInfo( bindString, ref bindKeyModifiers, ref bindKey );

            if( !parsed )
            {
                AddErrorToLog( String.Format( "\"{0}\" was not understood as a proper binding string.", bindString ) );
                return;
            }
            foreach( BindingObject bs in ms_bindings )
            {
                if( ( bindKey == bs._key ) && ( bindKeyModifiers == bs._modifierKeys ) )
                {
                    ms_bindings.Remove( bs );
                    AddOutputToLog( bs.GetModifierString() + " has been unbound." );
                    return;
                }
            }
            AddErrorToLog( bindString + " is not bound." );

            return;
        }

        void CommandConsoleBase_Bind( string[] cmdLine )
        {
			if( m_bindingsLocked )
			{
				AddErrorToLog( "Binding-related actions have been disabled." );
				return;
			}
			string bindString = cmdLine[ 1 ];
            BindingObject.EModifier bindKeyModifiers = BindingObject.EModifier.None;
            Keys bindKey = Keys.None;
            bool parsed = GetKeyBindingInfo( bindString, ref bindKeyModifiers, ref bindKey );

            if( !parsed )
            {
                AddErrorToLog( String.Format( "\"{0}\" was not understood as a proper binding string.", bindString ) );
                return;
            }
            foreach( BindingObject bs in ms_bindings )
            {
                if( ( bindKey == bs._key ) && ( bindKeyModifiers == bs._modifierKeys ) )
                {
                    AddErrorToLog( String.Format( "{0} is already bound.", bindString ) );
                    return;
                }
            }
            BindingObject newBS = new BindingObject();
            string cmds = String.Empty;

            for( int cmd = 2; cmd < cmdLine.Length; cmd++ )
            {
                cmds += cmdLine[ cmd ];
                cmds += " ";
            }
            newBS._key = bindKey;
            newBS._modifierKeys = bindKeyModifiers;
            newBS._text = cmds;

            ms_bindings.Add( newBS );

            return;
        }

        void CommandConsoleBase_Function( string[] cmdLine )
        {
			if( m_functionsLocked )
			{
				AddErrorToLog( "Function-related actions have been disabled." );
				return;
			}
			string cmds = String.Empty;
			bool isSecret = false;

			// This Is A Bit Heavyweight, Building A command Line And Then
			// Breaking It Apart Again, But It Seemed Easier And Less 
			// Complicated Than Trying To Do It All In One Shot
			//
			// Build Command Line Minus Command And Secret Option
			for( int cmd = 2; cmd < cmdLine.Length; cmd++ )
			{
				if( ( cmd == 2 ) && ( cmdLine[ cmd ].ToLowerInvariant() == "secret" ) )
				{
					isSecret = true;
					continue;
				}
				cmds += cmdLine[ cmd ];
                cmds += " ";
            }
			if( String.IsNullOrWhiteSpace( cmds ) )
			{
				AddErrorToLog( "Function contains no code to execute." );
				return;
			}
			string[] ft = cmds.Split( ';' );
			bool	hasCode = false;

			// Reassemble Command Lines From Tokens
			for( int line = 0; line <ft.Length; line++ )
            {
				ft[ line ] = ft[ line ].Trim().ToLowerInvariant();
				if( ft[ line ].Length == 0 )
				{
					continue;
				}
				hasCode = true;
            }
			if( !hasCode )
            {
                AddErrorToLog( "Function contains no code to execute." );
            }
            else
            {
				FuncObject newFN = new FuncObject( cmdLine[ 1 ], ft );

				newFN.IsSecret = isSecret;
				AddFunction( newFN );
            }
            return;
        }

        #endregion
    }
}