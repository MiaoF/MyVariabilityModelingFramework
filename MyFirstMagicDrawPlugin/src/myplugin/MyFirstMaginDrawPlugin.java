package myplugin;

import com.nomagic.magicdraw.plugins.Plugin;

public class MyFirstMaginDrawPlugin extends Plugin {

	public boolean close() {
		return true;
	}

	public void init() {
		//at MagicDraw's startup, pop-up a window.
		javax.swing.JOptionPane.showMessageDialog(null, "My Plugin init");
	}

	public boolean isSupported() {
		return true;
	}

}
