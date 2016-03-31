package eu.miaofang.md.plugin.control;

import java.util.Collection;
import java.util.Iterator;

import javax.swing.JOptionPane;
import javax.swing.tree.TreePath;

import com.nomagic.magicdraw.copypaste.CopyPasting;
import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.openapi.uml.ModelElementsManager;
import com.nomagic.magicdraw.openapi.uml.PresentationElementsManager;
import com.nomagic.magicdraw.openapi.uml.ReadOnlyElementException;
import com.nomagic.magicdraw.openapi.uml.SessionManager;
import com.nomagic.magicdraw.uml.BaseElement;
import com.nomagic.magicdraw.uml.symbols.DiagramPresentationElement;
import com.nomagic.magicdraw.uml.symbols.shapes.ShapeElement;
import com.nomagic.uml2.ext.jmi.helpers.StereotypesHelper;
import com.nomagic.uml2.ext.magicdraw.activities.mdfundamentalactivities.Activity;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Namespace;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Package;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.PackageableElement;
import com.nomagic.uml2.ext.magicdraw.components.mdbasiccomponents.Component;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Profile;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Stereotype;
import com.nomagic.uml2.impl.ElementsFactory;

import eu.miaofang.md.plugin.control.builder.ComponentModelBuilder;
import eu.miaofang.md.plugin.control.builder.ModelBuilder;
import eu.miaofang.md.plugin.control.builder.FeatureConfigurationModelBuilder;
import eu.miaofang.md.plugin.control.builder.ProcessModelBuilder;
import eu.miaofang.md.plugin.control.builder.RootComponentModelBuilder;
import eu.miaofang.md.plugin.model.FeatureToComponentMapper;
import eu.miaofang.md.plugin.model.WmsModel;
import eu.miaofang.md.plugin.model.component.WmsComponent;
import eu.miaofang.md.plugin.model.component.CompositeComponent;
import eu.miaofang.md.plugin.model.component.instances.RootComponent;
import eu.miaofang.wms.plugin.config.PlugInConstants;

public class ModelInstanceGenerator {
	private Project activeProject;
	private FeatureToComponentMapper featureManager;
	private Element selectedWmsLayoutDiagram = null;

	public ModelInstanceGenerator(Project project,
			BaseElement selectedLayoutDiagram) {
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
			CompositeComponent theComponent) {
		SessionManager.getInstance().createSession(
				theComponent.getComponentName());
		ElementsFactory factory = activeProject.getElementsFactory();
//
		
		String name = theComponent.getComponentName() + "_ToDo";

		PackageableElement existingRootElement = checkExistingPackage(name);
//		Element existingSubPackage = this.tryToGetExistingPackage(magicdrawPackage, name);
		Package subMagicDrawPackage = null;
		
		
		
		if(magicdrawPackage == null) { //if it is root
			if(existingRootElement != null) { // root package exist
				subMagicDrawPackage = (Package) existingRootElement;
			} else {
				subMagicDrawPackage = factory.createPackageInstance();
				subMagicDrawPackage.setName(name);
				subMagicDrawPackage.setOwner(activeProject.getModel());
			}

		} else {//if it is not root, but a subpackage
			Element existingSubPackage = this.tryToGetExistingPackage(magicdrawPackage, name);
			if(existingSubPackage == null){ //no existing one
				subMagicDrawPackage = factory.createPackageInstance();
				subMagicDrawPackage.setName(name);
				subMagicDrawPackage.setOwner(magicdrawPackage);
			} else {//the package exist, then use this one
				subMagicDrawPackage = (Package) existingSubPackage;
			}
				
		}
	
		
		SessionManager.getInstance().closeSession();
		for (WmsModel modelType : theComponent.getContainedModels()) {
			if (checkExistingPackage(subMagicDrawPackage,
					modelType.getModelName()) == null) { //if there is no existing diagram created, then create a new one, otherwise leave it
				Activity context;
				if (modelType.getModelType().equals(
						PlugInConstants.PROCESS_MODEL)) {
					// put WmsProcess into an activity (required by MagicDraw)
					// context = factory.createActivityInstance();
					// context.setOwner(subMagicDrawPackage);
					new ProcessModelBuilder(modelType, subMagicDrawPackage,
							theComponent, activeProject);
				} else if (modelType.getModelType().equals(
						PlugInConstants.COMPONENT_MODEL)) {
					if (theComponent.equals(featureManager.getRootComponent())) {
						new RootComponentModelBuilder(modelType,
								subMagicDrawPackage,
								(RootComponent) theComponent, activeProject);
					} else {
						new ComponentModelBuilder(modelType,
								subMagicDrawPackage, theComponent,
								activeProject);
					}
				} else if (modelType.getModelType().equals(
						PlugInConstants.SELECTED_FEATURE_MODEL)) {
					new FeatureConfigurationModelBuilder(modelType,
							subMagicDrawPackage, theComponent, activeProject, featureManager.getAllSelectedFeatures() );
				} else if (modelType.getModelType().equals(
						PlugInConstants.TOPOLOGY_MODEL)) {
					new ModelBuilder(modelType, subMagicDrawPackage,
							theComponent, activeProject);
				}
			}

		}
		for (WmsComponent subComponent : theComponent.getChildComponents()) {
			if (subComponent.getComponentType().equalsIgnoreCase(
					PlugInConstants.COMPOSITE_COMPONENT)) {
				CompositeComponent comp = (CompositeComponent) subComponent;
				if (comp.getContainedModels().size() > 0)
					createPackageHierarchy(subMagicDrawPackage, comp);
			}
		}
		return subMagicDrawPackage;
	}

	private PackageableElement checkExistingPackage(String name) {
		Iterator<PackageableElement> packages = activeProject.getModel()
				.getPackagedElement().iterator();
		while (packages.hasNext()) {
			PackageableElement element = packages.next();
			if (element.getQualifiedName().equals(name)) {
//				JOptionPane.showMessageDialog(null, "Dont Create " + name);
				return element;
			}
		}
		return null;
	}


	private Element checkExistingPackage(Package magicPackage, String name) {
		Iterator<Element> diagrams = magicPackage.getOwnedElement().iterator();
		while (diagrams.hasNext()) {
			Element diagram = diagrams.next();
//			JOptionPane.showMessageDialog(null, diagram.getHumanName() + "---" + name);
			if (diagram.getHumanName().endsWith(name)) {
//				JOptionPane.showMessageDialog(null, "Dont Create " + name);
				return diagram;
			}
		}
		return null;
	}
	
	private Element tryToGetExistingPackage(Package magicPackage, String name) {
		if(magicPackage == null)
			magicPackage = activeProject.getModel();
		Iterator<Element> diagrams = magicPackage.getOwnedElement().iterator();
		while (diagrams.hasNext()) {
			Element diagram = diagrams.next();
			if (diagram.getHumanName().endsWith(name)) {
//				JOptionPane.showMessageDialog(null, "Dont Create " + name);
				return diagram;
			}
		}
		return null;
	}

}
