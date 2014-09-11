package magicdraw.feature.models.details;


public class AutomaticSmallPartsStoragePackage extends ModelPackage{
	
	public AutomaticSmallPartsStoragePackage() {
		super();
		setPackageName("AutomaticSmallPartsStoragePackage-AKL");
		addNewModelType(new WarehouseModelType("AKL-TopologyModel", "WmsTopology"));
		addNewModelType(new WarehouseModelType("AKL-OrderProcessing", "WmsProcess"));
		addNewModelType(new WarehouseModelType("AKL-GoodsIn-CreateBox", "WmsProcess"));
	}
}
