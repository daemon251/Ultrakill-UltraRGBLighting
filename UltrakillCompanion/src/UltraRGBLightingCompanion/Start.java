package UltraRGBLightingCompanion;

import java.awt.Color;
import java.awt.FontFormatException;
import java.io.File;
import java.io.IOException;
import java.net.URISyntaxException;
import java.util.Timer;
import java.util.TimerTask;

import io.gitlab.mguimard.openrgb.entity.*;
import io.gitlab.mguimard.openrgb.client.*;
//import io.gitlab.mguimard.openrgb.utils.*;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Scanner;

//TODO
//bugs
//naming
//altColors sets colors anyway... refactor
//indexes green if letter... bad

public class Start 
{
	public static final String[] styleRanks = {	" NO RANK", 
												" DESTRUCTIVE", 
												" CHAOTIC", 
												" BRUTAL", 
												" ANARCHIC", 
												" SUPREME", 
												" SSADISTIC", 
												" SSSHITSTORM", 
												" ULTRAKILL"};
	
	public static final Color[] styleColors ={	new Color(255, 255, 255),
												new Color(26, 161, 255),
												new Color(26, 255, 26),
												new Color(235, 235, 24),
												new Color(255, 174, 26),
												new Color(215, 22, 22),
												new Color(230, 24, 24),
												new Color(255, 26, 26),
												new Color(255, 255, 0),};
	
	public static long refreshRateMS = 25; //loaded from the UK plugin
	
	//*************************//
	//*PREFERENCE LOADER START*//
	//*************************//
	
	public static String outputTXTpath = "";
	
	public static Color[] colors = {styleColors[0], styleColors[1], styleColors[2], styleColors[3], styleColors[4], styleColors[5], styleColors[6], styleColors[7], styleColors[8]};
	public static Color[] altColors = {styleColors[0], styleColors[1], styleColors[2], styleColors[3], styleColors[4], styleColors[5], styleColors[6], styleColors[7], styleColors[8]};
	
	public static boolean gradientColors = false; 
	//public static String styleFilePath = "";
	
	public static ArrayList<Integer> connectedDeviceIndexes = new ArrayList<Integer>();
	public static boolean useAllDeviceIndexes = false; 
	
	public static double[] pulsateIntensities = new double[9];
	public static double[] pulsateFreqs = new double[9];
	public static double[] flickerIntensities = new double[9];
	public static double[] flickerFreqs = new double[9];
	public static double[] altColorFreqs = new double[9];
	
	//x
	//y
	//SCALE
	
	//***********************//
	//*PREFERENCE LOADER END*//
	//***********************//
	
	public static double[] flickerValuesArray;
	
	public static Timer timer = new Timer();
	
	public static boolean connected = false;
	
	public static OpenRGBClient client;
	
	public static float styleRank = -1f;
	
	public static Color currentColorBase = styleColors[0];
	public static Color currentColorAlt = styleColors[0];
	
	public static String jarPath;
	public static String prefsPath = "";
	public static boolean prefsFileFound = false;
	
	//static int ledCount = 0;
	
