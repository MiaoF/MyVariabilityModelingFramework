package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;


public class PickByLightStorageComponent extends WmsCompositeComponent{
	
	public PickByLightStorageComponent() {
		super();
		setComponentName("BKS");
		setComponentType(PlugInConstants.MULTI_VARIABLE_COMPONENT);
		modelSuite.setModelPakcageName(this.getComponentName());
		addNewModelType(new WmsModelType("BKS-OrderProcessing", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModelType("BKS-Component", PlugInConstants.COMPONENT_MODEL, "WmsCompositeComponent"));
	}
}
