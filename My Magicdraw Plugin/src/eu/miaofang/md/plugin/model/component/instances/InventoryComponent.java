package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;


public class InventoryComponent extends WmsCompositeComponent{
	
	public InventoryComponent() {
		super();
		setComponentName("Inventory");
		modelSuite.setModelPakcageName(this.getComponentName());
		setComponentType(PlugInConstants.BEHAVIOR_VARIABLE_COMPONENT);
		addNewModelType(new WmsModelType("InventoryProcesses", PlugInConstants.PROCESS_MODEL));
	}
}
