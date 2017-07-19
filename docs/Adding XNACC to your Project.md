## Adding XNACC to Your Project
To use XNACC in your own project, you must add the **CommandConsoleBase.cs** file to your project, and reference the **CommandConsoleSharedTypes** assembly (source is included for this assembly, you need to build it yourself).

**CommandConsoleBase.cs** contains the complete implementation of XNACC, sans the definitions for the {{ CVar }} (**C**onsole **Var**iable) type and the **{{ IConsoleKeyboard }}** interface, which abstracts (keyboard) input into the console.

You then derive a class from **CommandConsoleBase.cs** and can implement whatever additional functionality you need.

Your code needs to instantiate an instance of the resulting XNACC class, and hook it into your game update/input/drawing pipeline.

In order to have XNACC actually accept input, you need to implement the **{{ IConsoleKeyboard }}** interface in your input manager (for example) and provide the necessary functionality.  Use the tilde (**~**) key to show/hide the console.  

### Customizing the Console Background
For a better console background effect, be sure to set the {{ FadeImage }} property to a background image, and set the **{{ FadeColor }}** (play around with the alpha value) to get the opacity/fade you need,  A simple way to demonstrate how this works is to have a texture with a single black pixel assigned to **{{ FadeImage }}** (it will be scaled over the console area) and a **{{ FadeColor }}** of **{{ Color.Black ** 0.80f. }}**

You can even create a dynamic image, such as a Matrix-ish _green falling characters_ image and use it for a more fancy background.