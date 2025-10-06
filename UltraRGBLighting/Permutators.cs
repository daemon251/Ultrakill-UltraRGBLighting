using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace UltraRGBLighting;

public enum PermutatorCause {None, RecentKills, RecentDamage, RecentStyle, OnStyleGain, OnRankGain, OnRankLose, OnKill, OnDamage, OnTakeDamage, OnPlayerDeath, Always}
//public enum PermutatorEffect {None, Colorfy, ColorfyCheckerboard, CheckerboardInverseColor, InverseColors, Brighten}
public enum FadeEnum {Linear, NoFade, SquareRoot, Quadratic, PassIfAboveHalf, PassIfBelowHalf}
public class Permutator
{
    public PermutatorCause cause = PermutatorCause.None;
    public bool scaleWithCauseMagnitude = false;
    //public PermutatorEffect effect = PermutatorEffect.None;
    public FadeEnum fadeType = FadeEnum.Linear;
    public UnityEngine.Color propertyColor = UnityEngine.Color.white;
    public colorPropertyEnum property = colorPropertyEnum.None;
    public float propertyFloat1 = 0f;
    public float propertyFloat2 = 0f;
    public float propertyFloat3 = 0f;
    public float length = 0f;
}
public class Permutators
{
    public static float lastTimeStyleGain = -1f; //
    public static float lastTimeRankGain = -1f; //
    public static float lastTimeRankLose = -1f; //
    public static float lastTimeKill = -1f; //
    public static float lastTimeDamage = -1f; //
    public static float lastTimeOnTakeDamage = -1f; //
    public static float lastTimePlayerDeath = -1f; //

    public static List<float> damageDealtThisTick = new List<float>();
    public static float lastDamage = -1f; //
    public static float lastOnTakeDamage = -1f; //
    public static bool lastKillBig = false; //
    public static List<float> styleGainedThisTick = new List<float>();
    public static float lastStyleGain = -1f; //

    public static float recentKills = 0f; //
    public static float recentDamage = 0f; //
    public static float recentStyle = 0f; //


