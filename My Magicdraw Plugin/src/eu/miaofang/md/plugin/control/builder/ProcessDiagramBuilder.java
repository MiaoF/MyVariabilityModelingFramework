package eu.miaofang.md.plugin.control.builder;

import com.nomagic.magicdraw.core.Project;
import com.nomagic.uml2.ext.magicdraw.activities.mdfundamentalactivities.Activity;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Namespace;

import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;


public class ProcessDiagramBuilder extends DiagramBuilder {

	Activity context;
	
	public ProcessDiagramBuilder(WmsModelType modelType,
			Namespace theOwnerPackage, WmsCompositeComponent theComponent, Project activeProject) {
		super(modelType, theOwnerPackage, theComponent, activeProject);
	}
	

	public void addVariableElments() {
		
	}

	public void addInvariableElments() {
		
	}

}
