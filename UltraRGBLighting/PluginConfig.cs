using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OpenRGB.NET;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using PluginConfig.API.Functionals;

namespace UltraRGBLighting;

public class styleRankColoringScheme
{
    public UnityEngine.Color mainColor;
    public UnityEngine.Color secondaryColor;
    public float alternateColorFreq = 0f;
    public float pulsateAmplitude = 0f; //ranging to 1
    public float pulsateFrequency = 0f; //hz
    public float flickerAmplitude = 0f; //ranging to 1
    public UnityEngine.Color flickerColor = UnityEngine.Color.black;
    public float flickerFrequency = 0f; //hz


    public colorPropertyEnum property1 = colorPropertyEnum.None;
    public UnityEngine.Color property1Color = UnityEngine.Color.black;
    public float property1Float1 = 0f;
    public float property1Float2 = 0f;
    public float property1Float3 = 0f;
    public colorPropertyEnum property2 = colorPropertyEnum.None;
    public UnityEngine.Color property2Color = UnityEngine.Color.black;
    public float property2Float1 = 0f;
    public float property2Float2 = 0f;
    public float property2Float3 = 0f;
    public colorPropertyEnum property3 = colorPropertyEnum.None;
    public UnityEngine.Color property3Color = UnityEngine.Color.black;
    public float property3Float1 = 0f;
    public float property3Float2 = 0f;
    public float property3Float3 = 0f;
}
public enum styleRankColoringSchemeEnum {None, Scheme1, Scheme2, Scheme3, Scheme4}
public enum colorPropertyEnum {None, RainbowWhole, RainbowFlickerFixed, FlickerFixed, InverseColors, Brighten, CheckerboardInverseColor, CheckerboardColor, Colorfy, SnakeColor, DoubleSnakeColor}
public class deviceColoringScheme
{
    public bool enabled = false;
    public styleRankColoringSchemeEnum scheme = styleRankColoringSchemeEnum.None;
}
public class ModConfig
{
    static string DefaultParentFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";
    public static bool gradientizeRankColors = false;
    public static void connectToOpenRGB()
    {
        OpenRgbClient client = null;
        if(!Plugin.connected || Plugin.client == null)
        {
            client = new OpenRgbClient();
            Plugin.client = client;
            try {client.Connect();}
            catch (Exception) 
            {
                //Plugin.logger.LogInfo(e.ToString());
                Plugin.logger.LogInfo("could not connect to OpenRGB");
            }
            Plugin.logger.LogInfo("Connected to OpenRGB");
        }
        else {client = Plugin.client;}

        var plugins = client.GetPlugins();

        var devices = client.GetAllControllerData();

        var profiles = client.GetProfiles();

        Plugin.logger.LogInfo("Found devices:");
        foreach (var device in devices)
        {
            Plugin.connectedDevices.Add(device);
            Plugin.logger.LogInfo(device.Name);
            List<OpenRGB.NET.Color> emptyColorList = new List<OpenRGB.NET.Color>(device.Leds.Length);
            for(int i = 0; i < device.Leds.Length; i++)
            {
                emptyColorList.Add(new OpenRGB.NET.Color(255, 255, 255));
            }
            Plugin.colorsBoard[device.Index] = emptyColorList; //why is ts required bruh
            //Plugin.colorsBoard[device.Index] = new List<OpenRGB.NET.Color>(device.Leds.Length);
            if(Plugin.maxLEDCount < device.Leds.Length) {Plugin.maxLEDCount = device.Leds.Length;}
        }

        Plugin.flickerIntensity[0] = new List<float>();
        Plugin.flickerIntensity[1] = new List<float>();
        Plugin.flickerIntensity[2] = new List<float>();
        Plugin.flickerIntensity[3] = new List<float>();
        Plugin.flickerIntensityFixed = new List<float>();
        for(int i = 0; i < Plugin.maxLEDCount; i++)
        {
            Plugin.flickerIntensity[0].Add(0f);
            Plugin.flickerIntensity[1].Add(0f);
            Plugin.flickerIntensity[2].Add(0f);
            Plugin.flickerIntensity[3].Add(0f);
            Plugin.flickerIntensityFixed.Add(0f);
        }

        Plugin.connected = true;
        refillConnectedDevicePanels();
    }
    public static ConfigPanel connectedDevicesPanel;
    public static ButtonField connectButton;
    public static bool autoConnect = false;
    public static void createConfig()
    {
        var config = PluginConfigurator.Create("UltraRGBLighting", "UltraRGBLighting");
        config.SetIconWithURL($"{Path.Combine(DefaultParentFolder!, "icon.png")}");

        connectButton = new ButtonField(config.rootPanel, "Connect", "button.connectButton");
        connectButton.onClick += new ButtonField.OnClick(connectToOpenRGB);

        BoolField enabledField = new BoolField(config.rootPanel, "Mod Enabled", "modEnabled", true);
        ConfigDivision division = new ConfigDivision(config.rootPanel, "division");
        enabledField.onValueChange += (BoolField.BoolValueChangeEvent e) => {Plugin.modEnabled = e.value; division.interactable = e.value;};
        Plugin.modEnabled = enabledField.value; division.interactable = enabledField.value;

        BoolField autoConnectField = new BoolField(division, "Attempt Autoconnect Periodically", "autoconnectfield", false);
        autoConnectField.onValueChange += (BoolField.BoolValueChangeEvent e) => {autoConnect = e.value;};
        autoConnect = autoConnectField.value;

        BoolField gradientizeField = new BoolField(division, "Gradientize Colors between Ranks", "stylerankcolorsgradientize", false);
        gradientizeField.onValueChange += (BoolField.BoolValueChangeEvent e) => {gradientizeRankColors = e.value;};
        gradientizeRankColors = gradientizeField.value;

        FloatField flickerFixedField = new FloatField(division, "Fixed Flicker Frequency", "fixedflickerfrequency", 0f, 0f, 1000f);
        flickerFixedField.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.flickerIntensityFixedFrequency = e.value;};
        Plugin.flickerIntensityFixedFrequency = flickerFixedField.value;

