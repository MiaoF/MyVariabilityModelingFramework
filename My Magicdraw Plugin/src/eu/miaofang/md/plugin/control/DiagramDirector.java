package eu.miaofang.md.plugin.control;

import java.util.Collection;
import java.util.List;

import javax.swing.tree.TreePath;

import com.nomagic.magicdraw.copypaste.CopyPasting;
import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.openapi.uml.ModelElementsManager;
import com.nomagic.magicdraw.openapi.uml.PresentationElementsManager;
import com.nomagic.magicdraw.openapi.uml.ReadOnlyElementException;
import com.nomagic.magicdraw.openapi.uml.SessionManager;
import com.nomagic.magicdraw.uml.BaseElement;
import com.nomagic.magicdraw.uml.symbols.DiagramPresentationElement;
import com.nomagic.magicdraw.uml.symbols.PresentationElement;
import com.nomagic.magicdraw.uml.symbols.shapes.ShapeElement;
import com.nomagic.uml2.ext.jmi.helpers.StereotypesHelper;
import com.nomagic.uml2.ext.magicdraw.activities.mdfundamentalactivities.Activity;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Package;
import com.nomagic.uml2.ext.magicdraw.components.mdbasiccomponents.Component;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Profile;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Stereotype;
import com.nomagic.uml2.impl.ElementsFactory;

import eu.miaofang.md.plugin.control.builder.ComponentDiagramBuilder;
import eu.miaofang.md.plugin.control.builder.DiagramBuilder;
import eu.miaofang.md.plugin.control.builder.FeatureConfigurationDiagramBuilder;
import eu.miaofang.md.plugin.control.builder.ProcessDiagramBuilder;
import eu.miaofang.md.plugin.model.FeatureToComponentMapper;
import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsComponent;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;
import eu.miaofang.md.plugin.model.component.instances.RootComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;

public class DiagramDirector {
	private Project activeProject;
	private FeatureToComponentMapper featureManager;
	private Element selectedWmsLayoutDiagram = null;

	public DiagramDirector(Project project, BaseElement selectedLayoutDiagram) {
		this.activeProject = project;
		this.selectedWmsLayoutDiagram = (Element) selectedLayoutDiagram;
	}

	public void createModelStructureInMagicdraw(TreePath[] checkedPaths) {
		featureManager = new FeatureToComponentMapper(checkedPaths);
		RootComponent rootComponent = featureManager.getRootComponent();
		Package modelRootPackage = createPackageHierarchy(null, rootComponent);

		// if selected diagram is a layout, don't create a new layout,
		// but copy the selected one
		if (this.selectedWmsLayoutDiagram != null) {
			SessionManager.getInstance().createSession("Root Layout Topology");
			CopyPasting.copyPasteElement(this.selectedWmsLayoutDiagram,
					modelRootPackage, true);
			SessionManager.getInstance().closeSession();
		} else {
			Diagram diagram;
			try {
				SessionManager.getInstance().createSession(
						"Root Layout Topology");
				diagram = ModelElementsManager.getInstance().createDiagram(
						"WmsOverviewLayout", modelRootPackage);
				diagram.setName("WMS Layout Overview");
				SessionManager.getInstance().closeSession();
			} catch (ReadOnlyElementException e) {
				e.printStackTrace();
			}
		}

	}

