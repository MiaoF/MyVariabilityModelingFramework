package eu.miaofang.md.plugin;

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
			category.addAction(new FeatureModelBrowserAction());
			ActionsConfiguratorsManager manager = ActionsConfiguratorsManager.getInstance();
			manager.addContainmentBrowserContextConfigurator(new BrowserContextConfigurator(category));
			Application.getInstance()
				.getGUILog().log("[Feature Editor Plugin] Loading OK");
		} catch (Exception e) {
			Application
					.getInstance()
					.getGUILog()
					.log("FeatureEditor Could not be instantiated : "
							+ e.toString());
		}
	}

	@Override
	public boolean isSupported() {
		return false;
	}

}
