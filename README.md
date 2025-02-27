# WARNING

Updated for ULTRA_REVAMP.

This mod is more convoluted than others to use.

This mod is in development. Expect issues. Raise an issue on the github or ontact me at discord: daemon8363, preferrably in #ultrakill-modding if you encounter problems.

# REQUIREMENTS

The Ultrakill mod (you must have it downloaded and enabled) 

The companion app for this mod (accessible in the mod's directory folder which can be opened ingame from the plugin configurator section) 

OpenRGB: https://openrgb.org/

# HOW TO USE

1. Start OpenRGB and make sure that it can access whatever RGB devices you need (if it doesn't work the first time, run it in admin mode).
 
2. In OpenRGB, switch to the "SDK Server" tab and click the "Start Server" button. The host should be 0.0.0.0, and the port 6742 (these are the default options). 

3. Run the companion app, located in the mod plugin's directory (folder is openable from this mod's option menu ingame). 

4. Type in the relative or absolute file path of the output.txt file found in the mod directory. By default, "output.txt" should work (don't type the "").

5. Type in the indexes of the device(s) you want the companion app to use. Typing "ALL" will connect all available devices. If you want to only do it for specific devices, you have to type in their device index (starting from zero), which follows the order that they are listed in OpenRGB

6. Click "Connect"

From there, it should work in game.

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

# ADVANCED

If you want to have different devices have different color schemes, you will need to run multiple instances of the companion open with each one coloring different devices.
Every instance needs to be located in a different location if you need it to save values.

The .jar file can be downloaded here if needed: https://github.com/daemon251/Ultrakill-UltraRGBLighting/tree/main/UltrakillCompanion/release 
