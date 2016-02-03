package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.WmsModel;
import eu.miaofang.md.plugin.model.component.CompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;


public class AutomaticSmallPartsStorageComponent extends CompositeComponent{
	
	public AutomaticSmallPartsStorageComponent() {
		super();
		setComponentName("AKL");
		modelSuite.setModelPakcageName(this.getComponentName());
		addNewModelType(new WmsModel("AKL-TopologyModel", PlugInConstants.TOPOLOGY_MODEL));
		addNewModelType(new WmsModel("AKL-OrderProcessing", PlugInConstants.PROCESS_MODEL, "AklOrderProcessing-Template"));
		addNewModelType(new WmsModel("AKL-GoodsIn-CreateBox", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("AKL-Relocate", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("AKL-AmendAmount", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("AKL-HandleEmptyBin", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("AKL-RequestMaterials", PlugInConstants.PROCESS_MODEL, "Akl-RequestMaterials-Template"));
		addNewModelType(new WmsModel("AKL-RequestTablar", PlugInConstants.PROCESS_MODEL, "Akl-RequestTablar-Template"));
		addNewModelType(new WmsModel("AKL-Components", PlugInConstants.COMPONENT_MODEL, "WmsCompositeComponent"));
		this.isProcessVariabile = true;
		this.isTopologyVariabile = true;
		
		optionalSubComponents.put("AutomaticSmallPartsStorage", false);
	}
}
