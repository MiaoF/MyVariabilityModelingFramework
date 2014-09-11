package magicdraw.feature.models;

import java.util.Collection;
import java.util.List;

import javax.swing.tree.TreePath;

import magicdraw.feature.models.details.ModelPackage;
import magicdraw.feature.models.details.WarehouseModelType;

import com.nomagic.magicdraw.copypaste.CopyPasting;
import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.openapi.uml.ModelElementsManager;
import com.nomagic.magicdraw.openapi.uml.PresentationElementsManager;
import com.nomagic.magicdraw.openapi.uml.ReadOnlyElementException;
import com.nomagic.magicdraw.openapi.uml.SessionManager;
import com.nomagic.magicdraw.uml.symbols.DiagramPresentationElement;
import com.nomagic.magicdraw.uml.symbols.PresentationElement;
import com.nomagic.uml2.ext.magicdraw.activities.mdfundamentalactivities.Activity;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Package;
import com.nomagic.uml2.impl.ElementsFactory;

public class ArtifactCreator {
	private Project activeProject;
	private ModelHierarchy modelStructure;
	private Element rootFeature;
	private Diagram selectedWmsLayoutDiagram = null;

	public ArtifactCreator(Project project, Diagram selectedElement,
			Element rootFeature) {
		this.activeProject = project;
		this.selectedWmsLayoutDiagram = selectedElement;
		this.rootFeature = rootFeature;
	}

	public void createModelStructureInMagicdraw(TreePath[] checkedPaths) {
		modelStructure = new ModelHierarchy(checkedPaths, this.selectedWmsLayoutDiagram);
		ModelPackage rootPackage = modelStructure.getRootPackage();
		Package modelRootPackage = createPackageHierarchy(null, rootPackage);
		if(this.selectedWmsLayoutDiagram != null) {
			Diagram diagram;
			SessionManager.getInstance().createSession("Root Layout Topology");
//				Collection<DiagramPresentationElement> diagrams = this.activeProject
//						.getDiagrams();
//				DiagramPresentationElement selectedLayoutDiagram = null;
//				for (DiagramPresentationElement aDiagram : diagrams) {
//					if (aDiagram.getHumanName().equalsIgnoreCase(
//							selectedWmsLayoutDiagram.getHumanName())) {
//						selectedLayoutDiagram = aDiagram;
//					}
//				}
			CopyPasting.copyPasteElement(this.selectedWmsLayoutDiagram, modelRootPackage, true);
			SessionManager.getInstance().closeSession();
			
		} else {
			Diagram diagram;
			try {
				SessionManager.getInstance().createSession("Root Layout Topology");
				diagram = ModelElementsManager.getInstance().createDiagram(
						"WmsOverviewLayout", modelRootPackage);
				diagram.setName("WMS Overview");
				SessionManager.getInstance().closeSession();
			} catch (ReadOnlyElementException e) {
				e.printStackTrace();
			}
		}
			
	}

	private Package createPackageHierarchy(Package magicdrawPackage,
			ModelPackage aPackageModel) {
		SessionManager.getInstance().createSession(
				aPackageModel.getPackageName());
		ElementsFactory factory = activeProject.getElementsFactory();
		Package subMagicDrawPackage = factory.createPackageInstance();
		subMagicDrawPackage.setName(aPackageModel.getPackageName() + "_ToDo");
		if (magicdrawPackage != null)
			subMagicDrawPackage.setOwner(magicdrawPackage);
		else {
			subMagicDrawPackage.setOwner(activeProject.getModel());
		}
		for (WarehouseModelType modelType : aPackageModel.getContainedModels()) {
			Diagram diagram;
			Activity context;
			try {
				if (modelType.getModelType().equalsIgnoreCase("WmsProcess")) {
					context = factory.createActivityInstance();
					diagram = ModelElementsManager.getInstance().createDiagram(
							modelType.getModelType(), context);
					context.setOwner(subMagicDrawPackage);
				} else {
					diagram = ModelElementsManager.getInstance().createDiagram(
							modelType.getModelType(), subMagicDrawPackage);
				}
				diagram.setName(modelType.getModelName());
			} catch (ReadOnlyElementException e) {
				e.printStackTrace();
			}
		}
		SessionManager.getInstance().closeSession();
		for (ModelPackage subPackage : aPackageModel.getChildPackages()) {
			createPackageHierarchy(subMagicDrawPackage, subPackage);
		}

		return subMagicDrawPackage;
	}
}