	public static void main(String[] args) throws IOException, FontFormatException, URISyntaxException
	{
		jarPath = Start.class.getProtectionDomain().getCodeSource().getLocation().toURI().getPath();
		//System.out.println(jarPath);
		
		//this wont work properly in IDE, switch to other method
		prefsPath = Start.jarPath;
		int slashIndex = prefsPath.lastIndexOf("/");
		prefsPath = prefsPath.substring(0, slashIndex + 1); 
		prefsPath += "UltraRGBLightingPrefs.txt";
		//System.out.println("Preference Path " + prefsPath);
		
		//prefsPath = "C:\\Users\\willb\\Downloads\\UltraRGBLightingPrefs.txt"; //use when building in IDE
		
		if(new File(prefsPath).isFile())
		{
			System.out.println("Preferences file exists.");
			prefsFileFound = true;
		}
		else
		{
			System.out.println("Preferences file was not found. Creating.");
			new File("UltraRGBLightingPrefs.txt").createNewFile();
			try{PrefsManager.writeDefaultPrefs();} catch(Exception ex) {System.out.println("Error making preferences file");ex.printStackTrace();} //write default prefs to populate the file
		}
		
		client = new OpenRGBClient("localhost", 6742, "UltraRGBLightingCompanion");
		try {if(prefsFileFound) {PrefsManager.readScalePref();}} catch (Exception ex) {System.out.println("Error reading preferences file"); ex.printStackTrace();}
		
		Display.createWindow();
		try {if(prefsFileFound) {PrefsManager.readPrefs();}} catch (Exception ex) {System.out.println("Error reading preferences file"); ex.printStackTrace();} //has to come after the window so everything is init
		
		timer.schedule(new TimerTask() 
		{
			public void run()
			{
				readStyleRank();
				Display.updateColorOptions();
				if(connected == false) {return;}
				determineBaseOutputColor();
				setOutputColor();
			}
		}, 0, refreshRateMS);
	}
	
	public static void readStyleRank()
	{
		outputTXTpath = Display.outputTXTpathField.getText();
		try 
		{
			File file = new File(outputTXTpath);
		    Scanner scanner = new Scanner(file);
		    while (scanner.hasNextLine()) 
		    {
		    	styleRank = scanner.nextFloat();
		        refreshRateMS = (int)scanner.nextFloat();
		    }
		    scanner.close();
		} 
		catch (Exception e)
		{
			//e.printStackTrace();
		}
	}
	
	public static void connectToOpenRGB()
	{
		try 
		{
			client.connect();
			connected = true;
			Display.connectButton.setForeground(Color.green);
		}
		catch (IOException e) 
		{
			Display.connectButton.setForeground(Color.red);
			System.out.println("Can't connect to OpenRGB. Try restarting the process and/or restarting the server.");
		}
	}
	
	public static void determineAdvancedColorSettings()
	{
		String string = "";
		for(int i = 0; i < colors.length; i++)
		{
			string = Display.pulsateIntensityFields[i].getText();
			try {pulsateIntensities[i] = Double.parseDouble(string);}
			catch (Exception ex) {}
			
			string = Display.pulsateFreqFields[i].getText();
			try {pulsateFreqs[i] = Double.parseDouble(string);}
			catch (Exception ex) {}
			
			string = Display.flickerIntensityFields[i].getText();
			try {flickerIntensities[i] = Double.parseDouble(string);}
			catch (Exception ex) {}
			
			string = Display.flickerFreqFields[i].getText();
			try {flickerFreqs[i] = Double.parseDouble(string);}
			catch (Exception ex) {}
			
			string = Display.altColorTextfields[i].getText();
			altColors[i] = Display.convertHexadecimalToColor(string);
			
			string = Display.altColorFreqTextfields[i].getText();
			try {altColorFreqs[i] = Double.parseDouble(string);}
			catch (Exception ex) {}
		}
		
	}
	
	public static void determineBaseOutputColor()
	{

		if(styleRank >= 7f) {currentColorBase = colors[8]; currentColorAlt = altColors[8]; return;}
		if(styleRank == -1f) {currentColorBase = colors[0]; currentColorAlt = altColors[0]; return;}
		if(!gradientColors && styleRank >= 0)
		{
			currentColorBase = colors[(int) Math.floor(styleRank) + 1];
			currentColorAlt = altColors[(int) Math.floor(styleRank) + 1];
		}
		if(gradientColors)
		{
			Color color1 = colors[(int) Math.floor(styleRank) + 1];
			Color color2 = colors[(int) Math.ceil(styleRank) + 1];
			float fraction = styleRank - (float)Math.floor(styleRank);
			
			currentColorBase = new Color(  (int)(color2.getRed() * fraction + color1.getRed() * (1f - fraction)), 
					 			(int)(color2.getGreen() * fraction + color1.getGreen() * (1f - fraction)), 
					 			(int)(color2.getBlue() * fraction + color1.getBlue() * (1f - fraction)));
			
			Color color3 = altColors[(int) Math.floor(styleRank) + 1];
			Color color4 = altColors[(int) Math.ceil(styleRank) + 1];
			
			currentColorAlt = new Color(  (int)(color4.getRed() * fraction + color3.getRed() * (1f - fraction)), 
					 			(int)(color4.getGreen() * fraction + color3.getGreen() * (1f - fraction)), 
					 			(int)(color4.getBlue() * fraction + color3.getBlue() * (1f - fraction)));
		}
	}
	
