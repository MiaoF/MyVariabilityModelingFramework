package impl;
import impl.plugin.md.SiemensWmsUmlGenerator;

import com.nomagic.magicdraw.core.Application;
import com.nomagic.runtime.ApplicationExitedException;

import eu.miaofang.md.plugin.FeatureEditor;

public class Debug {
	public static void main(String[] args) {
		try {
			FeatureEditor plugin = new FeatureEditor();
			SiemensWmsUmlGenerator generator = new SiemensWmsUmlGenerator();
			Application app = Application.getInstance();
			app.start(true, false, false, args, null);
			plugin.init();
			generator.init();
		} catch (ApplicationExitedException e) {
			e.printStackTrace();
			
		}
	}
}
