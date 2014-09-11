package magicdraw.feature.models.details;


public class LargePartsStoragePackage extends ModelPackage{
	
	public LargePartsStoragePackage() {
		super();
		setPackageName("LargePartsStoragePackage-MPL");
		addNewModelType(new WarehouseModelType("MPL-TopologyModel", "WmsTopology"));
		addNewModelType(new WarehouseModelType("MPL-OrderProcessing", "WmsProcess"));
		addNewModelType(new WarehouseModelType("MPL-ReleasePartialOrder", "WmsProcess"));
		addNewModelType(new WarehouseModelType("MPL-ReleaseOrder", "WmsProcess"));
	}
}
