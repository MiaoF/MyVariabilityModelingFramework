package eu.miaofang.md.plugin.control.builder;

import java.util.Collection;
import java.util.List;

import javax.swing.JOptionPane;

import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.openapi.uml.ModelElementsManager;
import com.nomagic.magicdraw.openapi.uml.PresentationElementsManager;
import com.nomagic.magicdraw.openapi.uml.ReadOnlyElementException;
import com.nomagic.magicdraw.openapi.uml.SessionManager;
import com.nomagic.magicdraw.uml.symbols.DiagramPresentationElement;
import com.nomagic.magicdraw.uml.symbols.PresentationElement;
import com.nomagic.magicdraw.uml.symbols.shapes.ShapeElement;
import com.nomagic.uml2.ext.jmi.helpers.StereotypesHelper;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Namespace;
import com.nomagic.uml2.ext.magicdraw.components.mdbasiccomponents.Component;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Profile;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Stereotype;

import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsComponent;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;

public class ComponentDiagramBuilder {
	Diagram diagram;
	WmsModelType diagramType;
	Namespace namespace;

	public ComponentDiagramBuilder(WmsModelType modelType,
			Namespace theOwnerPackage, WmsCompositeComponent theComponent,
			Project activeProject) {
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

	public void addInvariableElments(WmsCompositeComponent theComponent,
			Project activeProject) {
		Collection<DiagramPresentationElement> allDiagrams = activeProject
				.getDiagrams();
		for (DiagramPresentationElement existingDiagram : allDiagrams) {
			String name = existingDiagram.getHumanName();
			if (name.endsWith(diagramType.getModelTemplate())) {
				existingDiagram.ensureLoaded();
				DiagramPresentationElement diagramPresentationElement = Project
						.getProject(diagram).getDiagram(diagram);
				addPresentationElementsRecursively(
						existingDiagram.getPresentationElements(),
						diagramPresentationElement);
			}
		}
	}

	public void addVariableElments(WmsCompositeComponent theComponent,
			Project activeProject) {
		DiagramPresentationElement diagramPresentationElement = Project
				.getProject(diagram).getDiagram(diagram);
		diagramPresentationElement.ensureLoaded();
		int locationY = 210;
		int locationX = 60;
		for (WmsComponent subComponent : theComponent.getChildComponents()) {
			Component componentElement = activeProject.getElementsFactory()
					.createComponentInstance();
			componentElement.setName(subComponent.getComponentName());
			Profile componentProfile = StereotypesHelper.getProfile(
					activeProject, PlugInConstants.COMPONENTE_PROFILE);
			Stereotype type = StereotypesHelper.getStereotype(activeProject,
					subComponent.getComponentType(), componentProfile);
			if (StereotypesHelper.canApplyStereotype(componentElement, type))
				StereotypesHelper.addStereotype(componentElement, type);

			PresentationElementsManager presentationManager = PresentationElementsManager
					.getInstance();
			try {
				ModelElementsManager.getInstance().addElement(componentElement,
						diagram.getOwner());
				ShapeElement shapeElement = presentationManager
						.createShapeElement(componentElement,
								diagramPresentationElement);
				shapeElement.setLocation(locationX, locationY);
				locationX += 140;
				diagramPresentationElement
						.sAddPresentationElement(shapeElement);
				// StereotypesHelper.setStereotypePropertyValue(componentElement,
				// type, "Name", subComponent.getComponentName() );
			} catch (ReadOnlyElementException e) {
				e.printStackTrace();
			}
		}

	}

	protected void addPresentationElementsRecursively(
			List<PresentationElement> elementsToAdd,
			DiagramPresentationElement diagramPresentationElement) {
		for (PresentationElement e : elementsToAdd) {
			PresentationElement clonedElement;
			if (!e.getHumanName().startsWith("Diagram Frame")) {
				try {
					clonedElement = (PresentationElement) e.clone();
					diagramPresentationElement
							.sAddPresentationElement(clonedElement);
				} catch (CloneNotSupportedException e1) {
					e1.printStackTrace();
				}
			}
		}
	}
}
