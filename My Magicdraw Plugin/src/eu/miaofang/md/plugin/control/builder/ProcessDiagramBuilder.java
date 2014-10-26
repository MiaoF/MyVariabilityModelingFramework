package eu.miaofang.md.plugin.control.builder;

import com.nomagic.uml2.ext.magicdraw.activities.mdfundamentalactivities.Activity;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Namespace;
import eu.miaofang.md.plugin.model.WmsModelType;


public class ProcessDiagramBuilder extends DiagramBuilder {

	Activity context;
	
	public ProcessDiagramBuilder(WmsModelType modelType,
			Namespace theOwnerPackage) {
		super(modelType, theOwnerPackage);
	}
	

	public void addVariableElments() {
		
	}

	public void addInvariableElments() {
		
	}

}
