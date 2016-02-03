package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.WmsModel;
import eu.miaofang.md.plugin.model.component.CompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;


public class LargePartsStorageComponent extends CompositeComponent{
	
	public LargePartsStorageComponent() {
		super();
		setComponentName("MPL");
		modelSuite.setModelPakcageName(this.getComponentName());
		addNewModelType(new WmsModel("MPL-TopologyModel", PlugInConstants.TOPOLOGY_MODEL));
		addNewModelType(new WmsModel("MPL-OrderProcessing", PlugInConstants.PROCESS_MODEL, "MplOrderProcessing-Template"));
		addNewModelType(new WmsModel("MPL-ReleasePartialOrder", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("MPL-ReleaseOrder", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("MPL-RetrieveOrder", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("MPL-GoodsIn", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("MPL-Components", PlugInConstants.COMPONENT_MODEL, "WmsCompositeComponent"));
		this.isProcessVariabile = true;
		this.isTopologyVariabile = true;
		optionalSubComponents.put(PlugInConstants.MPL_HEAVYLOAD_STAPLER_FEATURE_NAME, false);
		optionalSubComponents.put(PlugInConstants.MPL_DISTRIBUTION_STAPlER_FEATURE_NAME, false);
		optionalSubComponents.put(PlugInConstants.MPL_PICKING_STAPLER_FEATURE_NAME, false);
	}
}
