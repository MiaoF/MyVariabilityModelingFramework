package magicdraw.feature.models.details;

import java.util.ArrayList;

public class ModelPackage {
	
	protected String packageName;
	protected ArrayList<ModelPackage> childPackages = new ArrayList<ModelPackage>();
	protected ArrayList<WarehouseModelType> containedModels = new ArrayList<WarehouseModelType>();
	
	public ModelPackage() {
		
	}
	
	public ArrayList<ModelPackage> getChildPackages() {
		return childPackages;
	}

	public void setChildPackages(ArrayList<ModelPackage> childPackages) {
		this.childPackages = childPackages;
	}

	public String getPackageName() {
		return packageName;
	}

	public void setPackageName(String packageName) {
		this.packageName = packageName;
	}

	public ArrayList<WarehouseModelType> getContainedModels() {
		return containedModels;
	}

	public void setContainedModels(ArrayList<WarehouseModelType> containedModels) {
		this.containedModels = containedModels;
	}
	
	public void addNewModelType(WarehouseModelType warehouseModelType) {
		containedModels.add(warehouseModelType);
	}
	
	public void addNewSubPackage(ModelPackage aPackage) {
		childPackages.add(aPackage);
	}

	
	
}
