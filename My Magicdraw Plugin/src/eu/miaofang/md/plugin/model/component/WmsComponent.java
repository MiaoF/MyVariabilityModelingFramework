package eu.miaofang.md.plugin.model.component;

import java.util.ArrayList;
import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.wms.plugin.config.PlugInConstants;

public class WmsComponent {
	protected String componentName = "";
	protected String componentType = PlugInConstants.ATOMIC_COMPONENT;
	protected WmsComponent parentComponent = null;
	protected ModelSuite modelSuite = new ModelSuite();
	
	public WmsComponent() {

	}
	public WmsComponent(String name) {
		componentName = name;
	}
	
	public String getComponentName() {
		return componentName;
	}

	public void setComponentName(String componentName) {
		this.componentName = componentName;
	}

	public WmsComponent getParentComponent() {
		return parentComponent;
	}

	public void setParentComponent(WmsComponent parentComponent) {
		this.parentComponent = parentComponent;
	}

	public ArrayList<WmsModelType> getContainedModels() {
		return this.modelSuite.getContainedModels();
	}

	public void setContainedModels(ArrayList<WmsModelType> containedModels) {
		this.modelSuite.setContainedModels(containedModels);
	}

	public void addNewModelType(WmsModelType warehouseModelType) {
		this.modelSuite.add(warehouseModelType);
	}

	public String getComponentType() {
		return componentType;
	}

	public void setComponentType(String componentType) {
		this.componentType = componentType;
	}

	public Object getModelStructure() {
		// TODO Auto-generated method stub
		return null;
	}

}
