package magicdraw.feature.editor.action;

import java.awt.event.ActionEvent;
import magicdraw.feature.editor.ui.FeatureEditorUIComponnet;
import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.ui.browser.actions.DefaultBrowserAction;
import com.nomagic.magicdraw.uml.BaseElement;
import com.nomagic.magicdraw.uml.DiagramType;
import com.nomagic.magicdraw.uml.symbols.DiagramPresentationElement;
import com.nomagic.uml2.ext.jmi.helpers.StereotypesHelper;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.NamedElement;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Profile;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Stereotype;

public class FeatureModelBrowserAction extends DefaultBrowserAction {

	private static final long serialVersionUID = 1L;
	private Project project;

	public FeatureModelBrowserAction() {
		super("", "Feature Configurator", null, null);
	}

	public void actionPerformed(ActionEvent actionEvent) {
		project = Project.getProject((BaseElement) this.getSelectedObject());
		Element selectedElement = (Element) this.getSelectedObject();
		if (!isWmsLayoutDiagram(selectedElement)) {
			new FeatureEditorUIComponnet(project, selectedElement);
		} else {
			Element rootFeature = findWmsRootFeature();
			Diagram diagram = (Diagram) selectedElement;
			FeatureEditorUIComponnet uicomponent = new FeatureEditorUIComponnet(project, rootFeature, diagram);
			uicomponent.boundKnownVariability(selectedElement);
		}
	}

	private Element findWmsRootFeature() {
		Profile profile = StereotypesHelper.getProfile(project, "WmsFeature");
		for(NamedElement e: profile.getMember()) {
			if(e.getName().equalsIgnoreCase("SiemensWMS"))
				return e;
		}
		return null;
	}

	/**
	 * Defines when your action is available.
	 */
	public void updateState() {
		// Check whether the clicked
		if (this.getSelectedObject() != null) {
			Element e = (Element) this.getSelectedObject();
			Stereotype featureNodeType = StereotypesHelper
					.getAppliedStereotypeByString(e, "FeatureNode");
			if (featureNodeType != null) {
				setEnabled(true);
			} else if (isWmsLayoutDiagram(e))
				setEnabled(true);
			else
				setEnabled(false);
		} else {
			setEnabled(false);
		}
	}

	private boolean isWmsLayoutDiagram(Element e) {
		if (e.getHumanType().equalsIgnoreCase("Diagram")) {
			Diagram diagram = (Diagram) e;
			DiagramPresentationElement diagramPresentationElement = Project
					.getProject(diagram).getDiagram(diagram);
			DiagramType diagramType = diagramPresentationElement
					.getDiagramType();
			if (diagramType.getType().equalsIgnoreCase("WmsOverviewLayout"))
				return true;
		}
		return false;
	}

}