	public static OpenRGBColor[] pulsateColor(OpenRGBColor[] colors)
	{
		OpenRGBColor color = colors[0]; //assume all the same
		
		int index = 0;
		if(styleRank >= 7f) {index = 8;}
		else if(styleRank < 0) {index = 0;}
		else {index = (int) Math.floor(styleRank) + 1;}
		double intensity = pulsateIntensities[index];
		double frequency = pulsateFreqs[index] * (2.0 * Math.PI) / 10.0; //amount of tenths a sin cycle per second
		if(frequency == 0.0) {return colors;}
		Double currentMult = (1 - intensity / 200.0) - (intensity / 200.0) * Math.sin(frequency * System.currentTimeMillis() / 1000f);
		if(currentMult > 1) {currentMult = 1.0;}
		if(currentMult < 0) {currentMult = 0.0;}
		
		// getColor() & 0xff turns the byte into an unsigned "int". If you just read the byte normally it gives you unsigned "int" which is not helpful.
		color = new OpenRGBColor((int)(currentMult * (color.getRed() & 0xff)), (int)(currentMult * (color.getGreen() & 0xff)), (int)(currentMult * (color.getBlue() & 0xff)));
		for(int i = 0; i < colors.length; i++)
		{
			colors[i] = color;
		}
		return colors;
	}
	
	static int timePassed = 0;
	public static OpenRGBColor[] flickerColor(OpenRGBColor[] colors)
	{
		int index = 0;
		if(styleRank >= 7f) {index = 8;}
		else if(styleRank < 0) {index = 0;}
		else {index = (int) Math.floor(styleRank) + 1;}
		double intensity = flickerIntensities[index];
		double frequency = flickerFreqs[index] / 10.0; //amount of tenths a cycle per second
		if(frequency == 0.0) {return colors;}
		timePassed += refreshRateMS;
		if(timePassed >= 1000 / frequency) //if enough time has passed
		{
			timePassed = 0;
			flickerValuesArray = new double[colors.length];
			for(int i = 0; i < colors.length; i++)
			{
				double currentMult = (1 - intensity / 100.0) - (intensity / 100.0) * Math.random();
				flickerValuesArray[i] = currentMult;

			}
		}
		
		for(int i = 0; i < colors.length; i++)
		{
			if(flickerValuesArray != null)
			{
				double currentMult = flickerValuesArray[i];
				// getColor() & 0xff turns the byte into an unsigned "int". If you just read the byte normally it gives you unsigned "int" which is not helpful.
				colors[i] = new OpenRGBColor((int)(currentMult * (colors[i].getRed() & 0xff)), (int)(currentMult * (colors[i].getGreen() & 0xff)), (int)(currentMult * (colors[i].getBlue() & 0xff)));
			}
		}
		
		return colors;
	}
	
