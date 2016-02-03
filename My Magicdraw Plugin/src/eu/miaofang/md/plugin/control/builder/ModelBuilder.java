package eu.miaofang.md.plugin.control.builder;

import java.util.Collection;
import java.util.Iterator;
import java.util.List;

import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.openapi.uml.ModelElementsManager;
import com.nomagic.magicdraw.openapi.uml.ReadOnlyElementException;
import com.nomagic.magicdraw.openapi.uml.SessionManager;
import com.nomagic.magicdraw.uml.symbols.DiagramPresentationElement;
import com.nomagic.magicdraw.uml.symbols.PresentationElement;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Namespace;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.PackageableElement;

import eu.miaofang.md.plugin.model.WmsModel;
import eu.miaofang.md.plugin.model.component.CompositeComponent;

public class ModelBuilder {

	Diagram diagram; 
	WmsModel diagramType;
	Namespace namespace;

	public ModelBuilder(WmsModel modelType, Namespace theOwnerPackage, CompositeComponent theComponent, Project activeProject) {
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
	
	protected void addVariableElments(CompositeComponent theComponent,
			Project activeProject) {
		
	}
	protected void addInvariableElments(CompositeComponent theComponent,
			Project activeProject) {
		
	}

}
