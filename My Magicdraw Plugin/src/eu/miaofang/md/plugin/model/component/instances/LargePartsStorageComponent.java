package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;


public class LargePartsStorageComponent extends WmsCompositeComponent{
	
	public LargePartsStorageComponent() {
		super();
		setComponentName("MPL");
		setComponentType(PlugInConstants.MULTI_VARIABLE_COMPONENT);
		modelSuite.setModelPakcageName(this.getComponentName());
		addNewModelType(new WmsModelType("MPL-TopologyModel", PlugInConstants.TOPOLOGY_MODEL));
		addNewModelType(new WmsModelType("MPL-OrderProcessing", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModelType("MPL-ReleasePartialOrder", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModelType("MPL-ReleaseOrder", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModelType("MPL-Components", PlugInConstants.COMPONENT_MODEL, "WmsCompositeComponent"));
		
		optionalSubComponents.put(PlugInConstants.MPL_HEAVYLOAD_STAPLER_FEATURE_NAME, false);
		optionalSubComponents.put(PlugInConstants.MPL_DISTRIBUTION_STAPlER_FEATURE_NAME, false);
		optionalSubComponents.put(PlugInConstants.MPL_PICKING_STAPLER_FEATURE_NAME, false);
	}
}
