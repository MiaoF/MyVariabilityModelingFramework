package impl.plugin.md;


import com.nomagic.actions.ActionsCategory;
import com.nomagic.magicdraw.actions.ActionsConfiguratorsManager;
import com.nomagic.magicdraw.core.Application;
import com.nomagic.magicdraw.plugins.Plugin;

public class SiemensWmsUmlGenerator extends Plugin {

	@Override
	public boolean close() {
		return false;
	}

	@Override
	public void init() {
		try {
			ActionsCategory category = new ActionsCategory(null, null);
			category.addAction(new GeneratorBrowserAction());
			ActionsConfiguratorsManager manager = ActionsConfiguratorsManager.getInstance();
			manager.addContainmentBrowserContextConfigurator(new ModelExportContextConfigurator(category));
			Application.getInstance()
				.getGUILog().log("[Generator Plugin] Loading OK");
		} catch (Exception e) {
			Application
					.getInstance()
					.getGUILog()
					.log("Generator Plugin Could not be instantiated : "
							+ e.toString());
		}
	}

	@Override
	public boolean isSupported() {
		return false;
	}

}
