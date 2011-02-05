using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

/// <summary>Namespace that contains shared types related to the XNACC (CommandConsole) component</summary>
namespace JRTS.XNA.Console.BaseTypes
{
	#region IConsoleKeyboard
	/// <summary>Basic keyboard/input functionality required by the CommandConsole(Base) class</summary>
	public interface IConsoleKeyboard
	{
		/// <summary>The current state of the keyboard as of the last Update</summary>
		KeyboardState CurrentKeyboardState
		{
			get;
		}
		/// <summary>Collection of keys that are newly pressed (i.e. not held) as of the last Update</summary>
		IList<Keys> NewlyPressedKeys
		{
			get;
		}
		/// <summary>Collection of keys that are being held fown (i.e. not newly pressed) as of the last Update</summary>
		IList<Keys> HeldKeys
		{
			get;
		}
	}
	#endregion

	#region CVar Object
	// JRT:	OK - Why Is This In Its Own File?  Because Of The Way The Type System Works In .NET.  If You Create
	// JRT:	Two Completely Identical Types, But Put Them In Different Assemblies, They Are Considered To Be
	// JRT:	Two Completely DIFFERENT Types, Even Though They Are Compatible With One Another.  By Putting
	// JRT:	The CVar Class Into A Separate Assembly, Both The CommandConsole And External Functions
	// JRT:	Reference The Same Type, Allowing Us To Pass CVars Back And Forth.
	/// <summary>Object that wraps the functionality of a console variable -- defined separately because external functions will receive CVar instances</summary>
	public class CVar
    {
		/// <summary>The storage for the Name property</summary>
		protected string m_name = String.Empty;
		/// <summary>The storage for the Value property</summary>
		protected object m_value = null;
            
        /// <summary>The string name for this console variable</summary>
        public  string  Name
        {
            get
            {
                return (m_name);
            }
            protected set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("The name for a console variable cannot be null, empty, or whitespace");
                }
                m_name = value;
            }
        }
        /// <summary>The actual value for this console variable, as an object, or null</summary>
        public  object  Value
        {
            get
            {
                return( m_value );
            }
            set
            {
                // Set Value.  Set Type To Type.Missing If Value Is null
                m_value = value;
                if (value == null)
                {
                    ValueType = (Type)Type.Missing;
                }
                else
                {
                    ValueType = value.GetType();
                }
            }
        }
        /// <summary>The actual type of the contained variable, or Type.Missing for null values</summary>
        public  Type    ValueType
        {
            get;
            protected set;
        }
        /// <summary>Construct a console variable with a default value of null</summary>
        /// <param name="name">The name for ths console variable</param>
        public  CVar(string name )
                : this( name, null )
        {
            return;
        }
        /// <summary>Construct a console variable with the specified name and value</summary>
        /// <param name="name">The name for ths console variable</param>
        /// <param name="value">The value for this console variable</param>
        public  CVar(string name, object value)
        {
            Name = name;
            Value = value;
            return;
        }
		/// <summary>Get a string representation of the object (not round-trippable!)</summary>
		/// <returns>A string representation of this CVar</returns>
		public override string ToString()
		{
			return( String.Format( "{0} ({1}) = {2}", Name, 
					ValueType.ToString(), 
					Value.ToString() ) );
		}
		/// <summary>Hash code function - needed for the dictionary</summary>
        /// <returns>The hash value of the Name of the cvar</returns>
        override public  int     GetHashCode()
        {
            return(Name.GetHashCode() );
        }
    }
    #endregion
}
