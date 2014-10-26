package eu.miaofang.md.plugin.control.builder;

import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.openapi.uml.ModelElementsManager;
import com.nomagic.magicdraw.openapi.uml.ReadOnlyElementException;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Namespace;

import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;

public class DiagramBuilder implements IDiagramBuilder {

	Diagram diagram; 
	WmsModelType diagramType;
	Namespace namespace;

	public DiagramBuilder(WmsModelType modelType, Namespace theOwnerPackage) {
		try {
			diagram = ModelElementsManager.getInstance().createDiagram(
					modelType.getModelType(), theOwnerPackage);
			diagram.setName(modelType.getModelName());
			diagramType = modelType;
			namespace = theOwnerPackage;
			// initialzeDiagram(modelType, diagram, theComponent);
		} catch (ReadOnlyElementException e) {
			e.printStackTrace();
		}
	}
	
	public Diagram getDiagram() {
		return diagram;
	}

	public void addVariableElments(WmsModelType modelType,
			Diagram targetDiagram, WmsCompositeComponent theComponent) {
		
	}

	public void addInvariableElments() {
		
	}

	public void addVariableElments() {
		// TODO Auto-generated method stub
		
	}

	public void addInvariableElments(WmsCompositeComponent theComponent,
			Project activeProject) {
		// TODO Auto-generated method stub
		
	}

	public void addVariableElments(WmsCompositeComponent theComponent,
			Project activeProject) {
		// TODO Auto-generated method stub
		
	}

}
