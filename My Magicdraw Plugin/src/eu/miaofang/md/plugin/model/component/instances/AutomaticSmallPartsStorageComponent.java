package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;


public class AutomaticSmallPartsStorageComponent extends WmsCompositeComponent{
	
	public AutomaticSmallPartsStorageComponent() {
		super();
		setComponentName("AKL");
		setComponentType(PlugInConstants.MULTI_VARIABLE_COMPONENT);
		modelSuite.setModelPakcageName(this.getComponentName());
		addNewModelType(new WmsModelType("AKL-TopologyModel", PlugInConstants.TOPOLOGY_MODEL));
		addNewModelType(new WmsModelType("AKL-OrderProcessing", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModelType("AKL-GoodsIn-CreateBox", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModelType("AKL-Components", PlugInConstants.COMPONENT_MODEL, "WmsCompositeComponent"));
		
		optionalSubComponents.put(PlugInConstants.AKL_ORDER_PROCESSING_FEATURE_NAME, false);
	}
}
