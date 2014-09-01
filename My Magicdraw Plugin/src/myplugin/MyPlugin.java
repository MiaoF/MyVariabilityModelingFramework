package myplugin;

import magicdraw.feature.editor.action.FeatureModelBrowserAction;

import com.nomagic.magicdraw.plugins.Plugin;
import com.nomagic.actions.ActionsCategory;
import com.nomagic.magicdraw.actions.ActionsConfiguratorsManager;
import com.nomagic.magicdraw.core.Application;

/**
 * This class allows you to load your plugin in MagicDraw
 *
 * @author Xavier Seignard
 */
public class MyPlugin extends Plugin {

    // Methods

    /**
     * Plugin initialization and registration of the action in MagicDraw
     */
    public void init() {
        try {
            // We create a new actions category
            ActionsCategory category = new ActionsCategory(null, null);
            
            // We add our plugin action in this newly created category
            category.addAction(new SimplePluginAction());

            category.addAction(new FeatureModelBrowserAction());
            
            // We get the MagicDraw action manager
            ActionsConfiguratorsManager manager = ActionsConfiguratorsManager.getInstance();
            
            // We add our new configuration for our category in the MagicDraw action manager  
            manager.addContainmentBrowserContextConfigurator(new BrowserContextConfigurator(category));
            
            // If everything is OK we log it to the MagicDraw GUI logger
            // Not really clever to put it there, but it's just to show how to log something in MagicDraw
            Application.getInstance().getGUILog().log("[Simple Plugin] Loading OK");
            
            
        }
        catch (Exception e) {
            // If something goes wrong we log it to the MagicDraw GUI logger
            Application.getInstance().getGUILog().log(
                    "[Simple Plugin] Could not instantiate plugin : " + e.toString());
        }

    }

    /**
     * The plugin is always supported, no matter the version of MagicDraw, or the perspective, or what you want. 
     * 
     * @return always true
     */
    public boolean isSupported() {
        return true;
    }

    /**
     * Return true always, because this plugin does not have any close specific actions.
     * 
     * @return always true
     */
    public boolean close() {
        return true;
    }
}
