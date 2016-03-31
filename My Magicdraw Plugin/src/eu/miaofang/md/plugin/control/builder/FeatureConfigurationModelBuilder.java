package eu.miaofang.md.plugin.control.builder;

import java.util.List;

import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.openapi.uml.ModelElementsManager;
import com.nomagic.magicdraw.openapi.uml.PresentationElementsManager;
import com.nomagic.magicdraw.openapi.uml.ReadOnlyElementException;
import com.nomagic.magicdraw.openapi.uml.SessionManager;
import com.nomagic.magicdraw.uml.symbols.DiagramPresentationElement;
import com.nomagic.magicdraw.uml.symbols.shapes.ShapeElement;
import com.nomagic.uml2.ext.jmi.helpers.StereotypesHelper;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Class;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Namespace;
import com.nomagic.uml2.ext.magicdraw.components.mdbasiccomponents.Component;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Profile;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Stereotype;

import eu.miaofang.md.plugin.model.SelectedFeature;
import eu.miaofang.md.plugin.model.WmsModel;
import eu.miaofang.md.plugin.model.component.CompositeComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;


public class FeatureConfigurationModelBuilder{
	Diagram diagram;
	WmsModel diagramType;
	Namespace namespace;

	public FeatureConfigurationModelBuilder(WmsModel modelType,
			Namespace theOwnerPackage, CompositeComponent theComponent, Project activeProject, List<SelectedFeature> selectedFeatures) {		
		try {
			SessionManager.getInstance().createSession("Create New Diagram");
			diagram = ModelElementsManager.getInstance().createDiagram(
					modelType.getModelType(), theOwnerPackage);
			diagram.setName(modelType.getModelName());
			diagramType = modelType;
			namespace = theOwnerPackage;
			addInvariableElments(selectedFeatures, activeProject);
//			addVariableElments(theComponent, activeProject);
			SessionManager.getInstance().closeSession();
			// initialzeDiagram(modelType, diagram, theComponent);
		} catch (ReadOnlyElementException e) {
			e.printStackTrace();
		}
		
	}


	public void addInvariableElments(List<SelectedFeature> selectedFeatures, Project activeProject) {
		for (SelectedFeature feature : selectedFeatures) {
			DiagramPresentationElement diagramPresentationElement = Project
				.getProject(diagram).getDiagram(diagram);
			diagramPresentationElement.ensureLoaded();
			int locationY = 50;
			int locationX = 50;
			Class fn = activeProject.getElementsFactory().createClassInstance();
			fn.setName(feature.getFeatureName());
			
			Profile featureProfile = StereotypesHelper.getProfile(
					activeProject, PlugInConstants.FEATURE_PROFILE);
			Stereotype type = StereotypesHelper.getStereotype(activeProject,
					"FeatureNode", featureProfile);
			if (StereotypesHelper.canApplyStereotype(fn, type))
				StereotypesHelper.addStereotype(fn, type);

			PresentationElementsManager presentationManager = PresentationElementsManager
					.getInstance();
			try {
				ModelElementsManager.getInstance().addElement(fn,
						diagram.getOwner());
				ShapeElement shapeElement = presentationManager
						.createShapeElement(fn,	diagramPresentationElement);
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

}