        connectedDevicesPanel = new ConfigPanel(division, "Connected Devices Settings", "connectedDevicesPanel");
        fillConnectedDevicesPanel(connectedDevicesPanel);

        ConfigPanel styleRankSettingsPanel = new ConfigPanel(division, "Style Rank Settings", "styleranksettingsPanel");
        for(int i = 0; i < 4; i++)
        {
            createEntireStyleColoringPanel(styleRankSettingsPanel, i);
        }
        ConfigPanel permutatorSettingsPanel = new ConfigPanel(division, "Permutator Settings", "permutatorsettingsPanel");
        new ConfigHeader(permutatorSettingsPanel, "Permutators are handled sequentially in order", 15);
        for(int i = 0; i < 15; i++)
        {
            createPermutatorPanel(permutatorSettingsPanel, i);
        }

        new ConfigHeader(division, "Increase refresh rate for more responsiveness on RGB device, decrease to reduce load on game.", 18);

        //will be a little lower than this, very low impact at higher fps
        FloatField RefreshRateField = new FloatField(division, "Refresh rate (Hz)", "refreshRate", 50f, 0.1f, 1000f);
        RefreshRateField.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.refreshRate = e.value;};
        Plugin.refreshRate = RefreshRateField.value;
    }

    public static void changePermutatorPanel(PermutatorCause pc, colorPropertyEnum property, BoolField boolField, FloatField lengthField, FloatField ff1, FloatField ff2, FloatField ff3, ColorField colorField)
    {
        changePropertyFieldNames(property, ff1, ff2, ff3, colorField);

        if     (pc == PermutatorCause.None)             {boolField.interactable = false; boolField.displayName = "N/A"; lengthField.displayName = "Effect Duration";}
        else if(pc == PermutatorCause.RecentDamage)     {boolField.interactable = false; boolField.displayName = "N/A"; lengthField.displayName = "Damage Sensitivity";}
        else if(pc == PermutatorCause.RecentKills)      {boolField.interactable = false; boolField.displayName = "N/A"; lengthField.displayName = "Kill Sensitivity";}
        else if(pc == PermutatorCause.RecentStyle)      {boolField.interactable = false; boolField.displayName = "N/A"; lengthField.displayName = "Style Sensitivity";}
        else if(pc == PermutatorCause.OnDamage)         {boolField.interactable = true; boolField.displayName = "Scale Time With Damage"; lengthField.displayName = "Effect Duration";}
        else if(pc == PermutatorCause.OnKill)           {boolField.interactable = true; boolField.displayName = "Scale Time With Kill Size"; lengthField.displayName = "Effect Duration";}
        else if(pc == PermutatorCause.OnPlayerDeath)    {boolField.interactable = false; boolField.displayName = "N/A"; lengthField.displayName = "Effect Duration";}
        else if(pc == PermutatorCause.OnRankGain)       {boolField.interactable = false; boolField.displayName = "N/A"; lengthField.displayName = "Effect Duration";}
        else if(pc == PermutatorCause.OnRankLose)       {boolField.interactable = false; boolField.displayName = "N/A"; lengthField.displayName = "Effect Duration";}
        else if(pc == PermutatorCause.OnStyleGain)      {boolField.interactable = true; boolField.displayName = "Scale With Style Gained"; lengthField.displayName = "Effect Duration";}
        else if(pc == PermutatorCause.OnTakeDamage)     {boolField.interactable = true; boolField.displayName = "Scale With Damage Taken"; lengthField.displayName = "Effect Duration";}
        else if(pc == PermutatorCause.Always)           {boolField.interactable = false; boolField.displayName = "N/A"; lengthField.displayName = "Effect Duration";}
    }

    public static void createPermutatorPanel(ConfigPanel rootPanel, int i)
    {
        ConfigPanel newPanel = new ConfigPanel(rootPanel, "Permutator " + (i + 1), "permutatorPanel" + i);
        EnumField<PermutatorCause> causeField = new EnumField<PermutatorCause>(newPanel, "Permutator Cause", "permutatorCause" + i, PermutatorCause.None);

        BoolField scaleWithCauseField = new BoolField(newPanel, "Scale With Cause Magnitude", "permutatorscalecause" + i, false);
        scaleWithCauseField.onValueChange += (BoolField.BoolValueChangeEvent e) => {Permutators.permutatorArr[i].scaleWithCauseMagnitude = e.value;};
        Permutators.permutatorArr[i].scaleWithCauseMagnitude = scaleWithCauseField.value;

        EnumField<colorPropertyEnum> effectField = new EnumField<colorPropertyEnum>(newPanel, "Permutator Effect", "permutatorEffect" + i, colorPropertyEnum.None);

        EnumField<FadeEnum> fadeField = new EnumField<FadeEnum>(newPanel, "Permutator Fade Type", "permutatorfade" + i, FadeEnum.Linear);
        fadeField.onValueChange += (EnumField<FadeEnum>.EnumValueChangeEvent e) => {Permutators.permutatorArr[i].fadeType = e.value;};
        Permutators.permutatorArr[i].fadeType = fadeField.value;

        FloatField lengthField = new FloatField(newPanel, "Effect Duration", "permutatorEffectduration" + i, 0f, 0f, 100f);
        lengthField.onValueChange += (FloatField.FloatValueChangeEvent e) => {Permutators.permutatorArr[i].length = e.value;};
        Permutators.permutatorArr[i].length = lengthField.value;

        ColorField colorField = new ColorField(newPanel, "Effect Color (if applicable)", "permutatorEffectColor" + i, UnityEngine.Color.white);
        colorField.onValueChange += (ColorField.ColorValueChangeEvent e) => {Permutators.permutatorArr[i].propertyColor = e.value;};
        Permutators.permutatorArr[i].propertyColor = colorField.value;

        FloatField propertyValue1Field = new FloatField(newPanel, "Value 1", "permutatorfloat1" + i, 0f, 0f, 1000f);
        propertyValue1Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Permutators.permutatorArr[i].propertyFloat1 = e.value;};
        Permutators.permutatorArr[i].propertyFloat1 = propertyValue1Field.value;

        FloatField propertyValue2Field = new FloatField(newPanel, "Value 2", "permutatorfloat2" + i, 0f, 0f, 1000f);
        propertyValue2Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Permutators.permutatorArr[i].propertyFloat2 = e.value;};
        Permutators.permutatorArr[i].propertyFloat2 = propertyValue2Field.value;

        FloatField propertyValue3Field = new FloatField(newPanel, "Value 3", "permutatorfloat3" + i, 0f, 0f, 1000f);
        propertyValue3Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Permutators.permutatorArr[i].propertyFloat3 = e.value;};
        Permutators.permutatorArr[i].propertyFloat3 = propertyValue3Field.value;

        causeField.onValueChange += (EnumField<PermutatorCause>.EnumValueChangeEvent e) => {Permutators.permutatorArr[i].cause = e.value; changePermutatorPanel(e.value, effectField.value, scaleWithCauseField, lengthField, propertyValue1Field, propertyValue2Field, propertyValue3Field, colorField);};
        Permutators.permutatorArr[i].cause = causeField.value;

        effectField.onValueChange += (EnumField<colorPropertyEnum>.EnumValueChangeEvent e) => {Permutators.permutatorArr[i].property = e.value; changePermutatorPanel(causeField.value, e.value, scaleWithCauseField, lengthField, propertyValue1Field, propertyValue2Field, propertyValue3Field, colorField);};
        Permutators.permutatorArr[i].property = effectField.value; 
        
        changePermutatorPanel(causeField.value, effectField.value, scaleWithCauseField, lengthField, propertyValue1Field, propertyValue2Field, propertyValue3Field, colorField);
    }
    public static void createEntireStyleColoringPanel(ConfigPanel rootPanel, int i)
    {
        ConfigPanel configPanelX = new ConfigPanel(rootPanel, "Style Rank Colors Config " + (i + 1), "configPanelX" + i);
        for(int j = 0; j < 9; j++)
        {
            createStyleRankColoringSchemePanel(configPanelX, i, j);
        }
    }
    public static string[] styleNames = {"No Rank", "Destructive", "Chaotic", "Brutal", "Anarchic", "Supreme", "SSadistic", "SSShitstorm", "ULTRAKILL"};
    public static UnityEngine.Color[] styleColors = {UnityEngine.Color.grey, UnityEngine.Color.blue, UnityEngine.Color.green, UnityEngine.Color.yellow, UnityEngine.Color.magenta, UnityEngine.Color.red, UnityEngine.Color.red, UnityEngine.Color.red, UnityEngine.Color.white};
    
    public static void changePropertyFieldNames(colorPropertyEnum property, FloatField ff1, FloatField ff2, FloatField ff3, ColorField colorField)
    {
        ff3.displayName = "N/A"; ff3.interactable = false;
        if     (property == colorPropertyEnum.None) {ff1.displayName = "N/A"; ff2.displayName = "N/A"; ff1.interactable = false; ff2.interactable = false; colorField.interactable = false;}
        else if(property == colorPropertyEnum.RainbowWhole) {ff1.displayName = "Intensity"; ff2.displayName = "Frequency"; ff1.interactable = true; ff2.interactable = true; colorField.interactable = false;}
        else if(property == colorPropertyEnum.RainbowFlickerFixed) {ff1.displayName = "Intensity Of Flicker"; ff2.displayName = "Frequency Of Fixed Flicker"; ff1.interactable = true; ff2.interactable = false; colorField.interactable = false;}
        else if(property == colorPropertyEnum.FlickerFixed) {ff1.displayName = "Intensity Of Flicker"; ff2.displayName = "Frequency Of Fixed Flicker"; ff1.interactable = true; ff2.interactable = false; colorField.interactable = true;}
        else if(property == colorPropertyEnum.InverseColors) {ff1.displayName = "N/A"; ff2.displayName = "N/A"; ff1.interactable = false; ff2.interactable = false; colorField.interactable = false;}
        else if(property == colorPropertyEnum.Brighten) {ff1.displayName = "Intensity"; ff2.displayName = "N/A"; ff1.interactable = true; ff2.interactable = false; colorField.interactable = false;}
        else if(property == colorPropertyEnum.CheckerboardInverseColor) {ff1.displayName = "N/A"; ff2.displayName = "Frequency of Swapping"; ff1.interactable = false; ff2.interactable = true; colorField.interactable = false;}
        else if(property == colorPropertyEnum.CheckerboardColor) {ff1.displayName = "Color Intensity"; ff2.displayName = "Frequency of Swapping"; ff3.displayName = "Frequency of Pulsating"; ff1.interactable = true; ff2.interactable = true; ff3.interactable = true; colorField.interactable = true;}
        else if(property == colorPropertyEnum.Colorfy) {ff1.displayName = "Color Intensity"; ff2.displayName = "N/A"; ff1.interactable = true; ff2.interactable = false; colorField.interactable = true;}
        else if(property == colorPropertyEnum.SnakeColor) {ff1.displayName = "Color Intensity"; ff2.displayName = "Speed"; ff3.displayName = "Length (rounded)"; ff1.interactable = true; ff2.interactable = true; ff3.interactable = true; colorField.interactable = true;}
        else if(property == colorPropertyEnum.DoubleSnakeColor) {ff1.displayName = "Color Intensity"; ff2.displayName = "Speed"; ff3.displayName = "Length (rounded)"; ff1.interactable = true; ff2.interactable = true; ff3.interactable = true; colorField.interactable = true;}
    }
    public static void createStyleRankColoringSchemePanel(ConfigPanel rootPanel, int index, int j)
    {
        ConfigPanel panel = new ConfigPanel(rootPanel, styleNames[j], "stylerankpanel" + j + " " + index);
        ColorField color1field = new ColorField(panel, "Main Color", "stylerankMainColor" + j + " " + index, styleColors[j]);
        color1field.onValueChange += (ColorField.ColorValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].mainColor = e.value;};
        Plugin.styleRankColorSettings[index, j].mainColor = color1field.value;

        new ConfigHeader(panel, "Alternate Color only used if Color Alternate Frequency > 0", 12);

        ColorField color2field = new ColorField(panel, "Alternate Color", "stylerankAltColor" + j + " " + index, UnityEngine.Color.cyan);
        color2field.onValueChange += (ColorField.ColorValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].secondaryColor = e.value;};
        Plugin.styleRankColorSettings[index, j].secondaryColor = color2field.value;

        FloatField colorAlternateFrequencyField = new FloatField(panel, "Color Alternate Frequency", "colorAlternateFrequency" + j + " " + index, 0f, 0f, 1000f);
        colorAlternateFrequencyField.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].alternateColorFreq = e.value;};
        Plugin.styleRankColorSettings[index, j].alternateColorFreq = colorAlternateFrequencyField.value;

        FloatField flickerFrequencyField = new FloatField(panel, "Flicker Frequency", "flickerFrequency" + j + " " + index, 0f, 0f, 1000f);
        flickerFrequencyField.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].flickerFrequency = e.value;};
        Plugin.styleRankColorSettings[index, j].flickerFrequency = flickerFrequencyField.value;

        ColorField flickerColorField = new ColorField(panel, "Flicker Color", "flickerColor" + j + " " + index, UnityEngine.Color.black);
        flickerColorField.onValueChange += (ColorField.ColorValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].flickerColor = e.value;};
        Plugin.styleRankColorSettings[index, j].flickerColor = flickerColorField.value;

        FloatField flickerIntensityField = new FloatField(panel, "Flicker Intensity", "flickerIntensity" + j + " " + index, 0f, 0f, 1f);
        flickerIntensityField.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].flickerAmplitude = e.value;};
        Plugin.styleRankColorSettings[index, j].flickerAmplitude = flickerIntensityField.value;

        FloatField pulsateFrequencyField = new FloatField(panel, "Pulsate Frequency", "pulsateFrequency" + j + " " + index, 0f, 0f, 1000f);
        pulsateFrequencyField.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].pulsateFrequency = e.value;};
        Plugin.styleRankColorSettings[index, j].pulsateFrequency = pulsateFrequencyField.value;

        FloatField pulsateIntensityField = new FloatField(panel, "Pulsate Intensity", "pulsateIntensity" + j + " " + index, 0f, 0f, 1f);
        pulsateIntensityField.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].pulsateAmplitude = e.value;};
        Plugin.styleRankColorSettings[index, j].pulsateAmplitude = pulsateIntensityField.value;

        new ConfigHeader(panel, "Additional Properties", 24);

        new ConfigHeader(panel, "Property 1", 15);

        EnumField<colorPropertyEnum> additionalFilter1Field = new EnumField<colorPropertyEnum>(panel, "Property 1", "property1" + j + " " + index, colorPropertyEnum.None);

        ColorField filter1ColorField = new ColorField(panel, "Property 1 Color", "property1color" + j + " " + index, UnityEngine.Color.black);
        filter1ColorField.onValueChange += (ColorField.ColorValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property1Color = e.value;};
        Plugin.styleRankColorSettings[index, j].property1Color = filter1ColorField.value;

        FloatField additionalFilter1Float1Field = new FloatField(panel, "Property 1 Value 1", "property1float1" + j + " " + index, 0f, 0f, 1000f);
        additionalFilter1Float1Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property1Float1 = e.value;};
        Plugin.styleRankColorSettings[index, j].property1Float1 = additionalFilter1Float1Field.value;

        FloatField additionalFilter1Float2Field = new FloatField(panel, "Property 1 Value 2", "property1float2" + j + " " + index, 0f, 0f, 1000f);
        additionalFilter1Float2Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property1Float2 = e.value;};
        Plugin.styleRankColorSettings[index, j].property1Float2 = additionalFilter1Float2Field.value;

        FloatField additionalFilter1Float3Field = new FloatField(panel, "Property 1 Value 3", "property1float3" + j + " " + index, 0f, 0f, 1000f);
        additionalFilter1Float3Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property1Float3 = e.value;};
        Plugin.styleRankColorSettings[index, j].property1Float3 = additionalFilter1Float3Field.value;

        additionalFilter1Field.onValueChange += (EnumField<colorPropertyEnum>.EnumValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property1 = e.value; changePropertyFieldNames(e.value, additionalFilter1Float1Field, additionalFilter1Float2Field, additionalFilter1Float3Field, filter1ColorField);};
        Plugin.styleRankColorSettings[index, j].property1 = additionalFilter1Field.value; changePropertyFieldNames(additionalFilter1Field.value, additionalFilter1Float1Field, additionalFilter1Float2Field, additionalFilter1Float3Field, filter1ColorField);


        new ConfigHeader(panel, "Property 2", 15);


        EnumField<colorPropertyEnum> additionalFilter2Field = new EnumField<colorPropertyEnum>(panel, "Property 2", "property2" + j + " " + index, colorPropertyEnum.None);

        ColorField filter2ColorField = new ColorField(panel, "Property 2 Color", "property2color" + j + " " + index, UnityEngine.Color.black);
        filter2ColorField.onValueChange += (ColorField.ColorValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property2Color = e.value;};
        Plugin.styleRankColorSettings[index, j].property2Color = filter2ColorField.value;

        FloatField additionalFilter2Float1Field = new FloatField(panel, "Property 2 Value 1", "property2float1" + j + " " + index, 0f, 0f, 1000f);
        additionalFilter2Float1Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property2Float1 = e.value;};
        Plugin.styleRankColorSettings[index, j].property2Float1 = additionalFilter2Float1Field.value;

        FloatField additionalFilter2Float2Field = new FloatField(panel, "Property 2 Value 2", "property2float2" + j + " " + index, 0f, 0f, 1000f);
        additionalFilter2Float2Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property2Float2 = e.value;};
        Plugin.styleRankColorSettings[index, j].property2Float2 = additionalFilter2Float2Field.value;

        FloatField additionalFilter2Float3Field = new FloatField(panel, "Property 2 Value 3", "property2float3" + j + " " + index, 0f, 0f, 1000f);
        additionalFilter2Float3Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property2Float3 = e.value;};
        Plugin.styleRankColorSettings[index, j].property2Float3 = additionalFilter2Float3Field.value;

        additionalFilter2Field.onValueChange += (EnumField<colorPropertyEnum>.EnumValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property2 = e.value; changePropertyFieldNames(e.value, additionalFilter2Float1Field, additionalFilter2Float2Field, additionalFilter2Float3Field, filter2ColorField);};
        Plugin.styleRankColorSettings[index, j].property2 = additionalFilter2Field.value; changePropertyFieldNames(additionalFilter2Field.value, additionalFilter2Float1Field, additionalFilter2Float2Field, additionalFilter2Float3Field, filter2ColorField);

        
        new ConfigHeader(panel, "Property 3", 15);


        EnumField<colorPropertyEnum> additionalFilter3Field = new EnumField<colorPropertyEnum>(panel, "Property 3", "property3" + j + " " + index, colorPropertyEnum.None);

        ColorField filter3ColorField = new ColorField(panel, "Property 3 Color", "property3color" + j + " " + index, UnityEngine.Color.black);
        filter3ColorField.onValueChange += (ColorField.ColorValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property3Color = e.value;};
        Plugin.styleRankColorSettings[index, j].property3Color = filter3ColorField.value;

        FloatField additionalFilter3Float1Field = new FloatField(panel, "Property 3 Value 1", "property3float1" + j + " " + index, 0f, 0f, 1000f);
        additionalFilter3Float1Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property3Float1 = e.value;};
        Plugin.styleRankColorSettings[index, j].property3Float1 = additionalFilter3Float1Field.value;

        FloatField additionalFilter3Float2Field = new FloatField(panel, "Property 3 Value 2", "property3float2" + j + " " + index, 0f, 0f, 1000f);
        additionalFilter3Float2Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property3Float2 = e.value;};
        Plugin.styleRankColorSettings[index, j].property3Float2 = additionalFilter3Float2Field.value;

        FloatField additionalFilter3Float3Field = new FloatField(panel, "Property 3 Value 3", "property3float3" + j + " " + index, 0f, 0f, 1000f);
        additionalFilter3Float3Field.onValueChange += (FloatField.FloatValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property3Float3 = e.value;};
        Plugin.styleRankColorSettings[index, j].property3Float3 = additionalFilter3Float3Field.value;

        additionalFilter3Field.onValueChange += (EnumField<colorPropertyEnum>.EnumValueChangeEvent e) => {Plugin.styleRankColorSettings[index, j].property3 = e.value; changePropertyFieldNames(e.value, additionalFilter3Float1Field, additionalFilter3Float2Field, additionalFilter3Float3Field, filter3ColorField);};
        Plugin.styleRankColorSettings[index, j].property3 = additionalFilter3Field.value; changePropertyFieldNames(additionalFilter3Field.value, additionalFilter3Float1Field, additionalFilter3Float2Field, additionalFilter3Float3Field, filter3ColorField);
    }

    public static Dictionary<int, ConfigPanel> deviceConfigPanels = new Dictionary<int, ConfigPanel>();
    public static Dictionary<int, ConfigHeader> deviceConfigHeaders = new Dictionary<int, ConfigHeader>();
    public static ConfigDivision enableRestDevicesDivision;
    public static void fillConnectedDevicesPanel(ConfigPanel rootPanel)
    {
        EnumField<styleRankColoringSchemeEnum> deviceColorField = new EnumField<styleRankColoringSchemeEnum>(rootPanel, "Default Color Scheme", "defaultColorSchemeField", styleRankColoringSchemeEnum.Scheme1);
        deviceColorField.onValueChange += (EnumField<styleRankColoringSchemeEnum>.EnumValueChangeEvent e) => {defaultColoring = e.value;};
        defaultColoring = deviceColorField.value;

        BoolField allDevicesConnectedField = new BoolField(rootPanel, "Connect All Devices", "connectAllDevicesField", true);
        enableRestDevicesDivision = new ConfigDivision(rootPanel, "enableRestDevicesDivision");
        allDevicesConnectedField.onValueChange += (BoolField.BoolValueChangeEvent e) => {allDevicesConnected = e.value;}; //enableRestDevicesDivision.interactable = !e.value;};
        allDevicesConnected = allDevicesConnectedField.value; //enableRestDevicesDivision.interactable = !allDevicesConnectedField.value;
        foreach(Device d in Plugin.connectedDevices)
        {
            int i = d.Index;
            ConfigHeader nameHeader = new ConfigHeader(enableRestDevicesDivision, d.Name, 18);
            ConfigPanel panel = new ConfigPanel(enableRestDevicesDivision, "Device " + i, "devicepanel" + i);
            createConnectedDevicePanel(panel, i);
            deviceConfigPanels[i] = panel;
            deviceConfigHeaders[i] = nameHeader;
        }
    }
    public static bool allDevicesConnected = false;
    public static styleRankColoringSchemeEnum defaultColoring = styleRankColoringSchemeEnum.None;
    public static void checkForDeviceColorSettingsOfIExists(int i)
    {
        if(!Plugin.deviceColorSettings.ContainsKey(i))
        {
            Plugin.deviceColorSettings[i] = new deviceColoringScheme();
        }
    }
    public static void createConnectedDevicePanel(ConfigPanel rootPanel, int index)
    {

        checkForDeviceColorSettingsOfIExists(index);
        BoolField enabledField = new BoolField(rootPanel, "Enabled for this device", "deviceEnabledField", false);
        enabledField.onValueChange += (BoolField.BoolValueChangeEvent e) => {checkForDeviceColorSettingsOfIExists(index); Plugin.deviceColorSettings[index].enabled = e.value;};
        Plugin.deviceColorSettings[index].enabled = enabledField.value;

        EnumField<styleRankColoringSchemeEnum> deviceColorField = new EnumField<styleRankColoringSchemeEnum>(rootPanel, "Device Color Scheme", "deviceColorSchemeField", styleRankColoringSchemeEnum.None);
        deviceColorField.onValueChange += (EnumField<styleRankColoringSchemeEnum>.EnumValueChangeEvent e) => {checkForDeviceColorSettingsOfIExists(index); Plugin.deviceColorSettings[index].scheme = e.value;};
        Plugin.deviceColorSettings[index].scheme = deviceColorField.value;
    }

    public static void refillConnectedDevicePanels()
    {
        //ConfigPanel rootPanel = connectedDevicesPanel;
        foreach(int i in deviceConfigHeaders.Keys)
        {
            ConfigHeader ch = deviceConfigHeaders[i];
            ch.text = "Device not connected";
            for(int j = 0; j < Plugin.connectedDevices.Count; j++)
            {
                if(i == Plugin.connectedDevices[j].Index)
                {
                    ch.text = Plugin.connectedDevices[j].Name;
                }
            }
        }
        foreach(ConfigPanel cp in deviceConfigPanels.Values)
        {
            cp.interactable = false;
        }
        foreach(Device d in Plugin.connectedDevices)
        {
            if(deviceConfigPanels.ContainsKey(d.Index))
            {
                deviceConfigPanels[d.Index].interactable = true;
            }
            else
            {
                ConfigHeader nameHeader = new ConfigHeader(enableRestDevicesDivision, d.Name, 18);
                ConfigPanel panel = new ConfigPanel(enableRestDevicesDivision, "Device " + d.Index, "devicepanel" + d.Index);
                createConnectedDevicePanel(panel, d.Index);
                deviceConfigPanels[d.Index] = panel;
                deviceConfigHeaders[d.Index] = nameHeader;
            }
        }
    }


}