	//we set colors here anyway?
	public static OpenRGBColor[] altColors(OpenRGBColor[] colorsBoard)
	{
		int index = 0;
		if(styleRank >= 7f) {index = 8;}
		else if(styleRank < 0) {index = 0;}
		else {index = (int) Math.floor(styleRank) + 1;}
		double frequency = altColorFreqs[index] * (2.0 * Math.PI) / 10.0; //amount of tenths a sin cycle per second
		Color altColor = currentColorAlt;
		Color color = currentColorBase;
		if(frequency == 0.0) {return colorsBoard;}
		Double currentFraction = Math.sin(frequency * System.currentTimeMillis() / 1000f) / 2.0 + 0.5;
		if(currentFraction > 1) {currentFraction = 1.0;}
		if(currentFraction < 0) {currentFraction = 0.0;}
		
		int red = (int)(color.getRed() * currentFraction + altColor.getRed() * (1 - currentFraction));
		int green = (int)(color.getGreen() * currentFraction + altColor.getGreen() * (1 - currentFraction));
		int blue = (int)(color.getBlue() * currentFraction + altColor.getBlue() * (1 - currentFraction));
		
		OpenRGBColor outputColor = new OpenRGBColor(red, green, blue);
		for(int i = 0; i < colorsBoard.length; i++)
		{
			colorsBoard[i] = outputColor;
		}
		
		return colorsBoard;
	}
	
	public static OpenRGBColor[] determineFinalOutputColors(OpenRGBColor[] colors)
	{
		determineAdvancedColorSettings();
		colors = altColors(colors);
		colors = pulsateColor(colors);
		colors = flickerColor(colors);
		return colors;
	}
	
	public static boolean findDeviceIndexes() //returns true if the deviceIndexField.getText() has valid indexes
	{
		String string = Display.deviceIndexField.getText();
		string = string.replaceAll("\\s", ""); //gets rid of spaces
		String[] substrings = string.split(",");
		useAllDeviceIndexes = false;
		connectedDeviceIndexes = new ArrayList<Integer>();
		for(int i = 0; i < substrings.length; i++)
		{
			String currentString = substrings[i];
			if(currentString.toLowerCase().equals("all"))
	        {
				useAllDeviceIndexes = true;
				Display.deviceIndexField.setForeground(Color.green);
				return true;
	        }
	        try {Integer.parseInt(currentString, 10);}
			catch (Exception e) {continue;}
	        
	        int tempInt = Integer.parseInt(currentString, 10);
	        int controllerCount = 0;
	        
	        try {controllerCount = client.getControllerCount();} 
	        catch (IOException e) {}//e.printStackTrace();}}
	        if(tempInt < controllerCount) {connectedDeviceIndexes.add(Integer.parseInt(currentString, 10));}
	        else {Display.deviceIndexField.setForeground(Color.red); return false;}
		}
		Display.deviceIndexField.setForeground(Color.green);
        return true;
	}
	
	public static void setOutputColor()
	{
		if(findDeviceIndexes() == false) {return;}
		ArrayList<Integer> deviceListTemp = new ArrayList<Integer>();
		
		if(useAllDeviceIndexes == true)
		{
	        int controllerCount = 0;
	        try {controllerCount = client.getControllerCount();} 
	        catch (IOException e) {}//e.printStackTrace();}}
			for(int i = 0; i < controllerCount; i++)
			{
				deviceListTemp.add(i);
			}
		}
		else
		{
			deviceListTemp = connectedDeviceIndexes;
		}
		for(int deviceIndex : deviceListTemp)
		{
			try 
	        {
	            OpenRGBDevice controller = client.getDeviceController(deviceIndex);
	            int ledCount = 0;
	            for (int i = 0; i < controller.getZones().size(); i++)
	            {
	            	ledCount += controller.getZones().get(i).getLedsMax();
	            }
	
	            OpenRGBColor[] openRGBColors = new OpenRGBColor[ledCount];
	            Arrays.fill(openRGBColors, new OpenRGBColor(currentColorBase.getRed(), currentColorBase.getGreen(), currentColorBase.getBlue()));
	            openRGBColors = determineFinalOutputColors(openRGBColors);
	
	            client.updateLeds(deviceIndex, openRGBColors);
	        } 
	        catch (Exception e) 
	        {
		        e.printStackTrace();
		        return;
	        }
			
		}
		
	}

}
