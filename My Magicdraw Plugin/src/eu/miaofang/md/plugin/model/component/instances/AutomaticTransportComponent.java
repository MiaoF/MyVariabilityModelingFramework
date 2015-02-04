package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.component.WmsComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;

public class AutomaticTransportComponent extends WmsComponent {
	public AutomaticTransportComponent() {
		super();
		setComponentName("AutomaticTransport");
		modelSuite.setModelPakcageName(this.getComponentName());
		setComponentType(PlugInConstants.TOPOLOGY_VARIABLE_COMPONENT);
	}

}
