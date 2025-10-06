package UltraRGBLightingCompanion;

import java.awt.Color;
import java.awt.Font;
import java.awt.FontFormatException;
import java.awt.GraphicsEnvironment;
import java.awt.Image;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.ComponentAdapter;
import java.awt.event.ComponentEvent;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.io.IOException;

import javax.imageio.ImageIO;
import javax.swing.*;

public class Display 
{
	static JFrame frame = new JFrame();;
	static float globalScale = 1f;
	
	static int xDimensionBase = 900;
	static int yDimensionBase = 525;
	
	static int xDimension = (int)(xDimensionBase * globalScale);
	static int yDimension = (int)(yDimensionBase * globalScale);
	
	static String[] colorText = new String[9];
	static JTextField[] colorTextfields = new JTextField[9];
	
	static String[] altColorText = new String[9];
	static JTextField[] altColorTextfields = new JTextField[9];
	static JTextField[] altColorFreqTextfields = new JTextField[9];
	
	static JButton[] buttons = new JButton[9];
	
	static JTextField[] pulsateIntensityFields = new JTextField[9];
	static JTextField[] pulsateFreqFields = new JTextField[9];
	static JTextField[] flickerIntensityFields = new JTextField[9];
	static JTextField[] flickerFreqFields = new JTextField[9];
	
	public static Color convertHexadecimalToColor(String string)
	{
		string = string.replaceAll("#", "");
		int r = 255;
		int g = 255;
		int b = 255;
		
		Color color = new Color(r, g, b);
		
		//lazy but works
		try {Integer.parseInt(string, 16);}
		catch (Exception e) {return color;}//System.out.println("String \"" + string + "\" is not a hexadecimal number. Returning white color."); return color;}
		
		if(string.length() != 6) {return color;} //System.out.println("String \"" + string + "\" is not the correct length. Returning white color."); }
		
		r = Integer.parseInt(string.substring(0, 2), 16);
		g = Integer.parseInt(string.substring(2, 4), 16);
		b = Integer.parseInt(string.substring(4, 6), 16);
		
		color = new Color(r, g, b);
		
		return color;
	}
	
	public static String convertColorToHexadecimal(Color color) //copied
	{
	    String hex = Integer.toHexString(color.getRGB() & 0xffffff);
	    if(hex.length() < 6) 
	    {
	        if(hex.length()==5)
	            hex = "0" + hex;
	        if(hex.length()==4)
	            hex = "00" + hex;
	        if(hex.length()==3)
	            hex = "000" + hex;
	    }
	    //hex = "#" + hex;
	    hex = hex.toUpperCase(); //uppercase looks better
	    return hex;
	}

