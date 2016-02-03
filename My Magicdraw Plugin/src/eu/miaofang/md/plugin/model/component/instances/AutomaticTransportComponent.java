package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.component.CompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;

public class AutomaticTransportComponent extends CompositeComponent {
	public AutomaticTransportComponent() {
		super();
		setComponentName("AutomaticTransport");
		modelSuite.setModelPakcageName(this.getComponentName());
		setComponentType(PlugInConstants.TOPOLOGY_VARIABLE_COMPONENT);

		this.isTopologyVariabile = true;
		
	}
}
