# WARNING

This mod is more difficult than others to use.

The .jar companion file is nessecary. I will confirm when I can, but I'm not sure whether r2modman will allow .jar files to be downloaded.

This mod is in development. Expect issues. Raise an issue on the github or ontact me at discord: daemon8363, preferrably in #ultrakill-modding if you encounter problems.

# REQUIREMENTS

This mod (you must have it downloaded and enabled) 

The companion app for this mod (accessible from the plugin configurator by opening the folder) or from here: 
https://github.com/daemon251/Ultrakill-UltraRGBLighting/tree/main/UltrakillCompanion/release (the preferences file is also required for now)

OpenRGB: https://openrgb.org/

# OVERVIEW

This mod gives users a way to change the color of any RGB device with the current style rank in Ultrakill.

This mod consists of three parts: The Ultrakill plugin (this mod), a companion app, and OpenRGB. All that the Ultrakill plugin does is provide the current 
style rank in a file, which the companion app then reads from to then change colors with OpenRGB.

This mod works using OpenRGB. You need to have OpenRGB installed. OpenRGB was chosen because of its customizability and wide support.
If you run into issues with OpenRGB, you may need to run it in administrator mode.
This mod will probably work with any RGB fixture you have (anything OpenRGB supports).

This mod only supports relatively basic lighting configurations.
If you want to use more advanced configurations, you will have to make a script yourself that does your preferred behavior instead of using the companion. 
The file "output.txt" provides the style ranks in decimal, from 0 to 7 and with -1 being no rank, if you decide to make your own script. 

# HOW TO USE

You must first start OpenRGB and make sure that it can access whatever RGB devices you need (if it doesn't work the first time, run it in admin mode). 

Then in OpenRGB, switch to the "SDK Server" tab and click the "Start Server" button. The host should be 0.0.0.0, and the port 6742 (these are the default options). 

Then, you must run the companion app, located in the mod plugin's directory (folder is openable from this mod's option menu ingame). 

You must then type in the relative or absolute file path of output.txt in the mod directory. By default, "output.txt" should work.

Then type in the indexes of the device(s) you want the companion app to use and then click "connect". The index is zero-indexed and should be in the same order as listed in OpenRGB. Typing "ALL" will connect all available devices.

From there, it should work in game.

# ADVANCED

If you want to have different devices have different color schemes, you will need to run multiple instances of the companion open with each one coloring different devices.
Every instance needs to be located in a different location with a corresponding UltraRGBLightingPrefs.txt if you need it to save values.
The program currently will not generate the preferences file by itself (and will error if its malformed), which means you need to make a proper copy for each one.

