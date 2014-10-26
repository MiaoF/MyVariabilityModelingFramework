package eu.miaofang.md.plugin.model.component;

import java.util.ArrayList;

import eu.miaofang.md.plugin.model.WmsModelType;

public class ModelSuite {
	protected String modelPakcageName = "";

	public ModelSuite() {
		
	}
	
	public ModelSuite(String name) {
		this.modelPakcageName = name;
	}

	public String getModelPakcageName() {
		return modelPakcageName;
	}

	public void setModelPakcageName(String modelPakcageName) {
		this.modelPakcageName = modelPakcageName;
	}

	protected ArrayList<WmsModelType> containedModels = new ArrayList<WmsModelType>();

	public ArrayList<WmsModelType> getContainedModels() {
		return containedModels;
	}

	public void setContainedModels(ArrayList<WmsModelType> containedModels) {
		this.containedModels = containedModels;
	}

	public void add(WmsModelType warehouseModelType) {
		this.containedModels.add(warehouseModelType);
	}

}
