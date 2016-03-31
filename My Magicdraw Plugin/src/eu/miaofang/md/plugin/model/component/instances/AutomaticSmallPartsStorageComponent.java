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
		addNewModelType(new WmsModel("AKL-OrderProcessing", PlugInConstants.PROCESS_MODEL, "Akl-OrderProcessing-Template"));
		addNewModelType(new WmsModel("AKL-ActivatePartialOrder", PlugInConstants.PROCESS_MODEL, "Akl-ActivatePartialOrder-Template"));
		addNewModelType(new WmsModel("Akl-RelocateMaterials", PlugInConstants.PROCESS_MODEL, "Akl-RelocateMaterials-Template"));
		addNewModelType(new WmsModel("AKL-DeliverTablar", PlugInConstants.PROCESS_MODEL, "Akl-DeliverTablar-Template"));
		addNewModelType(new WmsModel("AKL-TransportiereTablarAway", PlugInConstants.PROCESS_MODEL, "Akl-TransportiereTablarAway-Template"));
		addNewModelType(new WmsModel("AKL-RequrestTablar", PlugInConstants.PROCESS_MODEL, "Akl-RequrestTablar-Template"));
		addNewModelType(new WmsModel("AKL-RequrestMaterials", PlugInConstants.PROCESS_MODEL, "Akl-RequrestMaterials-Template"));
		addNewModelType(new WmsModel("AKL-GetNewContainerForGoods", PlugInConstants.PROCESS_MODEL, "Akl-GetNewContainerForGoods-Template"));
			
		addNewModelType(new WmsModel("AKL-AmendAmount", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("AKL-HandleEmptyBin", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("AKL-C-RequestMaterials", PlugInConstants.PROCESS_MODEL, "Akl-C-RequestMaterials-Template"));
		addNewModelType(new WmsModel("AKL-C-RequestTablar", PlugInConstants.PROCESS_MODEL, "Akl-C-RequestTablar-Template"));
		addNewModelType(new WmsModel("AKL-Components", PlugInConstants.COMPONENT_MODEL, "WmsCompositeComponent"));
		addNewModelType(new WmsModel("AKL-GoodsIn-CreateBox", PlugInConstants.PROCESS_MODEL));
		this.isProcessVariabile = true;
		this.isTopologyVariabile = true;
		
		optionalSubComponents.put("AutomaticSmallPartsStorage", false);
	}
}
