## Tips and Tricks
Just a few suggestions to help get the best use out of XNACC.
# Have your game code pre-load the console's runtime environment by supporting an **{{ autoexec.xnacc }}** script file that your app automatically loads into the console during initialization.  That way, you will not have to keep setting up things like functions, bindings, loading ExFuncs, etc.
# Save and restore console state for debugging purposes, as the above autoexec suggestion and restoring of saved state can overlap.
# Add the **{{nobindings}}**, **{{nofunctions}}**, and **{{noexfuncs}}** commands to the end of your startup scripts to help _lock down_ the console.
# Remember that starting a script line with a bang (exclamation point) causes the line to NOT get echoed in the console's log.
# If the first line of a script file is a double-bang line (!!), then ALL lines in the file will NOT get echoed to the console's log.