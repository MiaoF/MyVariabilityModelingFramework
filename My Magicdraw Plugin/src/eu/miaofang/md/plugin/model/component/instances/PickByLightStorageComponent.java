package eu.miaofang.md.plugin.model.component.instances;

import eu.miaofang.md.plugin.model.WmsModel;
import eu.miaofang.md.plugin.model.component.CompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;


public class PickByLightStorageComponent extends CompositeComponent{
	
	public PickByLightStorageComponent() {
		super();
		setComponentName("BKS");
		modelSuite.setModelPakcageName(this.getComponentName());
		addNewModelType(new WmsModel("BKS-OrderProcessing", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("BKS-RequestMaterials", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("BKS-Supply", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("BKS-trackContainer", PlugInConstants.PROCESS_MODEL));
		addNewModelType(new WmsModel("BKS-Component", PlugInConstants.COMPONENT_MODEL, "WmsCompositeComponent"));
		this.isProcessVariabile = true;
		this.isTopologyVariabile = true;
		
	}
}