	static Image checkboxFullImage;
	static Image checkboxEmptyImage;
	static JTextField deviceIndexField;
	static JButton connectButton;
	static JTextField outputTXTpathField;
	static JTextField altColorFreqField;
	static JTextField resizeTextField;
	static JButton gradientButton;
	static void createWindow() throws IOException, FontFormatException
	{
		checkboxFullImage = ImageIO.read(Display.class.getResource("checkboxFilled.png").openStream()).getScaledInstance((int)(21 * globalScale), (int)(21 * globalScale), Image.SCALE_DEFAULT);
		checkboxEmptyImage = ImageIO.read(Display.class.getResource("checkboxEmpty.png").openStream()).getScaledInstance((int)(21 * globalScale), (int)(21 * globalScale), Image.SCALE_DEFAULT);
		
		Font sizedFont = null;
		Font font = Font.createFont(Font.TRUETYPE_FONT, Display.class.getResource("VCR_OSD_MONO.ttf").openStream());
		sizedFont = font.deriveFont(14.5f * globalScale);
		frame.getContentPane().setBackground(Color.black);
		frame.setSize((int)(xDimension * globalScale), (int)(yDimension * globalScale));
		frame.setResizable(true);
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		frame.setLayout(null);
		frame.setTitle("UltraRGBLighting Companion");
		Image iconImage = ImageIO.read(Display.class.getResource("icon.png").openStream());
		frame.setIconImage(iconImage);

		frame.addWindowListener(new WindowAdapter()
		{
		     public void windowClosing(WindowEvent e) 
		     {
		    	 System.out.println("UltraRGBLightingCompanion Saving Prefs");
		    	 try {if(Start.prefsFileFound) {PrefsManager.writePrefs();}} catch (IOException ex) {ex.printStackTrace();}
		    	 System.exit(0); //hopefully this causes less ghost processes
		     }
		});
		
		deviceIndexField = new JTextField();
		deviceIndexField.setBounds((int)(210 * globalScale), (int)(40 * globalScale), (int)(265 * globalScale), (int)(25 * globalScale));
		deviceIndexField.setBorder(BorderFactory.createLineBorder(Color.white));
		deviceIndexField.setFont(sizedFont);
		deviceIndexField.setForeground(Color.white);
		deviceIndexField.setBackground(Color.black);
		deviceIndexField.setCaretColor(Color.white);
		deviceIndexField.setHorizontalAlignment(JLabel.CENTER);
		deviceIndexField.setText("ALL");
		frame.add(deviceIndexField);
		
		JLabel outputTXTpathLabel = new JLabel(" OUTPUT.TXT LOCATION:");
		outputTXTpathLabel.setBounds((int)(10 * globalScale), (int)(10 * globalScale), (int)(190 * globalScale), (int)(25 * globalScale));
		outputTXTpathLabel.setBorder(BorderFactory.createLineBorder(Color.white));
		outputTXTpathLabel.setFont(sizedFont);
		outputTXTpathLabel.setForeground(Color.white);
		outputTXTpathLabel.setBackground(Color.black);
		frame.add(outputTXTpathLabel);
		
		outputTXTpathField = new JTextField();
		outputTXTpathField.setBounds((int)(200 * globalScale), (int)(10 * globalScale), (int)(660 * globalScale), (int)(25 * globalScale));
		outputTXTpathField.setBorder(BorderFactory.createLineBorder(Color.white));
		outputTXTpathField.setFont(sizedFont);
		outputTXTpathField.setForeground(Color.white);
		outputTXTpathField.setBackground(Color.black);
		outputTXTpathField.setCaretColor(Color.white);
		outputTXTpathField.setHorizontalAlignment(JLabel.CENTER);
		outputTXTpathField.setText("output.txt"); //overriden if prefs are loaded
		frame.add(outputTXTpathField);
		
		JLabel deviceIndexLabel = new JLabel(" DEVICE INDEXES ADDED:");
		deviceIndexLabel.setBounds((int)(10 * globalScale), (int)(40 * globalScale), (int)(210 * globalScale), (int)(25 * globalScale));
		deviceIndexLabel.setBorder(BorderFactory.createLineBorder(Color.white));
		deviceIndexLabel.setFont(sizedFont);
		deviceIndexLabel.setForeground(Color.white);
		frame.add(deviceIndexLabel);
		
		//looks bad
		connectButton = new JButton("CONNECT");
		connectButton.setBounds((int)(485 * globalScale), (int)(40 * globalScale), (int)(205 * globalScale), (int)(25 * globalScale));
		connectButton.setBorder(BorderFactory.createLineBorder(Color.white));
		connectButton.setFont(sizedFont);
		connectButton.setForeground(Color.white);
		connectButton.setBackground(Color.black);
		frame.add(connectButton);
		
		connectButton.addActionListener(new ActionListener()
		{
			@Override
			public void actionPerformed(ActionEvent e) 
			{
				Start.connectToOpenRGB();
			}
			
		});
		
		JLabel resizeLabel = new JLabel(" PANEL SCALE: ");
		resizeLabel.setBounds((int)(700 * globalScale), (int)(40 * globalScale), (int)(160 * globalScale), (int)(25 * globalScale));
		resizeLabel.setBorder(BorderFactory.createLineBorder(Color.white));
		resizeLabel.setFont(sizedFont);
		resizeLabel.setForeground(Color.white);
		frame.add(resizeLabel);
		
		resizeTextField = new JTextField();
		resizeTextField.setBounds((int)(820 * globalScale), (int)(40 * globalScale), (int)(40 * globalScale), (int)(25 * globalScale));
		resizeTextField.setBorder(BorderFactory.createLineBorder(Color.white));
		resizeTextField.setFont(sizedFont);
		resizeTextField.setForeground(Color.white);
		resizeTextField.setBackground(Color.black);
		resizeTextField.setCaretColor(Color.white);
		resizeTextField.setHorizontalAlignment(JLabel.CENTER);
		resizeTextField.addActionListener(new ActionListener()
		{
			@Override
			public void actionPerformed(ActionEvent e) 
			{
				rescaleFrame();
			}
		});
		frame.add(resizeTextField);
		
		JLabel colorHeaderLabel = new JLabel("COLOR SETTINGS", SwingConstants.CENTER);
		colorHeaderLabel.setBounds((int)(10 * globalScale), (int)(70 * globalScale), (int)(345 * globalScale), (int)(25 * globalScale));
		colorHeaderLabel.setBorder(BorderFactory.createLineBorder(Color.white));
		colorHeaderLabel.setFont(sizedFont.deriveFont(Font.BOLD));
		colorHeaderLabel.setForeground(Color.white);
		colorHeaderLabel.setHorizontalAlignment(JLabel.CENTER);
		frame.add(colorHeaderLabel);
		
		for (int i = 0; i < Start.styleRanks.length; i++)
		{
			JLabel label = new JLabel();
			label.setBounds((int)(10 * globalScale),(int)((i * 30 + 100) * globalScale),(int)(130 * globalScale),(int)(25 * globalScale));
			label.setBorder(BorderFactory.createLineBorder(Color.white));
			label.setText(Start.styleRanks[i]);
			label.setForeground(Start.styleColors[i]);
			label.setBackground(Color.red);
			label.setFont(sizedFont);
			if(i == 8) {label.setFont(sizedFont.deriveFont(Font.ITALIC + Font.BOLD));}
			frame.add(label);
			
			JTextField textfield = new JTextField();
			textfield.setBounds((int)(270 * globalScale),(int)((i * 30 + 100) * globalScale),(int)(85 * globalScale),(int)(25 * globalScale));
			
			Color color = new Color((int)(Start.colors[i].getRed() * 0.8f) + 50, (int)(Start.colors[i].getGreen() * 0.8f) + 50, (int)(Start.colors[i].getBlue() * 0.8f) + 50);
			textfield.setForeground(color);
			textfield.setBorder(BorderFactory.createLineBorder(color));
				
			textfield.setBackground(Color.black);
			textfield.setCaretColor(Color.white);
			textfield.setFont(sizedFont);
			textfield.setText(convertColorToHexadecimal(Start.colors[i]));
			textfield.setHorizontalAlignment(JLabel.CENTER);
			
			colorTextfields[i] = textfield;
			frame.add(textfield);
			
			JLabel label3 = new JLabel();
			label3.setBounds((int)(150 * globalScale),(int)((i * 30 + 100) * globalScale),(int)(150 * globalScale),(int)(25 * globalScale));
			label3.setBorder(BorderFactory.createLineBorder(Color.white));
			label3.setText(" COLOR (HEX):");
			label3.setForeground(Color.white);
			label3.setFont(sizedFont);
			frame.add(label3);
		}
		
		gradientButton = new JButton();
		gradientButton.setBounds((int)(330 * globalScale),(int)(372 * globalScale),(int)(21 * globalScale),(int)(21 * globalScale));
		if(Start.gradientColors == true) {gradientButton.setIcon(new ImageIcon(checkboxFullImage));}
		else {gradientButton.setIcon(new ImageIcon(checkboxEmptyImage));}
		gradientButton.setBorder(BorderFactory.createLineBorder(Color.white));
		frame.add(gradientButton);
		
		gradientButton.addActionListener(new ActionListener()
		{
			@Override
			public void actionPerformed(ActionEvent e) 
			{
				Start.gradientColors = !Start.gradientColors;
				if(Start.gradientColors == true) {gradientButton.setIcon(new ImageIcon(checkboxFullImage));}
				else {gradientButton.setIcon(new ImageIcon(checkboxEmptyImage));}
			}
		});
		
		JLabel fadeColorsLabel = new JLabel(" GRADIENTIZE COLORS BETWEEN RANKS:");
		fadeColorsLabel.setBounds((int)(10 * globalScale), (int)(370 * globalScale), (int)(345 * globalScale), (int)(25 * globalScale));
		fadeColorsLabel.setBorder(BorderFactory.createLineBorder(Color.white));
		fadeColorsLabel.setFont(sizedFont);
		fadeColorsLabel.setForeground(Color.white);
		frame.add(fadeColorsLabel);
		
		frame.addComponentListener(new ComponentAdapter()
		{
			public void componentResized(ComponentEvent evt) 
			{
				resizeTextField.setText(Float.toString(globalScale));
	        }
		});
		
		JLabel advancedColorHeaderLabel = new JLabel("ADVANCED COLOR SETTINGS", SwingConstants.CENTER);
		advancedColorHeaderLabel.setBounds((int)(365 * globalScale), (int)(70 * globalScale), (int)(495 * globalScale), (int)(25 * globalScale));
		advancedColorHeaderLabel.setBorder(BorderFactory.createLineBorder(Color.white));
		advancedColorHeaderLabel.setFont(sizedFont.deriveFont(Font.BOLD));
		advancedColorHeaderLabel.setForeground(Color.white);
		advancedColorHeaderLabel.setHorizontalAlignment(JLabel.CENTER);
		frame.add(advancedColorHeaderLabel);
		
		for (int i = 0; i < Start.styleRanks.length; i++)
		{
			JTextField textfield = new JTextField();
			textfield.setBounds((int)(520 * globalScale),(int)((i * 30 + 100) * globalScale),(int)(85 * globalScale),(int)(25 * globalScale));
			
			Color altColor = new Color((int)(Start.altColors[i].getRed() * 0.8f) + 50, (int)(Start.altColors[i].getGreen() * 0.8f) + 50, (int)(Start.altColors[i].getBlue() * 0.8f) + 50);
			textfield.setForeground(altColor);
			textfield.setBorder(BorderFactory.createLineBorder(altColor));
				
			textfield.setBackground(Color.black);
			textfield.setCaretColor(Color.white);
			textfield.setFont(sizedFont);
			textfield.setText(convertColorToHexadecimal(Start.colors[i]));
			textfield.setHorizontalAlignment(JLabel.CENTER);
			
			altColorTextfields[i] = textfield;
			frame.add(textfield);
			
			JLabel label = new JLabel();
			label.setBounds((int)(365 * globalScale),(int)((i * 30 + 100) * globalScale),(int)(185 * globalScale),(int)(25 * globalScale));
			label.setBorder(BorderFactory.createLineBorder(Color.white));
			label.setText(" ALT COLOR (HEX):");
			label.setForeground(Color.white);
			label.setFont(sizedFont);
			frame.add(label);
			
			altColorFreqField = new JTextField();
			altColorFreqField.setBounds((int)(605 * globalScale), (int)((100 + 30 * i) * globalScale), (int)(35 * globalScale), (int)(25 * globalScale));
			altColorFreqField.setBorder(BorderFactory.createLineBorder(Color.white));
			altColorFreqField.setFont(sizedFont.deriveFont(Font.BOLD));
			altColorFreqField.setForeground(Color.white);
			altColorFreqField.setHorizontalAlignment(JLabel.CENTER);
			altColorFreqField.setBackground(Color.black);
			altColorFreqField.setCaretColor(Color.white);
			altColorFreqField.setText("0");
			altColorFreqTextfields[i] = altColorFreqField;
			frame.add(altColorFreqField);
			
			JTextField pulsateIntensityField = new JTextField();
			pulsateIntensityField.setBounds((int)(680 * globalScale), (int)((100 + 30 * i) * globalScale), (int)(35 * globalScale), (int)(25 * globalScale));
			pulsateIntensityField.setBorder(BorderFactory.createLineBorder(Color.white));
			pulsateIntensityField.setFont(sizedFont.deriveFont(Font.BOLD));
			pulsateIntensityField.setForeground(Color.white);
			pulsateIntensityField.setHorizontalAlignment(JLabel.CENTER);
			pulsateIntensityField.setBackground(Color.black);
			pulsateIntensityField.setCaretColor(Color.white);
			pulsateIntensityField.setText("0");
			pulsateIntensityFields[i] = pulsateIntensityField;
			frame.add(pulsateIntensityField);
			
			
			JTextField pulsateFreqField = new JTextField();
			pulsateFreqField.setBounds((int)(715 * globalScale), (int)((100 + 30 * i) * globalScale), (int)(35 * globalScale), (int)(25 * globalScale));
			pulsateFreqField.setBorder(BorderFactory.createLineBorder(Color.white));
			pulsateFreqField.setFont(sizedFont.deriveFont(Font.BOLD));
			pulsateFreqField.setForeground(Color.white);
			pulsateFreqField.setHorizontalAlignment(JLabel.CENTER);
			pulsateFreqField.setBackground(Color.black);
			pulsateFreqField.setCaretColor(Color.white);
			pulsateFreqField.setText("0");
			pulsateFreqFields[i] = pulsateFreqField;
			frame.add(pulsateFreqField);
			
			JLabel pulsateSettingLabel = new JLabel(" P:");
			pulsateSettingLabel.setBounds((int)(650 * globalScale), (int)((100 + 30 * i) * globalScale), (int)(100 * globalScale), (int)(25 * globalScale));
			pulsateSettingLabel.setBorder(BorderFactory.createLineBorder(Color.white));
			pulsateSettingLabel.setFont(sizedFont.deriveFont(Font.BOLD));
			pulsateSettingLabel.setForeground(Color.white);
			frame.add(pulsateSettingLabel);
			
			JTextField flickerIntensityField = new JTextField();
			flickerIntensityField.setBounds((int)(790 * globalScale), (int)((100 + 30 * i) * globalScale), (int)(35 * globalScale), (int)(25 * globalScale));
			flickerIntensityField.setBorder(BorderFactory.createLineBorder(Color.white));
			flickerIntensityField.setFont(sizedFont.deriveFont(Font.BOLD));
			flickerIntensityField.setForeground(Color.white);
			flickerIntensityField.setHorizontalAlignment(JLabel.CENTER);
			flickerIntensityField.setBackground(Color.black);
			flickerIntensityField.setCaretColor(Color.white);
			flickerIntensityField.setText("0");
			flickerIntensityFields[i] = flickerIntensityField;
			frame.add(flickerIntensityField);
			
			JTextField flickerFreqField = new JTextField();
			flickerFreqField.setBounds((int)(825 * globalScale), (int)((100 + 30 * i) * globalScale), (int)(35 * globalScale), (int)(25 * globalScale));
			flickerFreqField.setBorder(BorderFactory.createLineBorder(Color.white));
			flickerFreqField.setFont(sizedFont.deriveFont(Font.BOLD));
			flickerFreqField.setForeground(Color.white);
			flickerFreqField.setHorizontalAlignment(JLabel.CENTER);
			flickerFreqField.setBackground(Color.black);
			flickerFreqField.setCaretColor(Color.white);
			flickerFreqField.setText("0");
			flickerFreqFields[i] = flickerFreqField;
			frame.add(flickerFreqField);
			
			JLabel flickerSettingLabel = new JLabel(" F:");
			flickerSettingLabel.setBounds((int)(760 * globalScale), (int)((100 + 30 * i) * globalScale), (int)(100 * globalScale), (int)(25 * globalScale));
			flickerSettingLabel.setBorder(BorderFactory.createLineBorder(Color.white));
			flickerSettingLabel.setFont(sizedFont.deriveFont(Font.BOLD));
			flickerSettingLabel.setForeground(Color.white);
			frame.add(flickerSettingLabel);
		}
		
		//nessesary to set the font in HTML. Only needed here, so here it is.
		GraphicsEnvironment ge = GraphicsEnvironment.getLocalGraphicsEnvironment();
		ge.registerFont(sizedFont);
		String fontfamily = sizedFont.getFamily();
		
		JLabel advancedColorExplainerLabel = new JLabel("<html><p style=\"font-family: " + fontfamily + ";text-align:center\"<b>ALTERNATE AND NORMAL COLOR WILL ALTERNATE AT THE FREQUENCY OF VALUE IN THE SECOND FIELD IN DECIHZ. IF SECOND FIELD IS ZERO, THEN NO ALTERNATING WILL OCCUR.</b></p></html>.", SwingConstants.CENTER);
		advancedColorExplainerLabel.setBounds((int)(365 * globalScale), (int)(370 * globalScale), (int)(275 * globalScale), (int)(100 * globalScale));
		advancedColorExplainerLabel.setBorder(BorderFactory.createLineBorder(Color.white));
		advancedColorExplainerLabel.setFont(sizedFont.deriveFont(Font.BOLD)); //this field is still required... to size the font. makes no sense
		advancedColorExplainerLabel.setForeground(Color.white);
		advancedColorExplainerLabel.setHorizontalAlignment(JLabel.CENTER);
		frame.add(advancedColorExplainerLabel);
		
		JLabel advancedEffectExplainerLabel = new JLabel("<html><p style=\"font-family: " + fontfamily + ";text-align:center\"<b>P - PULSATE <br> F - FLICKER <br> FIRST ARGUMENT IS INTENSITY (0-100), SECOND IS FREQUENCY IN DECIHZ.</b></p></html>.", SwingConstants.CENTER);
		advancedEffectExplainerLabel.setBounds((int)(650 * globalScale), (int)(370 * globalScale), (int)(210 * globalScale), (int)(100 * globalScale));
		advancedEffectExplainerLabel.setBorder(BorderFactory.createLineBorder(Color.white));
		advancedEffectExplainerLabel.setFont(sizedFont.deriveFont(Font.BOLD));
		advancedEffectExplainerLabel.setForeground(Color.white);
		advancedEffectExplainerLabel.setHorizontalAlignment(JLabel.CENTER);
		frame.add(advancedEffectExplainerLabel);
		
		JLabel gradientEffectExplainerLabel = new JLabel("<html><p style=\"font-family: " + fontfamily + ";text-align:center\"<b>THE GRADIENTIZE SETTING MAKES IT SO THAT BEING INBETWEEN RANKS WILL MAKE THE OUTPUT COLOR INBETWEEN THEIR COLORS.</b></p></html>.", SwingConstants.CENTER);
		gradientEffectExplainerLabel.setBounds((int)(10 * globalScale), (int)(400 * globalScale), (int)(345 * globalScale), (int)(70 * globalScale));
		gradientEffectExplainerLabel.setBorder(BorderFactory.createLineBorder(Color.white));
		gradientEffectExplainerLabel.setFont(sizedFont.deriveFont(Font.BOLD));
		gradientEffectExplainerLabel.setForeground(Color.white);
		gradientEffectExplainerLabel.setHorizontalAlignment(JLabel.CENTER);
		frame.add(gradientEffectExplainerLabel);
		
		frame.setVisible(true);
	}
	
