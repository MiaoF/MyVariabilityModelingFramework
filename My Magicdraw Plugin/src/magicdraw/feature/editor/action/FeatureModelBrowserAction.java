package magicdraw.feature.editor.action;

import java.awt.BorderLayout;
import java.awt.event.ActionEvent;

import javax.swing.JOptionPane;

import myplugin.CountingVisitor;
import myplugin.SimplePluginBehavior;

import com.nomagic.magicdraw.core.Application;
import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.ui.MainFrame;
import com.nomagic.magicdraw.ui.browser.Browser;
import com.nomagic.magicdraw.ui.browser.BrowserTabTree;
import com.nomagic.magicdraw.ui.browser.actions.DefaultBrowserAction;
import com.nomagic.magicdraw.uml.BaseElement;
import com.nomagic.magicdraw.uml.ClassTypes;
import com.nomagic.uml2.ext.jmi.helpers.StereotypesHelper;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Profile;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Stereotype;

public class FeatureModelBrowserAction extends DefaultBrowserAction {

	private static final long serialVersionUID = 1L;

	public FeatureModelBrowserAction() {
		super("", "FeatureEditor", null, null);
	}

	public void actionPerformed(ActionEvent actionEvent) {
		Project project = Project.getProject((BaseElement) this.getSelectedObject());
//		// find a profile
//		Profile profile = StereotypesHelper.getProfile(project, "FeatureViewProfile");
//		// find a stereotype
//		Stereotype stereotype = StereotypesHelper.getStereotype(project,
//				"FeatureNode", profile);
		
		Element e = (Element) this.getSelectedObject();
		Stereotype featureNodeType = StereotypesHelper.getAppliedStereotypeByString(e, "FeatureNode");
//		String classType = ClassTypes.getShortName(e
//				.getAppliedStereotypeInstance().getClass());
//		JOptionPane.showMessageDialog(null, e.get_relationshipOfRelatedElement().size());
		FeatureEditorUIComponnet configUI = new FeatureEditorUIComponnet(e);
		configUI.open();

	}

	/**
	 * Defines when your action is available.
	 */
	public void updateState() {
		// This action is only available when your click on an instance of
		// Element in the containement tree
		if (this.getSelectedObject() != null) {
			Element e = (Element) this.getSelectedObject();
			Stereotype featureNodeType = StereotypesHelper.getAppliedStereotypeByString(e, "FeatureNode");
			if (featureNodeType != null) {
				setEnabled(true);
			} else {
				setEnabled(false);
			}
		} else {
			setEnabled(false);
		}
	}

}
