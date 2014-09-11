package magicdraw.feature.models.details;


public class PickByLightStoragePackage extends ModelPackage{
	
	public PickByLightStoragePackage() {
		super();
		setPackageName("PickByLight-BKS");
		addNewModelType(new WarehouseModelType("BKS-OrderProcessing", "WmsProcess"));
	}
}
