package UltraRGBLightingCompanion;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.Scanner;

import javax.swing.ImageIcon;

public class PrefsManager 
{
	public static void readScalePref() throws IOException //more spaghetti
	{
		File prefsFile = new File(Start.prefsPath);
		
		Scanner sc = new Scanner(prefsFile);
		
		float fl = 1.0f;
		int i = 0;
		while(sc.hasNext() == true) //gets last float
		{
			if(i > 1000) {System.out.println("Something is wrong with the preferences file."); break;}
			i++;
			if(sc.hasNextFloat()) {fl = sc.nextFloat();}
			else {sc.next();}
		}
		
		Display.globalScale = fl;
		sc.close();
	}
	
	public static void readPrefs() throws IOException
	{
		try
		{
			File prefsFile = new File(Start.prefsPath);
			Scanner sc = new Scanner(prefsFile);
			
			for(int i = 0; i < 9; i++)
			{
				Start.colors[i] = Display.convertHexadecimalToColor(sc.next());
				Display.colorText[i] = ""; //done so it isnt null... spaghetti
				Display.colorTextfields[i].setText(Display.convertColorToHexadecimal(Start.colors[i]));
			}
			
			for(int i = 0; i < 9; i++)
			{
				Start.altColors[i] = Display.convertHexadecimalToColor(sc.next());
				Display.altColorText[i] = ""; //done so it isnt null... spaghetti
				Display.altColorTextfields[i].setText(Display.convertColorToHexadecimal(Start.colors[i]));
			}
			Start.gradientColors = sc.nextBoolean();
			if(Start.gradientColors == true) {Display.gradientButton.setIcon(new ImageIcon(Display.checkboxFullImage));}
			else {Display.gradientButton.setIcon(new ImageIcon(Display.checkboxEmptyImage));}
			sc.nextLine();
			Start.outputTXTpath = sc.nextLine();
			Display.outputTXTpathField.setText(Start.outputTXTpath);
			Display.deviceIndexField.setText(sc.nextLine());
			for(int i = 0; i < 9; i++)
			{
				Start.pulsateIntensities[i] = sc.nextDouble();
				Display.pulsateIntensityFields[i].setText(String.valueOf(Start.pulsateIntensities[i]));
			}
			for(int i = 0; i < 9; i++)
			{
				Start.pulsateFreqs[i] = sc.nextDouble();
				Display.pulsateFreqFields[i].setText(String.valueOf(Start.pulsateFreqs[i]));
			}
			for(int i = 0; i < 9; i++)
			{
				Start.flickerIntensities[i] = sc.nextDouble();
				Display.flickerIntensityFields[i].setText(String.valueOf(Start.flickerIntensities[i]));
			}
			for(int i = 0; i < 9; i++)
			{
				Start.flickerFreqs[i] = sc.nextDouble();
				Display.flickerFreqFields[i].setText(String.valueOf(Start.flickerFreqs[i]));
			}
			for(int i = 0; i < 9; i++)
			{
				Start.altColorFreqs[i] = sc.nextDouble();
				Display.altColorFreqTextfields[i].setText(String.valueOf(Start.altColorFreqs[i]));
			}
			int width = sc.nextInt();
			int height = sc.nextInt();
			Display.frame.setSize(width, height);
			
			sc.close();
		}
		catch(Exception ex)
		{
			System.out.println("Is the config file in the correct place and correctly configured?");
			ex.printStackTrace();
		}
	}
	
	public static void writeDefaultPrefs() throws IOException
	{
		try
		{
			//note that this will overwrite the file, not add on to it.
			FileWriter fw = new FileWriter(Start.prefsPath);
			for(int i = 0; i < 9; i++)
			{
				fw.write(Display.convertColorToHexadecimal(Start.colors[i]) + " ");
			}
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(Display.convertColorToHexadecimal(Start.altColors[i]) + " ");
			}
			fw.write("\n");
			
			fw.write("false");
			fw.write("\n");
			
			fw.write("output.txt");
			fw.write("\n");
			
			fw.write("ALL");
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(0 + " ");
			}
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(0 + " ");
			}
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(0 + " ");
			}
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(0 + " ");
			}
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(0 + " ");
			}
			fw.write("\n");
			
			fw.write(Display.xDimensionBase + "\n");
			fw.write(Display.yDimensionBase + "\n");
			fw.write(Display.globalScale + "\n");
			
			fw.close();
		}
		catch(Exception ex)
		{
			System.out.println("Is the config file in the correct place?");
			ex.printStackTrace();
		}
	}
	
	public static void writePrefs() throws IOException
	{
		try
		{
			//note that this will overwrite the file, not add on to it.
			//FileWriter fw = new FileWriter("C:\\Users\\willb\\Downloads\\UltraRGBLightingPrefs.txt"); //change
			FileWriter fw = new FileWriter(Start.prefsPath);
			for(int i = 0; i < 9; i++)
			{
				fw.write(Display.convertColorToHexadecimal(Start.colors[i]) + " ");
			}
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(Display.convertColorToHexadecimal(Start.altColors[i]) + " ");
			}
			fw.write("\n");
			
			fw.write(String.valueOf(Start.gradientColors));
			fw.write("\n");
			
			fw.write(Start.outputTXTpath);
			if(Start.outputTXTpath.equals(""))
			{
				fw.write("ADD LOCATION");
			}
			fw.write("\n");
			
			if(Display.deviceIndexField != null)
			{
				fw.write(Display.deviceIndexField.getText());
				if(Display.deviceIndexField.getText().equals("")) {fw.write("NONE");}
			}
			fw.write("\n");
			
			try{Start.determineAdvancedColorSettings();} catch (Exception ex) {}
			for(int i = 0; i < 9; i++)
			{
				fw.write(Start.pulsateIntensities[i] + " ");
			}
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(Start.pulsateFreqs[i] + " ");
			}
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(Start.flickerIntensities[i] + " ");
			}
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(Start.flickerFreqs[i] + " ");
			}
			fw.write("\n");
			
			for(int i = 0; i < 9; i++)
			{
				fw.write(Start.altColorFreqs[i] + " ");
			}
			fw.write("\n");
			
			fw.write(Display.frame.getWidth() + "\n");
			fw.write(Display.frame.getHeight() + "\n");
			fw.write(Display.globalScale + "\n");
			
			fw.close();
		}
		catch(Exception ex)
		{
			System.out.println("Is the config file in the correct place?");
			ex.printStackTrace();
		}
	}
}
