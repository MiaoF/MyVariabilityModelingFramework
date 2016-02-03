package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.WmsModel;
import eu.miaofang.md.plugin.model.component.CompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;


public class InventoryComponent extends CompositeComponent{
	
	public InventoryComponent() {
		super();
		setComponentName("Inventory");
		modelSuite.setModelPakcageName(this.getComponentName());
		setComponentType(PlugInConstants.BEHAVIOR_VARIABLE_COMPONENT);
		addNewModelType(new WmsModel("InventoryProcesses", PlugInConstants.PROCESS_MODEL));
		
		this.isProcessVariabile = true;
		
	}
}
