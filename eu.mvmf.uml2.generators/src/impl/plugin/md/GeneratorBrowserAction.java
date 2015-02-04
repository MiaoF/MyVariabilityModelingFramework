package impl.plugin.md;

import java.awt.event.ActionEvent;
import javax.swing.JFileChooser;
import javax.swing.JOptionPane;
import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.emfuml2xmi.v4.EmfUml2XmiPlugin;
import com.nomagic.magicdraw.ui.browser.actions.DefaultBrowserAction;
import com.nomagic.magicdraw.uml.BaseElement;

public class GeneratorBrowserAction extends DefaultBrowserAction {

	private static final long serialVersionUID = 1L;
	private Project project;

	public GeneratorBrowserAction() {
		super("", "Eclipse UML Export", null, null);
	}

	public void actionPerformed(ActionEvent actionEvent) {
		project = Project.getProject((BaseElement) this.getSelectedObject());
//		BaseElement selectedElement = (BaseElement) this.getSelectedObject();
		showChooseDialog();
	}

	private void showChooseDialog() {
		JFileChooser chooser = new JFileChooser();
		chooser.setCurrentDirectory(new java.io.File("."));
		chooser.setDialogTitle("Choose a Path for code generation");
		chooser.setFileSelectionMode(JFileChooser.DIRECTORIES_ONLY);
		chooser.setAcceptAllFileFilterUsed(false);

		if (chooser.showOpenDialog(null) == JFileChooser.APPROVE_OPTION) {
			try {
				EmfUml2XmiPlugin.getInstance().exportXMI(project,
						chooser.getSelectedFile().getAbsolutePath());
			} catch (Exception e) {
				e.printStackTrace();
			}
		} else {
			JOptionPane.showMessageDialog(null, "wrong");
		}
	}

//	private void initUML() {
//		ResourceSet resourceSet = new ResourceSetImpl();
//		resourceSet.getPackageRegistry().put(UMLPackage.eNS_URI, UMLPackage.eINSTANCE);
//
//		resourceSet.getResourceFactoryRegistry().getExtensionToFactoryMap().put(UMLResource.FILE_EXTENSION, UMLResource.Factory.INSTANCE);
//		Map uriMap = resourceSet.getURIConverter().getURIMap();
//		
//		URI uml_resource_uri = URI.createPlatformPluginURI("eu.mvmf.uml2.generators", true);
//		uriMap.put(URI.createURI("pathmap://resources/"), uml_resource_uri.appendSegment("resources").appendSegment(""));
//		uml_resource_uri = URI.createPlatformPluginURI("org.eclipse.uml2.uml.resources", true);
//		
//		
//		
//		URI uri = URI.createURI("jar:file:/D:/development/eclipse-uml2/eclipse/plugins/org.eclipse.uml2.uml_5.0.0.v20140602-0749.jar!/"); // for example
//		uriMap.put(URI.createURI(UMLResource.LIBRARIES_PATHMAP), uri.appendSegment("libraries").appendSegment(""));
//		uriMap.put(URI.createURI(UMLResource.METAMODELS_PATHMAP), uri.appendSegment("metamodels").appendSegment(""));
//		uriMap.put(URI.createURI(UMLResource.PROFILES_PATHMAP), uri.appendSegment("profiles").appendSegment(""));
//	}

}