    static int hp = 0;
    static bool dead = false;
    public static void actionLogic()
    {
        if(hp > MonoSingleton<NewMovement>.instance.hp)
        {
            lastOnTakeDamage = hp - MonoSingleton<NewMovement>.instance.hp;
            lastTimeOnTakeDamage = Time.realtimeSinceStartup;
        }
        hp = MonoSingleton<NewMovement>.instance.hp;

        if(dead != MonoSingleton<NewMovement>.instance.dead && MonoSingleton<NewMovement>.instance.dead == true)
        {
            lastTimePlayerDeath = Time.realtimeSinceStartup;
        }
        dead = MonoSingleton<NewMovement>.instance.dead;
        //these can go massive then be overriden by small damage... bad?
        if(styleGainedThisTick.Count != 0)
        {
            float sum = 0f;
            for(int i = 0; i < styleGainedThisTick.Count; i++)
            {
                sum += styleGainedThisTick[i];
            }
            styleGainedThisTick = new List<float>();
        }

        if(damageDealtThisTick.Count != 0)
        {
            float sum = 0f;
            for(int i = 0; i < damageDealtThisTick.Count; i++)
            {
                sum += damageDealtThisTick[i];
            }
            lastDamage = sum;
            damageDealtThisTick = new List<float>();
        }

        //this seems sensitive to dips in fps
        recentDamage = recentDamage * (1 - 1 * (Time.deltaTime * Plugin.framesSinceLastRefreshed)) - 0.1f * (Time.deltaTime * Plugin.framesSinceLastRefreshed); //slight differences depending on fps but wtv
        recentStyle = recentStyle * (1 - 1 * (Time.deltaTime * Plugin.framesSinceLastRefreshed)) - 5f * (Time.deltaTime * Plugin.framesSinceLastRefreshed); //slight differences depending on fps but wtv
        recentKills = recentKills * (1 - 1 * (Time.deltaTime * Plugin.framesSinceLastRefreshed)) - 0.04f * (Time.deltaTime * Plugin.framesSinceLastRefreshed); //slight differences depending on fps but wtv

        if(recentDamage < 0) {recentDamage = 0;}
        if(recentStyle < 0) {recentStyle = 0;}
        if(recentKills < 0) {recentKills = 0;}
    }
    public static Permutator[] permutatorArr = new Permutator[15];
    public static UnityEngine.Color ApplyPermutators(UnityEngine.Color colorIn, int LEDindex)
    {
        UnityEngine.Color c = colorIn;
        for(int i = 0; i < permutatorArr.Length; i++)
        {
            Permutator p = permutatorArr[i];
            PermutatorCause pc = p.cause;
            FadeEnum fe = p.fadeType;
            bool scale = p.scaleWithCauseMagnitude;
            float length = p.length;
            float timeDif = 0f;
            float lengthMult = 1f;
            if     (pc == PermutatorCause.None) {continue;}
            else if(pc == PermutatorCause.OnStyleGain) 
            {
                if(lastTimeStyleGain < 0) {continue;}
                timeDif = Time.realtimeSinceStartup - lastTimeStyleGain;
                if(scale) {lengthMult = Convert.ToSingle(Math.Sqrt(lastStyleGain / 5f));} //has to be sqrted to not be obnoxious (this is slightly different from sqrt fadetype)
            }
            else if(pc == PermutatorCause.OnRankGain) 
            {
                if(lastTimeRankGain < 0) {continue;}
                timeDif = Time.realtimeSinceStartup - lastTimeRankGain;
            }
            else if(pc == PermutatorCause.OnRankLose) 
            {
                if(lastTimeRankLose < 0) {continue;}
                timeDif = Time.realtimeSinceStartup - lastTimeRankLose;
            }
            else if(pc == PermutatorCause.OnKill) 
            {
                if(lastTimeKill < 0) {continue;}
                timeDif = Time.realtimeSinceStartup - lastTimeKill;
                if(scale) {if(lastKillBig) {lengthMult = 2f;}}
            }
            else if(pc == PermutatorCause.OnDamage) 
            {
                if(lastTimeDamage < 0) {continue;}
                timeDif = Time.realtimeSinceStartup - lastTimeDamage;
                if(scale) {lengthMult = Convert.ToSingle(Math.Sqrt(lastDamage));} //has to be sqrted to not be obnoxious (this is slightly different from sqrt fadetype)
            }
            else if(pc == PermutatorCause.OnTakeDamage) 
            {
                if(lastTimeOnTakeDamage < 0) {continue;}
                timeDif = Time.realtimeSinceStartup - lastTimeOnTakeDamage;
                if(scale) {lengthMult = lastOnTakeDamage / 25f;}
            }
            else if(pc == PermutatorCause.OnPlayerDeath) 
            {
                if(lastTimePlayerDeath < 0) {continue;}
                timeDif = Time.realtimeSinceStartup - lastTimePlayerDeath;
            }
            else if(pc == PermutatorCause.Always)
            {
                timeDif = 0f;
            }
            else if(pc == PermutatorCause.RecentKills)
            {
                float fraction = recentKills / (4 / length);
                if(fraction > 1) {fraction = 1;}
                timeDif = (length * lengthMult) * (1 -fraction);
            }
            else if(pc == PermutatorCause.RecentDamage)
            {   
                float fraction = recentStyle / (30 / length);
                if(fraction > 1) {fraction = 1;}
                timeDif = (length * lengthMult) * (1 -fraction);
            }
            else if(pc == PermutatorCause.RecentStyle)
            {
                float fraction = recentStyle / (1000 / length);
                if(fraction > 1) {fraction = 1;}
                timeDif = (length * lengthMult) * (1 -fraction);
            }
            timeDif = Math.Abs(timeDif);
            if(timeDif >= length * lengthMult) {continue;}
            float amt = 1f - timeDif / (length * lengthMult);

            amt = FadeLogic(amt, fe);

            c = Plugin.ApplyColorPropertyStep2(c, p.property, p.propertyColor, p.propertyFloat1, p.propertyFloat2, p.propertyFloat3, LEDindex);

            //apply amt
            c.r = c.r * amt + colorIn.r * (1f - amt);
            c.g = c.g * amt + colorIn.g * (1f - amt);
            c.b = c.b * amt + colorIn.b * (1f - amt);
        }
        return c;
    }

    public static float FadeLogic(float f, FadeEnum fe)
    {
        if(fe == FadeEnum.Linear) 
        {
            //f = f
        }
        else if (fe == FadeEnum.NoFade) 
        {
            if(f > 0 && f <= 1) {f = 1;}
        }
        else if (fe == FadeEnum.Quadratic) 
        {
            f = f * f;
        }
        else if (fe == FadeEnum.SquareRoot) 
        {
            f = Convert.ToSingle(Math.Sqrt(f));
        }
        else if(fe == FadeEnum.PassIfAboveHalf)
        {
            if(f > 0.5) {f = 1;}
            else {f = 0;}
        }
        else if(fe == FadeEnum.PassIfBelowHalf)
        {
            if(f < 0.5) {f = 1;}
            else {f = 0;}
        }
        return f;
    }

