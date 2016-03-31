package eu.miaofang.md.plugin.report;

import java.awt.event.ActionEvent;

import javax.swing.KeyStroke;

import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.ui.browser.actions.DefaultBrowserAction;
import com.nomagic.magicdraw.uml.BaseElement;
import com.nomagic.magicdraw.uml.DiagramType;
import com.nomagic.magicdraw.uml.symbols.DiagramPresentationElement;
import com.nomagic.uml2.ext.jmi.helpers.StereotypesHelper;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Profile;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Stereotype;

import eu.miaofang.md.plugin.view.FeatureEditorView;

public class ShowReportAction extends DefaultBrowserAction {


	private static final long serialVersionUID = 1L;
	private Project project;

	public ShowReportAction() {
		super("", "Show Report", null, null);
	}

	public void actionPerformed(ActionEvent actionEvent) {
		project = Project.getProject((BaseElement) this.getSelectedObject());
		BaseElement selectedElement = (BaseElement) this.getSelectedObject();
		 ReportTable b=new ReportTable();
		 
		//if the selected element is a root feature
//		if (!isWmsLayoutDiagram(selectedElement)) {
//			new FeatureEditorView(project, selectedElement);
//		} else {
//		// if the selected element is a factory layout, then try to solve some variability by "boundKnownVariability"
//			Element rootFeature = findWmsRootFeature();
//			new FeatureEditorView(
//					project, rootFeature, selectedElement);
//		}
	}

	private Element findWmsRootFeature() {

		Profile profile = StereotypesHelper.getProfile(project,
				"FeatureMetaModel");
		Stereotype stereotype = StereotypesHelper.getStereotype(project,
				"FeatureNode", profile);
		for (Element e : StereotypesHelper.getExtendedElements(stereotype)) {			
			if (e.getHumanName().endsWith("SiemensWMS")) {
				return e;
			}
		}
		return null;
	}

	/**
	 * Defines when your action is available.
	 */
	public void updateState() {
		setEnabled(true);
	}

	private boolean isWmsLayoutDiagram(BaseElement e) {
		if (e.getHumanType().equalsIgnoreCase("Diagram")) {
			Diagram diagram = (Diagram) e;
			DiagramPresentationElement diagramPresentationElement = Project
					.getProject(diagram).getDiagram(diagram);
			diagramPresentationElement.ensureLoaded();
			//ensure the diagram is loaded
			DiagramType diagramType = diagramPresentationElement
					.getDiagramType();
			if (diagramType.getType().equalsIgnoreCase("WmsOverviewLayout"))
				return true;
		}
		return false;
	}
}
