package eu.miaofang.md.plugin.control;

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
import com.nomagic.uml2.ext.magicdraw.components.mdbasiccomponents.Component;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Profile;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Stereotype;
import com.nomagic.uml2.impl.ElementsFactory;

import eu.miaofang.md.plugin.control.builder.ComponentDiagramBuilder;
import eu.miaofang.md.plugin.control.builder.DiagramBuilder;
import eu.miaofang.md.plugin.control.builder.FeatureConfigurationDiagramBuilder;
import eu.miaofang.md.plugin.control.builder.ProcessDiagramBuilder;
import eu.miaofang.md.plugin.control.builder.RootComponentDiagramBuilder;
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
		subMagicDrawPackage.setName(theComponent.getComponentName()
				+ "_ToConfigure");
		if (magicdrawPackage != null)
			subMagicDrawPackage.setOwner(magicdrawPackage);
		else {
			subMagicDrawPackage.setOwner(activeProject.getModel());
		}
		SessionManager.getInstance().closeSession();
		for (WmsModelType modelType : theComponent.getContainedModels()) {
			Activity context;
			if (modelType.getModelType().equals(PlugInConstants.PROCESS_MODEL)) {
				// put WmsProcess into an activity (required by MagicDraw)
				context = factory.createActivityInstance();
				context.setOwner(subMagicDrawPackage);
				new ProcessDiagramBuilder(modelType,
						context, theComponent, activeProject);
			} else if (modelType.getModelType().equals(PlugInConstants.COMPONENT_MODEL)) {
				if (theComponent.equals(featureManager.getRootComponent())) {
					new RootComponentDiagramBuilder(
							modelType, subMagicDrawPackage, (RootComponent)theComponent, activeProject);
				} else {
					new ComponentDiagramBuilder(
							modelType, subMagicDrawPackage, theComponent, activeProject);
					
				}
			} else if (modelType.getModelType().equals(
					PlugInConstants.SELECTED_FEATURE_MODEL)) {
				new FeatureConfigurationDiagramBuilder(
						modelType, subMagicDrawPackage, theComponent, activeProject);
			} else if (modelType.getModelType().equals(
					PlugInConstants.TOPOLOGY_MODEL)) {
				new DiagramBuilder(modelType,
						subMagicDrawPackage, theComponent, activeProject);
			}

		}
		for (WmsComponent subComponent : theComponent.getChildComponents()) {
			if (subComponent.getContainedModels().size() > 0)
				createPackageHierarchy(subMagicDrawPackage,
						(WmsCompositeComponent) subComponent);
		}
		return subMagicDrawPackage;
	}
}
