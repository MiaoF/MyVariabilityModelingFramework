package eu.miaofang.md.plugin.model.component;

import java.util.ArrayList;

import eu.miaofang.md.plugin.model.WmsModel;

public class ModelSuite {
	protected String modelPakcageName = "";
	protected ArrayList<WmsModel> containedModels = new ArrayList<WmsModel>();

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

	public ArrayList<WmsModel> getContainedModels() {
		return containedModels;
	}

	public void setContainedModels(ArrayList<WmsModel> containedModels) {
		this.containedModels = containedModels;
	}

	public void add(WmsModel warehouseModelType) {
		this.containedModels.add(warehouseModelType);
	}

}
