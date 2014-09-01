package magicdraw.feature.editor;

import magicdraw.feature.editor.action.FeatureModelBrowserAction;
import myplugin.BrowserContextConfigurator;
import myplugin.SimplePluginAction;

import com.nomagic.actions.ActionsCategory;
import com.nomagic.magicdraw.actions.ActionsConfiguratorsManager;
import com.nomagic.magicdraw.core.Application;
import com.nomagic.magicdraw.plugins.Plugin;

public class FeatureEditor extends Plugin {

	@Override
	public boolean close() {
		return false;
	}

	@Override
	public void init() {
		 try {
	            ActionsCategory category = new ActionsCategory(null, null);
	            category.addAction(new SimplePluginAction());
	            category.addAction(new FeatureModelBrowserAction());
	            ActionsConfiguratorsManager manager = ActionsConfiguratorsManager.getInstance();
	            manager.addContainmentBrowserContextConfigurator(new BrowserContextConfigurator(category));
	            Application.getInstance().getGUILog().log("[Simple Plugin] Loading OK");
	            javax.swing.JOptionPane.showMessageDialog(null, "Feature Editor init");
	        }
	        catch (Exception e) {
	            Application.getInstance().getGUILog().log(
	                    "FeatureEditor Could not be instantiated : " + e.toString());
	        }

	}

	@Override
	public boolean isSupported() {
		return false;
	}

}