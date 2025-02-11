using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;

using UnityEngine;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System;
using UnityEngine.SocialPlatforms;
using System.Linq;
using UnityEngine.UIElements;

using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using PluginConfig.API.Functionals;

namespace UltraRGBLighting;

[BepInPlugin("UltraRGBLighting", "UltraRGBLighting", "0.0.1")]
public class Plugin : BaseUnityPlugin
{
    public static int currentRankInt = -1;
    public static float currentRankFloat = -1f;
    public float pollingTime = 0.5f; //in seconds
    string DefaultParentFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";
    string path;

    public void openRootFolder()
    {
        Application.OpenURL(DefaultParentFolder);
    }
    private void Awake()
    {       
        //Harmony harmony = new Harmony("UltraRGBLighting");
        //harmony.PatchAll();
        path = $"{Path.Combine(DefaultParentFolder!, "output.txt")}";
        var config = PluginConfigurator.Create("UltraRGBLighting", "UltraRGBLighting");
        config.SetIconWithURL($"{Path.Combine(DefaultParentFolder!, "icon.png")}");

        ConfigHeader infoHeader = new ConfigHeader(config.rootPanel, "You must open UltraRGBLighting.jar (provided in the mod directory) and OpenRGB (must be downloaded from their site) for this mod to do anything. UltraRGBLighting.jar provides configuration options. Consult the readme if confused.");
        infoHeader.textSize = 14;
        infoHeader.textColor = Color.red;

        ButtonField openFolderField = new ButtonField(config.rootPanel, "Open Mod Folder", "button.openfolder");
        openFolderField.onClick += new ButtonField.OnClick(openRootFolder);

        ConfigHeader infoHeader2 = new ConfigHeader(config.rootPanel, "Polling rate refers to the amount of times a second that the style rank is recorded. High polling rates have a negative performance impact, and at a certain point start having a negative effect. A value around 40 is strongly reccomended.");
        infoHeader2.textSize = 12;
        FloatField pollingRateField = new FloatField(config.rootPanel, "Polling Rate (Hz)", "pollingRate", 40, 0.001f, 1000f);
        pollingRateField.onValueChange += (FloatField.FloatValueChangeEvent e) => {pollingTime = 1f / e.value;};
        pollingTime = 1f / pollingRateField.value;

        Debug.Log("UltraRGBLighting.dll Loaded");
    }
    float timeElapsed = 0f;
    void Update()
    {
        //All this code does is create a file that has the data that can be read from. I would love for this .dll to use OpenRGB directly,
        //but it seems that's not possible using Unity because of some version bullshittery. 
        //regardless, hours of fiddling yielded me no results.
        //I also don't know how to directly read game variables from outside of a plugin... if thats even possible
        //thats why the companion app is required.

        //Destructive is zero, etc. No rank is also destructive just not shown.
        timeElapsed += Time.deltaTime;

        if(MonoSingleton<StyleHUD>.Instance == null) {return;}
        currentRankInt = MonoSingleton<StyleHUD>.Instance.rankIndex;
        currentRankFloat = currentRankInt + MonoSingleton<StyleHUD>.Instance.currentMeter / MonoSingleton<StyleHUD>.Instance.currentRank.maxMeter;
        if(currentRankFloat < 0) {currentRankFloat = -1;} //yes, it does go below zero. This is when no rank is shown at all.
        //writing to file every frame has a noticeable performance impact, so we need to poll.
        if(timeElapsed > pollingTime)
        {
            File.WriteAllText(path, currentRankFloat.ToString() + "\n" + (pollingTime * 1000)); 
            timeElapsed = 0f;
        }
    }
}