    public static UnityEngine.Color Colorfy(UnityEngine.Color colorIn, UnityEngine.Color taintColor, float amt)
    {
        float r = taintColor.r * amt + colorIn.r * (1 - amt);
        float g = taintColor.g * amt + colorIn.g * (1 - amt);
        float b = taintColor.b * amt + colorIn.b * (1 - amt);
        UnityEngine.Color cOut = new UnityEngine.Color(r, g, b);
        return cOut;
    }
    public static UnityEngine.Color Brighten(UnityEngine.Color colorIn, float amt)
    {
        //brightens everything so that one rgb value reaches 1
        float r = colorIn.r;
        float g = colorIn.g;
        float b = colorIn.b;
        float max = Math.Max(Math.Max(r,g), b);
        float brightenAmount = max * amt + 1f * (1 - amt);
        r = r / brightenAmount;
        g = g / brightenAmount;
        b = b / brightenAmount;
        //Plugin.logger.LogInfo(max + " " + brightenAmount + " " + r + " " + g + " " + b);
        UnityEngine.Color cOut = new UnityEngine.Color(r, g, b);
        return cOut;
    }

    [HarmonyPatch(typeof(StyleHUD), "AddPoints")]
    public class StyleHUDAddPointsPatch
    {
        [HarmonyPrefix]
        private static void Prefix(int points, string pointID, GameObject sourceWeapon = null, EnemyIdentifier eid = null, int count = -1, string prefix = "", string postfix = "")
        {
            lastTimeStyleGain = Time.realtimeSinceStartup;
            styleGainedThisTick.Add(points);
            recentStyle += points;
        }
    }

    [HarmonyPatch(typeof(StyleHUD), "AscendRank")]
    public class StyleHUDAscendRankPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            lastTimeRankGain = Time.realtimeSinceStartup;
        }
    }

    [HarmonyPatch(typeof(StyleHUD), "DescendRank")]
    public class StyleHUDDescendRankPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            lastTimeRankLose = Time.realtimeSinceStartup;
        }
    }

    //copied from uk_DamageStats
    [HarmonyPatch(typeof(EnemyIdentifier))]
    public class EnemyIdentifierPatch
    {
        [HarmonyPatch(nameof(EnemyIdentifier.DeliverDamage))]
        static void Prefix(EnemyIdentifier __instance, out float __state)
        {
            // Save previous health
            __state = __instance.health;
        }
        [HarmonyPatch(nameof(EnemyIdentifier.DeliverDamage))]
        static void Postfix(EnemyIdentifier __instance, float __state, float multiplier, GameObject sourceWeapon)
        {
            float initial_health = __state;
            float final_health;
            //bool killing_blow = false;
            
            // this guy was dead even before we started
            if (initial_health <= 0) {
                return;
            }
            
            // Maybe unnecessary?
            __instance.ForceGetHealth();

            // Clamp value if it was a killing blow
            if (__instance.health <= 0) {
                final_health = 0;
                //killing_blow = true;
                lastTimeKill = Time.realtimeSinceStartup;
                //list of all enemies that you hook towards, "big" enemies
                EnemyType[] arr =  {EnemyType.Cerberus, EnemyType.HideousMass, EnemyType.MaliciousFace, EnemyType.Mindflayer, EnemyType.Swordsmachine, EnemyType.V2, EnemyType.Virtue,
                                    EnemyType.Minos, EnemyType.Gabriel, EnemyType.FleshPrison, EnemyType.MinosPrime, EnemyType.Sisyphus, EnemyType.Turret, EnemyType.V2Second,
                                    EnemyType.VeryCancerousRodent, EnemyType.Ferryman, EnemyType.Leviathan, EnemyType.GabrielSecond, EnemyType.SisyphusPrime, EnemyType.FleshPanopticon, EnemyType.Mannequin,
                                    EnemyType.Minotaur, EnemyType.Gutterman, EnemyType.Guttertank, EnemyType.Centaur};
                if(arr.Contains(__instance.enemyType)) {lastKillBig = true; recentKills += 3;} else {lastKillBig = false; recentKills += 1;}
            }
            else {
                final_health = __instance.health;
            }
            float dhealth = initial_health - final_health;
            if(dhealth != 0 && __instance.hitter != "enemy") {lastTimeDamage = Time.realtimeSinceStartup;}
            if(__instance.hitter != "enemy") {damageDealtThisTick.Add(dhealth); recentDamage += dhealth;}
        }
    }
}