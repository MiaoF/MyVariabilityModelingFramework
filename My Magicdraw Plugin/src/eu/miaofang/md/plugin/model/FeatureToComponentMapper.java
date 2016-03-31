
package eu.miaofang.md.plugin.model;

import java.util.ArrayList;
import java.util.List;

import javax.swing.JOptionPane;
import javax.swing.tree.TreePath;

import eu.miaofang.md.plugin.model.component.WmsComponent;
import eu.miaofang.md.plugin.model.component.CompositeComponent;
import eu.miaofang.md.plugin.model.component.instances.RootComponent;
import eu.miaofang.md.plugin.view.FeatureTreeNode;
import eu.miaofang.wms.plugin.config.PlugInConstants;

public class FeatureToComponentMapper {

	private RootComponent rootComponent;
	private final List<SelectedFeature> allSelectedFeatures = new ArrayList<SelectedFeature>();
	private final List<SelectedFeature> nonMatchingFeatures = new ArrayList<SelectedFeature>();

	public FeatureToComponentMapper(TreePath[] checkedPaths) {
		for (int i = 0; i < checkedPaths.length; i++) {
			SelectedFeature aFeature = new SelectedFeature(checkedPaths[i].getLastPathComponent().toString());
			allSelectedFeatures.add(aFeature);
		}
		rootComponent = new RootComponent();
		for(SelectedFeature fe: allSelectedFeatures) {
			bindComponentVariability(fe);
		}
		for(SelectedFeature fe: allSelectedFeatures) {
			ArrayList<WmsComponent> allComponent = rootComponent.getChildComponents();
			for(WmsComponent c: allComponent) {
				if(c.getComponentType().equalsIgnoreCase(PlugInConstants.COMPOSITE_COMPONENT)) {
					bindComponentVariability((CompositeComponent)c, fe);
				}
			}
		}
		
		
	}

	private void bindComponentVariability(SelectedFeature feature) {
		if(!rootComponent.hasOptionalComponentByFeatureName(feature)) {
			nonMatchingFeatures.add(feature);
		}
	}
	
	private void bindComponentVariability(CompositeComponent c, SelectedFeature feature) {
		if(!c.hasOptionalComponentByFeatureName(feature)) {
			nonMatchingFeatures.add(feature);
		}
	}


	public RootComponent getRootComponent() {
		return rootComponent;
	}

	public List<SelectedFeature> getAllSelectedFeatures() {
		return allSelectedFeatures;
	}

	public List<SelectedFeature> getNonMatchingFeatures() {
		return nonMatchingFeatures;
	}
}
