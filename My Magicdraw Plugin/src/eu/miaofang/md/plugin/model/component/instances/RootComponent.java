package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;

public class RootComponent extends WmsCompositeComponent{
	
	public RootComponent() {
		super();
		setComponentName("SiemensWMS");
		setComponentType(PlugInConstants.MULTI_VARIABLE_COMPONENT);
		modelSuite.setModelPakcageName(this.getComponentName());
		addNewModelType(new WmsModelType("System Component", PlugInConstants.COMPONENT_MODEL, "WmsComponents"));
		addNewModelType(new WmsModelType("Selected Features", PlugInConstants.SELECTED_FEATURE_MODEL));
		
		optionalSubComponents.put(PlugInConstants.AKL_FEATURE_NAME, false);
		optionalSubComponents.put(PlugInConstants.BKS_FEATURE_NAME, false);
		optionalSubComponents.put(PlugInConstants.MPL_FEATURE_NAME, false);
		optionalSubComponents.put(PlugInConstants.INVENTORY_FEATURE_NAME, false);
		
	}	
}
