package eu.miaofang.md.plugin.control.builder;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

import javax.swing.JOptionPane;

import com.nomagic.magicdraw.copypaste.CopyPasting;
import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.openapi.uml.ModelElementsManager;
import com.nomagic.magicdraw.openapi.uml.ReadOnlyElementException;
import com.nomagic.magicdraw.openapi.uml.SessionManager;
import com.nomagic.magicdraw.uml.symbols.DiagramPresentationElement;
import com.nomagic.magicdraw.uml.symbols.PresentationElement;
import com.nomagic.uml2.ext.magicdraw.activities.mdfundamentalactivities.Activity;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Namespace;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Package;
import com.nomagic.uml2.impl.ElementsFactory;

import eu.miaofang.md.plugin.model.WmsModelType;
import eu.miaofang.md.plugin.model.component.WmsCompositeComponent;

public class ProcessDiagramBuilder {

	Diagram diagram;
	WmsModelType diagramType;
	Activity context;

	public ProcessDiagramBuilder(WmsModelType modelType,
				Package subMagicDrawPackage,
				WmsCompositeComponent theComponent, Project activeProject) {
		diagramType = modelType;
		SessionManager.getInstance().createSession("Create New Diagram");
		if(diagramType.getModelTemplate() != null) {
			Collection<DiagramPresentationElement> allDiagrams = activeProject.getDiagrams();
			for (DiagramPresentationElement existingDiagram : allDiagrams) {
				String name = existingDiagram.getHumanName();
				if (name != null && diagramType.getModelTemplate() != null) {
					if (name.endsWith(diagramType.getModelTemplate())) {
						context = (Activity) CopyPasting.copyPasteElement(existingDiagram.getElement().getOwner(),subMagicDrawPackage, false);
					}
				}
			}
		} else {
			try {
				context = activeProject.getElementsFactory().createActivityInstance();
				context.setOwner(subMagicDrawPackage);
				
				diagram = ModelElementsManager.getInstance().createDiagram(
						modelType.getModelType(), context);
				diagram.setName(modelType.getModelName());
			} catch (ReadOnlyElementException e) {
				e.printStackTrace();
			}
		}


		context.setName(modelType.getModelName());
		SessionManager.getInstance().closeSession();
	}
}
