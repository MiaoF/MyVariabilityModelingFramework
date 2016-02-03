package eu.miaofang.md.plugin.model.component;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map.Entry;

import javax.swing.JOptionPane;

import eu.miaofang.md.plugin.model.SelectedFeature;
import eu.miaofang.md.plugin.model.WmsModel;
import eu.miaofang.wms.plugin.config.FeatureToComponentMapping;
import eu.miaofang.wms.plugin.config.PlugInConstants;

public class CompositeComponent extends WmsComponent{
	protected HashMap<String, Boolean> optionalSubComponents = new HashMap<String, Boolean>();
	protected List<String> commonSubComponents = new ArrayList<String>();
	protected ModelSuite modelSuite = new ModelSuite();
	
	public CompositeComponent() {
		this.componentType = PlugInConstants.COMPOSITE_COMPONENT;
	}
	
	public ArrayList<WmsComponent> getChildComponents() {
		 ArrayList<WmsComponent> childComponents = new ArrayList<WmsComponent>();
		for(Entry<String, Boolean> entry : optionalSubComponents.entrySet()) {
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
		if(optionalSubComponents.containsKey(feature.getFeatureName())) {
			optionalSubComponents.put(feature.getFeatureName(), true);
			return true;
		}
		return false;
	}
	
	public ArrayList<WmsModel> getContainedModels() {
		return this.modelSuite.getContainedModels();
	}

	public void setContainedModels(ArrayList<WmsModel> containedModels) {
		this.modelSuite.setContainedModels(containedModels);
	}

	public void addNewModelType(WmsModel warehouseModelType) {
		this.modelSuite.add(warehouseModelType);
	}
}
