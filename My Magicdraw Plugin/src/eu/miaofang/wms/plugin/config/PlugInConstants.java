package eu.miaofang.wms.plugin.config;

public class PlugInConstants {

	// Component types mapping to MagicDraw Model
	public static final String ATOMIC_COMPONENT = "AtomicComponent";
//	public static final String COMPOSITE_COMPONENT = "MultiVariableComponent";
	public static final String COMPOSITE_COMPONENT = "CompositeComponent";	
	
	public static final String VARIABLE_COMPONENT = "VariableComponent";
	public static final String INVARIABLE_COMPONENT = "InvariableComponent";
	public static final String TOPOLOGY_VARIABLE_COMPONENT = "TopologyVariableComponent";
	public static final String BEHAVIOR_VARIABLE_COMPONENT = "ProcessVariableComponent";
	

	// Profile types mapping to MagicDraw Meta Model
	public static final String COMPONENTE_PROFILE = "ComponentMetaModel";
	public static final String FEATURE_PROFILE = "FeatureMetaModel";

	// Model types in MagicDraw
	public static final String PROCESS_MODEL = "WmsProcess";
	public static final String TOPOLOGY_MODEL = "WmsTopology";
	public static final String COMPONENT_MODEL = "WmsComponents";
	public static final String SELECTED_FEATURE_MODEL = "WmsFeatureConfiguration";

	// Feature names in MagicDraw WMS Feature Model
	public static final String AKL_FEATURE_NAME = "SmallPartsStorage";
	public static final String BKS_FEATURE_NAME = "PickByLightStorage";
	public static final String MPL_FEATURE_NAME = "LargePartsStorage";
	public static final String MPL_PICKING_STAPLER_FEATURE_NAME = "PickingStapler";
	public static final String MPL_DISTRIBUTION_STAPlER_FEATURE_NAME = "DistributionStapler";
	public static final String MPL_HEAVYLOAD_STAPLER_FEATURE_NAME = "HeavyLoadStapler";
	public static final String INVENTORY_FEATURE_NAME = "Inventory";
	public static final String AKL_ORDER_PROCESSING_FEATURE_NAME = "AKL_ORDER_PROCESSING";
	public static final String MPL_ORDER_PROCESSING_FEATURE_NAME = "MPL_ORDER_PROCESSING";
	public static final String ORACLE_FEATURE_NAME = "Oracle";
	public static final String SQL_SERVER_FEATURE_NAME = "SqlServer";

	private PlugInConstants() {

	}

}
