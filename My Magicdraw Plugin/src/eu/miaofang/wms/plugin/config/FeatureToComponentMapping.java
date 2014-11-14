package eu.miaofang.wms.plugin.config;

import java.util.HashMap;

import eu.miaofang.md.plugin.model.component.WmsComponent;
import eu.miaofang.md.plugin.model.component.instances.AutomaticSmallPartsStorageComponent;
import eu.miaofang.md.plugin.model.component.instances.AutomaticTransportComponent;
import eu.miaofang.md.plugin.model.component.instances.InventoryComponent;
import eu.miaofang.md.plugin.model.component.instances.LargePartsStorageComponent;
import eu.miaofang.md.plugin.model.component.instances.PickByLightStorageComponent;

public class FeatureToComponentMapping {
	private static FeatureToComponentMapping instance = null;
	private static final HashMap<String, WmsComponent> featureToComponent = new HashMap<String, WmsComponent>();
	
	private FeatureToComponentMapping() {
		featureToComponent.put(PlugInConstants.AKL_FEATURE_NAME, new AutomaticSmallPartsStorageComponent());
		featureToComponent.put(PlugInConstants.MPL_FEATURE_NAME, new LargePartsStorageComponent());
		featureToComponent.put(PlugInConstants.BKS_FEATURE_NAME, new PickByLightStorageComponent());
		featureToComponent.put(PlugInConstants.MPL_PICKING_STAPLER_FEATURE_NAME, new WmsComponent());
		featureToComponent.put(PlugInConstants.MPL_DISTRIBUTION_STAPlER_FEATURE_NAME, new WmsComponent());
		featureToComponent.put(PlugInConstants.MPL_HEAVYLOAD_STAPLER_FEATURE_NAME, new WmsComponent());
		featureToComponent.put(PlugInConstants.MPL_HEAVYLOAD_STAPLER_FEATURE_NAME, new WmsComponent());
		featureToComponent.put(PlugInConstants.INVENTORY_FEATURE_NAME, new InventoryComponent());
		featureToComponent.put("AutomaticSmallPartsStorage", new AutomaticTransportComponent());
		featureToComponent.put(PlugInConstants.SQL_SERVER_FEATURE_NAME, new WmsComponent(PlugInConstants.SQL_SERVER_FEATURE_NAME));
		featureToComponent.put(PlugInConstants.ORACLE_FEATURE_NAME, new WmsComponent(PlugInConstants.ORACLE_FEATURE_NAME));
	}

	
	public static FeatureToComponentMapping getInstance() {
        if (instance == null) {
            instance = new FeatureToComponentMapping();
        }
        return instance;
    }
	public HashMap<String, WmsComponent> getFeatureToModelStructure() {
		return featureToComponent;
	}
}