	public static void updateColorOptions()
	{
		for(int i = 0; i < Start.styleRanks.length; i++)
		{
			if(colorText[i] != null && !colorText[i].equals(colorTextfields[i].getText()))
			{
				Start.colors[i] = convertHexadecimalToColor(colorTextfields[i].getText());
				
				Color color = new Color((int)(Start.colors[i].getRed() * 0.8f) + 50, (int)(Start.colors[i].getGreen() * 0.8f) + 50, (int)(Start.colors[i].getBlue() * 0.8f) + 50);
				colorTextfields[i].setForeground(color);
				colorTextfields[i].setBorder(BorderFactory.createLineBorder(color));
			}
			colorText[i] = colorTextfields[i].getText();
			
			if(altColorText[i] != null && !altColorText[i].equals(altColorTextfields[i].getText()))
			{
				Start.altColors[i] = convertHexadecimalToColor(altColorTextfields[i].getText());
				
				Color altColor = new Color((int)(Start.altColors[i].getRed() * 0.8f) + 50, (int)(Start.altColors[i].getGreen() * 0.8f) + 50, (int)(Start.altColors[i].getBlue() * 0.8f) + 50);
				altColorTextfields[i].setForeground(altColor);
				altColorTextfields[i].setBorder(BorderFactory.createLineBorder(altColor));
			}
			altColorText[i] = altColorTextfields[i].getText();
		}
	}
	
	public static void rescaleFrame() //this sucks bad
	{
		try {Float.parseFloat(resizeTextField.getText());} catch (Exception ex) {}
		float value = Float.parseFloat(resizeTextField.getText());
		try 
		{
			globalScale = value; 
			frame.getContentPane().removeAll(); frame.revalidate(); frame.pack(); createWindow(); 
			//this done after to get correct dimensions
			try {PrefsManager.writePrefs();} 
			catch (IOException e) {e.printStackTrace();}
		} 
        catch (IOException ex) {ex.printStackTrace();} 
        catch (FontFormatException ex) {ex.printStackTrace();}
		
		try {PrefsManager.readPrefs();} 
		catch (IOException e) {e.printStackTrace();}
	}
}
