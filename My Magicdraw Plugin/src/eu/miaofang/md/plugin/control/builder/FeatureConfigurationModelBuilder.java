package eu.miaofang.md.plugin.control.builder;

import com.nomagic.magicdraw.core.Project;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Namespace;

import eu.miaofang.md.plugin.model.WmsModel;
import eu.miaofang.md.plugin.model.component.CompositeComponent;


public class FeatureConfigurationModelBuilder extends ModelBuilder {

	public FeatureConfigurationModelBuilder(WmsModel modelType,
			Namespace theOwnerPackage, CompositeComponent theComponent, Project activeProject) {
		super(modelType, theOwnerPackage, theComponent, activeProject);
	}

	public void addVariableElments() {
		
	}

	public void addInvariableElments() {
		
	}

}
