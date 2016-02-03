package eu.miaofang.md.plugin.model.component.instances;


import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map.Entry;

import javax.swing.JOptionPane;

import eu.miaofang.md.plugin.model.SelectedFeature;
import eu.miaofang.md.plugin.model.WmsModel;
import eu.miaofang.md.plugin.model.component.WmsComponent;
import eu.miaofang.md.plugin.model.component.CompositeComponent;
import eu.miaofang.wms.plugin.config.FeatureToComponentMapping;
import eu.miaofang.wms.plugin.config.PlugInConstants;

public class RootComponent extends CompositeComponent{
	
	protected HashMap<String, Boolean> optionalBusinessComponents = new HashMap<String, Boolean>();
	protected HashMap<String, Boolean> optionalPlatformComponents = new HashMap<String, Boolean>();
		
	public RootComponent() {
		super();
		setComponentName("SiemensWMS");
		setComponentType(PlugInConstants.COMPOSITE_COMPONENT);
		modelSuite.setModelPakcageName(this.getComponentName());
		addNewModelType(new WmsModel("System Component", PlugInConstants.COMPONENT_MODEL, "WmsComponents"));
		addNewModelType(new WmsModel("Selected Features", PlugInConstants.SELECTED_FEATURE_MODEL));
		
		optionalBusinessComponents.put(PlugInConstants.AKL_FEATURE_NAME, false);
		optionalBusinessComponents.put(PlugInConstants.BKS_FEATURE_NAME, false);
		optionalBusinessComponents.put(PlugInConstants.MPL_FEATURE_NAME, false);
		optionalBusinessComponents.put(PlugInConstants.INVENTORY_FEATURE_NAME, false);
		optionalPlatformComponents.put(PlugInConstants.SQL_SERVER_FEATURE_NAME, false);
		optionalPlatformComponents.put(PlugInConstants.ORACLE_FEATURE_NAME, false);
	}
	
	
	public ArrayList<WmsComponent> getChildComponents() {
		ArrayList<WmsComponent>  childComponents = new ArrayList<WmsComponent>();
		for(Entry<String, Boolean> entry : optionalPlatformComponents.entrySet()) {
		    String key = entry.getKey().trim();
		    boolean selected = entry.getValue();
		    if(selected) {
		    	//put the component in the parent component
		    	if(FeatureToComponentMapping.getInstance().getFeatureToModelStructure().containsKey(key)) {
		    		childComponents.add(FeatureToComponentMapping.getInstance().getFeatureToModelStructure().get(key));	
		    	}
		    }
		}
		for(Entry<String, Boolean> entry : optionalBusinessComponents.entrySet()) {
		    String key = entry.getKey().trim();
		    boolean selected = entry.getValue();
		    if(selected) {
		    	//put the component in the parent component
		    	if(FeatureToComponentMapping.getInstance().getFeatureToModelStructure().containsKey(key)) {
		    		childComponents.add(FeatureToComponentMapping.getInstance().getFeatureToModelStructure().get(key));	
		    	}
		    }
		}
		return childComponents;
	}

	public boolean hasOptionalComponentByFeatureName(SelectedFeature feature) {
		if(optionalBusinessComponents.containsKey(feature.getFeatureName())) {
			optionalBusinessComponents.put(feature.getFeatureName(), true);
			return true;
		}
		
		if(optionalPlatformComponents.containsKey(feature.getFeatureName())) {
			optionalPlatformComponents.put(feature.getFeatureName(), true);
			return true;
		}
		return false;
	}
	
	public ArrayList<WmsComponent> getBusinessComponents() {
		ArrayList<WmsComponent> childComponents = new ArrayList<WmsComponent>();
		for(Entry<String, Boolean> entry : optionalBusinessComponents.entrySet()) {
		    String key = entry.getKey().trim();
		    boolean selected = entry.getValue();
		    if(selected) {
		    	//put the component in the parent component
		    	if(FeatureToComponentMapping.getInstance().getFeatureToModelStructure().containsKey(key)) {
		    		childComponents.add(FeatureToComponentMapping.getInstance().getFeatureToModelStructure().get(key));	
		    	}
		    }
		}
		return childComponents;
	}


	public ArrayList<WmsComponent> getPlatformComponents() {
		ArrayList<WmsComponent> childComponents = new ArrayList<WmsComponent>();
		for(Entry<String, Boolean> entry : optionalPlatformComponents.entrySet()) {
		    String key = entry.getKey().trim();
		    boolean selected = entry.getValue();
		    if(selected) {
		    	//put the component in the parent component
		    	if(FeatureToComponentMapping.getInstance().getFeatureToModelStructure().containsKey(key)) {
		    		childComponents.add(FeatureToComponentMapping.getInstance().getFeatureToModelStructure().get(key));	
		    	}
		    }
		}
		return childComponents;
	}
	
}
