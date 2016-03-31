package eu.miaofang.md.plugin.report;

import com.nomagic.actions.ActionsCategory;
import com.nomagic.magicdraw.actions.ActionsConfiguratorsManager;
import com.nomagic.magicdraw.core.Application;
import com.nomagic.magicdraw.plugins.Plugin;

import eu.miaofang.md.plugin.BrowserContextConfigurator;

public class ModelStatusReporter extends Plugin {

	@Override
	public boolean close() {
		return false;
	}

	@Override
	public void init() {
		try {
			ActionsCategory category = new ActionsCategory(null, null);
			category.addAction(new ShowReportAction());
			ActionsConfiguratorsManager manager = ActionsConfiguratorsManager.getInstance();
			manager.addContainmentBrowserContextConfigurator(new AnotherBrowserContextConfigurator(category));
			Application.getInstance()
				.getGUILog().log("[Show Report] Loading OK");
		} catch (Exception e) {
			Application
					.getInstance()
					.getGUILog()
					.log("Show Report View Could not be instantiated : "
							+ e.toString());
		}
	}

	@Override
	public boolean isSupported() {
		return false;
	}

}
