package eu.miaofang.md.plugin.control.builder;

import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.openapi.uml.ModelElementsManager;
import com.nomagic.magicdraw.openapi.uml.ReadOnlyElementException;
import com.nomagic.magicdraw.openapi.uml.SessionManager;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Namespace;

import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;

public class DiagramBuilder {

	Diagram diagram; 
	WmsModelType diagramType;
	Namespace namespace;

	public DiagramBuilder(WmsModelType modelType, Namespace theOwnerPackage, WmsCompositeComponent theComponent, Project activeProject) {
		try {
			SessionManager.getInstance().createSession("Create New Diagram");
			diagram = ModelElementsManager.getInstance().createDiagram(
					modelType.getModelType(), theOwnerPackage);
			diagram.setName(modelType.getModelName());
			diagramType = modelType;
			namespace = theOwnerPackage;
			addInvariableElments(theComponent, activeProject);
			addVariableElments(theComponent, activeProject);
			SessionManager.getInstance().closeSession();
			// initialzeDiagram(modelType, diagram, theComponent);
		} catch (ReadOnlyElementException e) {
			e.printStackTrace();
		}
	}
	
	public Diagram getDiagram() {
		return diagram;
	}
	
	protected void addVariableElments(WmsCompositeComponent theComponent,
			Project activeProject) {
		
	}
	protected void addInvariableElments(WmsCompositeComponent theComponent,
			Project activeProject) {
		
	}

}
