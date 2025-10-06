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

using OpenRGB.NET;
using BepInEx.Logging;

namespace UltraRGBLighting;

//client.UpdateLeds lags a bit
//flicker code also lags a bit

[BepInPlugin("UltraRGBLighting", "UltraRGBLighting", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    string DefaultParentFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";
    public static ManualLogSource logger = null;
    public static System.Random random = new System.Random();
    public static bool modEnabled = false;
    public static bool connected = false;
    public static int currentRankInt = 0;
    public static float currentRankFloat = 0f;
    
    public static styleRankColoringScheme[,] styleRankColorSettings = new styleRankColoringScheme[4, 9];
    public static Dictionary<int, deviceColoringScheme> deviceColorSettings = new Dictionary<int, deviceColoringScheme>();
    public static Dictionary<int, List<OpenRGB.NET.Color>> colorsBoard = new Dictionary<int, List<OpenRGB.NET.Color>>();
    private void Awake()
    {       
        Harmony harmony = new Harmony("UltraRGBLighting");
        harmony.PatchAll();

        for(int i = 0; i < Permutators.permutatorArr.Length; i++)
        {
            Permutators.permutatorArr[i] = new Permutator();
        }
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 9; j++)
            {
                styleRankColorSettings[i, j] = new styleRankColoringScheme();
            }
        }

        ModConfig.createConfig();
        logger = new ManualLogSource("UltraRGBLighting"); BepInEx.Logging.Logger.Sources.Add(logger);

        logger.LogInfo("UltraRGBLighting started!");
    }
    public static UnityEngine.Color AlternateColors(styleRankColoringScheme data)
    {
        UnityEngine.Color c = data.mainColor;
        if(data.alternateColorFreq > 0f)
        {
            UnityEngine.Color c1 = data.mainColor;
            UnityEngine.Color c2 = data.secondaryColor;
            float angle = Time.realtimeSinceStartup * data.alternateColorFreq * 2f * Convert.ToSingle(Math.PI);
            float percentage = Convert.ToSingle(Math.Sin(angle)) / 2.0f + 0.5f;
            c.r = c1.r * (1f - percentage) + c2.r * percentage;
            c.g = c1.g * (1f - percentage) + c2.g * percentage;
            c.b = c1.b * (1f - percentage) + c2.b * percentage;
        }
        return c;
    }
    public static UnityEngine.Color PulsateColors(UnityEngine.Color color, styleRankColoringScheme data)
    {
        if(data.pulsateFrequency > 0f)
        {
            float angle = Time.realtimeSinceStartup * data.pulsateFrequency * 2f * Convert.ToSingle(Math.PI);
            float percentageDark = data.pulsateAmplitude * Convert.ToSingle(Math.Sin(angle)) / 2.0f + 0.5f;
            color.r = color.r * (1f - percentageDark);
            color.g = color.g * (1f - percentageDark);
            color.b = color.b * (1f - percentageDark);
        }
        return color;
    }
    public static UnityEngine.Color FlickerColors(UnityEngine.Color color, styleRankColoringScheme data, int settingsIndex, int LEDindex)
    {
        //if(data.property1 == colorPropertyEnum.RainbowFlickerFixed || data.property2 == colorPropertyEnum.RainbowFlickerFixed || data.property3 == colorPropertyEnum.RainbowFlickerFixed) {return colorIn;} //dont flickerin if rainbowflicker
        if(data.flickerFrequency > 0f)
        {
            float percentageFlicker = flickerIntensity[settingsIndex][LEDindex] * data.flickerAmplitude;
            color.r = color.r * (1f - percentageFlicker) + data.flickerColor.r * percentageFlicker;
            color.g = color.g * (1f - percentageFlicker) + data.flickerColor.g * percentageFlicker;
            color.b = color.b * (1f - percentageFlicker) + data.flickerColor.b * percentageFlicker;
        }
        return color;
    }

    public static UnityEngine.Color HSVToColor(float h, float S, float V) //copied from internet somewhere
    {    
        float H = h;
        while (H < 0) { H += 360; };
        while (H >= 360) { H -= 360; };
        float R, G, B;
        if (V <= 0)
            { R = G = B = 0; }
        else if (S <= 0)
        {
            R = G = B = V;
        }
        else
        {
            float hf = H / 60.0f;
            int i = (int)Math.Floor(hf);
            float f = hf - i;
            float pv = V * (1 - S);
            float qv = V * (1 - S * f);
            float tv = V * (1 - S * (1 - f));
            switch (i)
            {

            // Red is the dominant color

            case 0:
                R = V;
                G = tv;
                B = pv;
                break;

            // Green is the dominant color

            case 1:
                R = qv;
                G = V;
                B = pv;
                break;
            case 2:
                R = pv;
                G = V;
                B = tv;
                break;

            // Blue is the dominant color

            case 3:
                R = pv;
                G = qv;
                B = V;
                break;
            case 4:
                R = tv;
                G = pv;
                B = V;
                break;

            // Red is the dominant color

            case 5:
                R = V;
                G = pv;
                B = qv;
                break;

            // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

            case 6:
                R = V;
                G = tv;
                B = pv;
                break;
            case -1:
                R = V;
                G = pv;
                B = qv;
                break;

            // The color is not defined, we should throw an error.

            default:
                //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                R = G = B = V; // Just pretend its black/white
                break;
            }
        }
        UnityEngine.Color color = new UnityEngine.Color(R,G,B);
        return color;
    }

    public static UnityEngine.Color InverseColor(UnityEngine.Color colorIn)
    {
        //inverse like this
        UnityEngine.Color c = new UnityEngine.Color();
        c.r = 1f - colorIn.r;
        c.g = 1f - colorIn.g;
        c.b = 1f - colorIn.b;
        return c;
    }

    public static UnityEngine.Color ApplyColorPropertyStep2(UnityEngine.Color colorIn, colorPropertyEnum property, UnityEngine.Color propertyColor, float f1, float f2, float f3, int LEDindex)
    {
        UnityEngine.Color c = colorIn;
        if(property == colorPropertyEnum.None)
        {
            return colorIn;
        }
        else
        {
            if(property == colorPropertyEnum.RainbowWhole)
            {
                if(f1 > 1f) {f1 = 1f;}
                if(f1 < 0) {f1 = 0f;}
                UnityEngine.Color rainbowColor = HSVToColor(Convert.ToSingle(Time.realtimeSinceStartup * 360.0 * f2), 1f, 1f);
                c.r = rainbowColor.r * f1 + colorIn.r * (1f - f1);
                c.g = rainbowColor.g * f1 + colorIn.g * (1f - f1);
                c.b = rainbowColor.b * f1 + colorIn.b * (1f - f1);
            }
            else if(property == colorPropertyEnum.RainbowFlickerFixed)
            {
                if(flickerIntensityFixed.Count != 0)
                {
                    UnityEngine.Color rainbowColor = HSVToColor(flickerIntensityFixed[LEDindex] * 360f, 1f, 1f);
                    c.r = rainbowColor.r * f1 + colorIn.r * (1f - f1);
                    c.g = rainbowColor.g * f1 + colorIn.g * (1f - f1);
                    c.b = rainbowColor.b * f1 + colorIn.b * (1f - f1);
                }
            }
            else if(property == colorPropertyEnum.FlickerFixed)
            {
                if(flickerIntensityFixed.Count != 0)
                {
                    float percentageFlicker = flickerIntensityFixed[LEDindex] * f1;
                    c.r = colorIn.r * (1f - percentageFlicker) + propertyColor.r * percentageFlicker;
                    c.g = colorIn.g * (1f - percentageFlicker) + propertyColor.g * percentageFlicker;
                    c.b = colorIn.b * (1f - percentageFlicker) + propertyColor.b * percentageFlicker;
                }
            }
            else if(property == colorPropertyEnum.InverseColors)
            {
                c = InverseColor(colorIn);
            }
            else if(property == colorPropertyEnum.Brighten)
            {
                c = Permutators.Brighten(colorIn, f1);
            }
            else if(property == colorPropertyEnum.CheckerboardInverseColor)
            {
                //f1
                //f2 - frequency of swapping
                //f3
                int num = 1;
                if(f2 > 0) {num = (int)Math.Round(Time.realtimeSinceStartup * f2) % 2;}
                if(LEDindex % 2 == num) 
                {
                    c = InverseColor(colorIn);
                }
            }
            else if(property == colorPropertyEnum.CheckerboardColor)
            {
                //f1 - amplitude
                //f2 - frequency of swapping checker spots
                //f3 - pulsate freq
                int num = 1;
                if(f1 > 1f) {f1 = 1f;}
                if(f2 > 0) {num = (int)Math.Round(Time.realtimeSinceStartup * f2) % 2;}
                if(LEDindex % 2 == num) 
                {
                    if(f3 > 0f) {f1 = f1 * (0.5f + 0.5f * Convert.ToSingle(Math.Sin(Time.realtimeSinceStartup * 2 * Math.PI * f3)));}
                    c.r = colorIn.r * (1f - f1) + propertyColor.r * f1;
                    c.g = colorIn.g * (1f - f1) + propertyColor.g * f1;
                    c.b = colorIn.b * (1f - f1) + propertyColor.b * f1;
                }
            }
            else if(property == colorPropertyEnum.Colorfy)
            {
                if(f1 > 1f) {f1 = 1f;}
                c = Permutators.Colorfy(colorIn, propertyColor, f1);
            }
            else if(property == colorPropertyEnum.SnakeColor)
            {
                //snake isnt always visible if not on device with maxLEDcount, but wtv. We dont actually know what device is being used here, nor can we because this could be used by two devices at once.
                //f1 - amplitude
                //f2 - speed of snake in led / s
                //f3 - length (rounded)
                int length = (int)Math.Round(f3);
                for(int i = 0; i < length; i++)
                {
                    if((int)(Math.Round(Time.realtimeSinceStartup * f2)) % maxLEDCount == LEDindex - i)
                    {
                        c.r = colorIn.r * (1f - f1) + propertyColor.r * f1;
                        c.g = colorIn.g * (1f - f1) + propertyColor.g * f1;
                        c.b = colorIn.b * (1f - f1) + propertyColor.b * f1;
                    }
                }
            }
            else if(property == colorPropertyEnum.DoubleSnakeColor)
            {
                //f1 - amplitude
                //f2 - speed of snake in led / s
                //f3 - length (rounded)
                int length = (int)Math.Round(f3);
                for(int i = 0; i < length; i++)
                {
                    if((int)(Math.Round(Time.realtimeSinceStartup * f2)) % maxLEDCount == LEDindex - i)
                    {
                        c.r = colorIn.r * (1f - f1) + propertyColor.r * f1;
                        c.g = colorIn.g * (1f - f1) + propertyColor.g * f1;
                        c.b = colorIn.b * (1f - f1) + propertyColor.b * f1;
                    }

                    //should just work?
                    if((((int)(-Math.Round(Time.realtimeSinceStartup * f2)) % maxLEDCount) + maxLEDCount) % maxLEDCount == LEDindex - i)
                    {
                        c.r = colorIn.r * (1f - f1) + propertyColor.r * f1;
                        c.g = colorIn.g * (1f - f1) + propertyColor.g * f1;
                        c.b = colorIn.b * (1f - f1) + propertyColor.b * f1;
                    }
                }
            }
            return c;
        }
    }

    public static UnityEngine.Color ApplyColorPropertyStep1(UnityEngine.Color colorIn, styleRankColoringScheme data, int settingsIndex, int LEDindex, int propertyNum)
    {
        colorPropertyEnum property = colorPropertyEnum.None;
        float f1 = 0f; //usually amp
        float f2 = 0f; //usually freq
        float f3 = 0f; //something else
        UnityEngine.Color propertyColor = UnityEngine.Color.black;
        if(propertyNum == 1) {property = data.property1; f1 = data.property1Float1; f2 = data.property1Float2; f3 = data.property1Float3; propertyColor = data.property1Color;}
        else if(propertyNum == 2) {property = data.property2; f1 = data.property2Float1; f2 = data.property2Float2; f3 = data.property2Float3; propertyColor = data.property2Color;}
        else if(propertyNum == 3) {property = data.property3; f1 = data.property3Float1; f2 = data.property3Float2; f3 = data.property3Float3; propertyColor = data.property3Color;}

        return ApplyColorPropertyStep2(colorIn, property, propertyColor, f1, f2, f3, LEDindex);
    }
    public static UnityEngine.Color getColorByRank(int i, int settingsIndex, int LEDindex)
    {
        UnityEngine.Color c = UnityEngine.Color.white;
        styleRankColoringScheme data = styleRankColorSettings[settingsIndex, i + 1];

        c = AlternateColors(data);
        c = FlickerColors(c, data, settingsIndex, LEDindex);
        c = PulsateColors(c, data);
        c = ApplyColorPropertyStep1(c, data, settingsIndex, LEDindex, 1);
        c = ApplyColorPropertyStep1(c, data, settingsIndex, LEDindex, 2);
        c = ApplyColorPropertyStep1(c, data, settingsIndex, LEDindex, 3);
        return c;
    }
    public static UnityEngine.Color getCurrentColor(int settingsIndex, int LEDindex)
    {
        UnityEngine.Color c = UnityEngine.Color.white;
        if(ModConfig.gradientizeRankColors)
        {
            int bottom = (int)Math.Floor(currentRankFloat);
            int top = (int)Math.Ceiling(currentRankFloat);
            float percentage = currentRankFloat - currentRankInt;

            UnityEngine.Color c1 = getColorByRank(bottom, settingsIndex, LEDindex);
            UnityEngine.Color c2 = getColorByRank(top, settingsIndex, LEDindex);

            c.r = c1.r * (1f - percentage) + c2.r * percentage;
            c.g = c1.g * (1f - percentage) + c2.g * percentage;
            c.b = c1.b * (1f - percentage) + c2.b * percentage;
        }
        else
        {
            c = getColorByRank(currentRankInt, settingsIndex, LEDindex);
        }

        c = Permutators.ApplyPermutators(c, LEDindex);

        return c;
    }
    public static List<Device> connectedDevices = new List<Device>();
    public static OpenRGB.NET.OpenRgbClient client;
    public static int getSettingsByDeviceIndex(int deviceIndex)
    {
        if(deviceColorSettings.ContainsKey(deviceIndex))
        {
            if(deviceColorSettings[deviceIndex].scheme == styleRankColoringSchemeEnum.Scheme1 && deviceColorSettings[deviceIndex].enabled){return 0;}
            else if(deviceColorSettings[deviceIndex].scheme == styleRankColoringSchemeEnum.Scheme2 && deviceColorSettings[deviceIndex].enabled){return 1;}
            else if(deviceColorSettings[deviceIndex].scheme == styleRankColoringSchemeEnum.Scheme3 && deviceColorSettings[deviceIndex].enabled){return 2;}
            else if(deviceColorSettings[deviceIndex].scheme == styleRankColoringSchemeEnum.Scheme4 && deviceColorSettings[deviceIndex].enabled){return 3;}
            else if(ModConfig.allDevicesConnected == true) {return ((int)ModConfig.defaultColoring) - 1;}
            else if(deviceColorSettings[deviceIndex].scheme == styleRankColoringSchemeEnum.None || deviceColorSettings[deviceIndex].enabled == false){return -1;}
        }
        return 0;
    }
    public static float[] lastFlickerTime = {0f, 0f, 0f, 0f};
    public static List<float>[] flickerIntensity = new List<float>[4];
    public static float flickerIntensityFixedFrequency = 10f; //default
    public static float lastFlickerTimeFixed = 0f;
    public static List<float> flickerIntensityFixed = new List<float>(); //used when settingsIndex is not known
    public static int maxLEDCount = 0;
    //async maybe better, doesnt matter for high enough fps though
    public static bool[] connectedSchemes = {false, false, false, false};
    public static void flickerIntensityLogic()
    {
        //i is settings index
        float time = Time.realtimeSinceStartup;
        for(int i = 0; i < 4; i++)
        {
            if(connectedSchemes[i] == false) {continue;} //this is kind of expensive, dont calculate if you dont need to !!!
            float freq = styleRankColorSettings[i, currentRankInt + 1].flickerFrequency;
            float period = 1 / freq;
            if(time > lastFlickerTime[i] + period)
            {
                for(int j = 0; j < maxLEDCount; j++)
                {
                    flickerIntensity[i][j] = Convert.ToSingle(random.NextDouble());
                    lastFlickerTime[i] = Time.realtimeSinceStartup;
                }
            }
        }

        float freq2 = flickerIntensityFixedFrequency;
        float period2 = 1 / freq2;
        if(time > lastFlickerTimeFixed + period2)
        {
            for(int j = 0; j < maxLEDCount; j++)
            {
                flickerIntensityFixed[j] = Convert.ToSingle(random.NextDouble());
                lastFlickerTimeFixed = Time.realtimeSinceStartup;
            }
        }
    }
    public static float nextTimeAttemptAutoconnect = 0f;
    public static float refreshRate = 60f;
    public static float lastTimeRefreshed = 0f;
    public static int framesSinceLastRefreshed = 0;
    void Update()
    {
        if(modEnabled == false) {return;}
        if(MonoSingleton<StyleHUD>.Instance == null) {return;}

        //decrease number execs to reduce lag
        framesSinceLastRefreshed += 1;
        if(lastTimeRefreshed + 1f / refreshRate < Time.realtimeSinceStartup)
        {
            lastTimeRefreshed = Time.realtimeSinceStartup;

            connectedSchemes[0] = false; connectedSchemes[1] = false; connectedSchemes[2] = false; connectedSchemes[3] = false;
            for(int i = 0; i < connectedDevices.Count; i++) //not a great way to do this
            {
                connectedSchemes[getSettingsByDeviceIndex(i)] = true;
            }

            Permutators.actionLogic();
            currentRankInt = MonoSingleton<StyleHUD>.Instance.rankIndex;
            currentRankFloat = currentRankInt + MonoSingleton<StyleHUD>.Instance.currentMeter / MonoSingleton<StyleHUD>.Instance.currentRank.maxMeter;
            if(currentRankFloat < 0) {currentRankFloat = -1; currentRankInt = -1;} //yes, it does go below zero. This is when no rank is shown at all.
            if(connected)
            {
                flickerIntensityLogic();
                foreach(Device device in connectedDevices)
                {
                    int deviceIndex = device.Index;
                    for(int i = 0; i < colorsBoard[deviceIndex].Count; i++)
                    {
                        int settingsIndex = getSettingsByDeviceIndex(deviceIndex);
                        if(settingsIndex == -1) {goto noColor;}
                        UnityEngine.Color color = getCurrentColor(settingsIndex, i);
                        Byte r = (Byte)((int)(color.r * 255));
                        Byte g = (Byte)((int)(color.g * 255));
                        Byte b = (Byte)((int)(color.b * 255));
                        colorsBoard[deviceIndex][i] = new OpenRGB.NET.Color(r, g, b);
                    }
                    OpenRGB.NET.Color[] cArr = new OpenRGB.NET.Color[colorsBoard[deviceIndex].Count];
                    for(int i = 0; i < cArr.Length; i++)
                    {
                        cArr[i] = colorsBoard[deviceIndex][i];
                    }

                    client.UpdateLeds(deviceIndex, cArr);
                }
            }
            else
            {
                if(ModConfig.autoConnect)
                {
                    if(Time.realtimeSinceStartup > nextTimeAttemptAutoconnect)
                    {
                        ModConfig.connectToOpenRGB();
                        nextTimeAttemptAutoconnect = Time.realtimeSinceStartup + 15f;
                    }
                }
            }
            noColor:;

            framesSinceLastRefreshed = 0;
        }
    }
}

