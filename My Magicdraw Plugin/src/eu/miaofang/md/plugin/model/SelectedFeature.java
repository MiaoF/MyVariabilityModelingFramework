package eu.miaofang.md.plugin.model;

import java.util.ArrayList;
import java.util.List;

public class SelectedFeature {
	private String featureName = "";
	private SelectedFeature parentFeature = null;
	private List<SelectedFeature> childrenFeatures = new ArrayList<SelectedFeature>();
	
	public SelectedFeature(String name) {
		featureName = name;
	}
	
	public String getFeatureName() {
		return featureName;
	}
	public void setFeatureName(String featureName) {
		this.featureName = featureName;
	}
	public SelectedFeature getParentFeature() {
		return parentFeature;
	}
	public void setParentFeature(SelectedFeature parentFeature) {
		this.parentFeature = parentFeature;
	}
	public List<SelectedFeature> getChildrenFeatures() {
		return childrenFeatures;
	}
	public void setChildrenFeatures(List<SelectedFeature> childrenFeatures) {
		this.childrenFeatures = childrenFeatures;
	}
	public void addChildFeature(SelectedFeature newSubfeature) {
		this.childrenFeatures.add(newSubfeature);
		newSubfeature.setParentFeature(this);
	}
	public boolean hasSubFeature(String subFeatureName) {
		for(SelectedFeature subFeature: this.childrenFeatures) {
			if(subFeature.getFeatureName().equalsIgnoreCase(subFeatureName));
				return true;
		}
		return false;
	}

	public SelectedFeature getChildFeatureByName(String subFeatureName) {
		for(SelectedFeature subFeature: this.childrenFeatures) {
			if(subFeature.getFeatureName().equalsIgnoreCase(subFeatureName));
				return subFeature;
		}
		return null;
	}

}