	private Package createPackageHierarchy(Package magicdrawPackage,
			WmsCompositeComponent theComponent) {
		SessionManager.getInstance().createSession(
				theComponent.getComponentName());
		ElementsFactory factory = activeProject.getElementsFactory();
		Package subMagicDrawPackage = factory.createPackageInstance();
		
		subMagicDrawPackage.setName(theComponent.getComponentName() + "_ToConfigure");
		if (magicdrawPackage != null)
			subMagicDrawPackage.setOwner(magicdrawPackage);
		else {
			subMagicDrawPackage.setOwner(activeProject.getModel());
		}
		for (WmsModelType modelType : theComponent.getContainedModels()) {
			DiagramBuilder builder = null;
			Diagram diagram;
			Activity context;
			// put WmsProcess into an activity (required by MagicDraw)
			if (modelType.getModelType().equals(PlugInConstants.PROCESS_MODEL)) {
				
				context = factory.createActivityInstance();
				builder = new ProcessDiagramBuilder(modelType, context);
				diagram = builder.getDiagram();
//						createDiagram(modelType, context);
				context.setOwner(subMagicDrawPackage);
			} else if (modelType.getModelType().equals(PlugInConstants.COMPONENT_MODEL)) {
				builder = new ComponentDiagramBuilder(modelType, subMagicDrawPackage);
				builder.addInvariableElments(theComponent, activeProject);
				builder.addVariableElments(theComponent, activeProject);
			} else if (modelType.getModelType().equals(PlugInConstants.SELECTED_FEATURE_MODEL)){
				builder = new FeatureConfigurationDiagramBuilder(modelType, subMagicDrawPackage);
//				diagram = createDiagram(modelType, subMagicDrawPackage,
//						theComponent);
			}
//			diagram.setName(modelType.getModelName());
		}
		SessionManager.getInstance().closeSession();
		for (WmsComponent subComponent : theComponent.getChildComponents()) {
			if (subComponent.getContainedModels().size() > 0)
				createPackageHierarchy(subMagicDrawPackage,
						(WmsCompositeComponent) subComponent);
		}
		return subMagicDrawPackage;
	}

//	private Diagram createDiagram(WmsModelType modelType,
//			Package theOwnerPackage, WmsCompositeComponent theComponent) {
//		Diagram diagram = null;
//		try {
//			diagram = ModelElementsManager.getInstance().createDiagram(
//					modelType.getModelType(), theOwnerPackage);
//			initialzeDiagram(modelType, diagram, theComponent);
//		} catch (ReadOnlyElementException e) {
//			e.printStackTrace();
//		}
//		return diagram;
//	}
//
//	private void initialzeDiagram(WmsModelType modelType,
//			Diagram targetDiagram, WmsCompositeComponent theComponent) {
//		if (modelType.getModelType().equals(PlugInConstants.COMPONENT_MODEL)) {
//			addInvariableComponents(modelType, targetDiagram);
//			addVariableComponents(modelType, targetDiagram, theComponent);
//		}
//		
//		if (modelType.getModelType().equals(PlugInConstants.SELECTED_FEATURE_MODEL)) {
////			
//		}
//	}

//	private void addVariableComponents(WmsModelType modelType,
//			Diagram targetDiagram, WmsCompositeComponent theComponent) {
//		DiagramPresentationElement diagramPresentationElement = Project
//				.getProject(targetDiagram).getDiagram(targetDiagram);
//		int locationY = 210;
//		int locationX = 60;
//		for (WmsComponent subComponent : theComponent.getChildComponents()) {
//			Component componentElement = activeProject.getElementsFactory()
//					.createComponentInstance();
//			componentElement.setName(subComponent.getComponentName());
//			Profile componentProfile = StereotypesHelper.getProfile(
//					activeProject, PlugInConstants.COMPONENTE_PROFILE);
//			Stereotype type = StereotypesHelper.getStereotype(activeProject,
//					subComponent.getComponentType(), componentProfile);
//			if (StereotypesHelper.canApplyStereotype(componentElement, type))
//				StereotypesHelper.addStereotype(componentElement, type);
//
//			PresentationElementsManager presentationManager = PresentationElementsManager
//					.getInstance();
//			try {
//				ModelElementsManager.getInstance().addElement(componentElement,
//						targetDiagram.getOwner());
//				ShapeElement shapeElement = presentationManager.createShapeElement(componentElement,
//						diagramPresentationElement);
//				shapeElement.setLocation(locationX, locationY);
//				locationX += 140;
////				StereotypesHelper.setStereotypePropertyValue(componentElement, type, "Name", subComponent.getComponentName() );
//			} catch (ReadOnlyElementException e) {
//				e.printStackTrace();
//			}
//		}
//
//	}
//
//	private void addInvariableComponents(WmsModelType modelType,
//			Diagram targetDiagram) {
//		Collection<DiagramPresentationElement> allDiagrams = activeProject
//				.getDiagrams();
//		for (DiagramPresentationElement existingDiagram : allDiagrams) {
//			String name = existingDiagram.getHumanName();
//			if (name.endsWith(modelType.getModelTemplate())) {
//				existingDiagram.ensureLoaded();
//				DiagramPresentationElement diagramPresentationElement = Project
//						.getProject(targetDiagram).getDiagram(targetDiagram);
//				addPresentationElementsRecursively(existingDiagram.getPresentationElements(),
//						diagramPresentationElement);
//			}
//		}
//	}
//
//	private void addPresentationElementsRecursively(
//			List<PresentationElement> elementsToAdd,
//			DiagramPresentationElement diagramPresentationElement) {
//		for (PresentationElement e : elementsToAdd) {
//			PresentationElement clonedElement;
//			try {
//				clonedElement = (PresentationElement) e.clone();
//				diagramPresentationElement
//						.sAddPresentationElement(clonedElement);
//			} catch (CloneNotSupportedException e1) {
//				e1.printStackTrace();
//			}
//			if(e.getPresentationElementCount() > 0)
//				addPresentationElementsRecursively(e.getPresentationElements(), diagramPresentationElement);
//
//		}
//	}
//
//	private Diagram createDiagram(WmsModelType modelType,
//			Activity theOwnerPackage) {
//		Diagram diagram = null;
//		try {
//			diagram = ModelElementsManager.getInstance().createDiagram(
//					modelType.getModelType(), theOwnerPackage);
//		} catch (ReadOnlyElementException e) {
//			// TODO Auto-generated catch block
//			e.printStackTrace();
//		}
//		return diagram;
//	}
}
