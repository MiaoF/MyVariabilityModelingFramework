package impl.generator;

import java.io.File;
import java.io.IOException;

import javax.swing.JFileChooser;

import org.apache.commons.io.FileUtils;
import org.eclipse.emf.ecore.EObject;
import org.eclipse.emf.ecore.resource.Resource;
import org.eclipse.emf.ecore.resource.ResourceSet;
import org.eclipse.emf.ecore.resource.impl.ResourceSetImpl;
import org.eclipse.emf.ecore.util.EcoreUtil;
import org.eclipse.emf.ecore.xmi.impl.XMIResourceFactoryImpl;
import org.eclipse.emf.mwe.core.WorkflowRunner;
import org.eclipse.emf.common.util.URI;
import org.eclipse.emf.common.util.WrappedException;
import org.eclipse.uml2.uml.Model;
import org.eclipse.uml2.uml.UMLPackage;
import org.eclipse.uml2.uml.resource.UMLResource;
import org.eclipse.uml2.uml.resources.util.UMLResourcesUtil;
import org.eclipse.uml2.uml.util.UMLUtil;

public class SaveGeneratedCode {

	private static ResourceSet RESOURCE_SET;

	public static void main(String[] args) {
		JFileChooser chooser = FileChooser("Choose the folder with exported UML models:");

		if (chooser.showOpenDialog(null) == JFileChooser.APPROVE_OPTION) {
			try {
				File targetDir = new File("src/model");
				// 1. copy all generated MD models into src/model
				FileUtils.copyDirectory(chooser.getSelectedFile(), targetDir);

				// /1.1 convert UML version to this eclipse UML version
				initResource();
				saveAllUMLModelsAgainToSolveVersionCompatibility("src/model");

				// 2. start the progress bar
//				Thread queryThread = new Thread() {
//					public void run() {
//						runQueries();
//					}
//
//					private void runQueries() {
//						try {
//							SwingProgressBarExample bar = new SwingProgressBarExample();
//							bar.showBar();
//						} catch (Exception e) {
//							e.printStackTrace();
//						}
//					}
//				};
//				queryThread.start();

				// 3. do the two step generation
				new WorkflowRunner()
						.main(new String[] { "src/workflow/generator0.mwe" });
				new WorkflowRunner()
						.main(new String[] { "src/workflow/workflow-gen/generator1.mwe" });

				// 4. copy the generated code into a target folder.
				chooser = FileChooser("Choose the folder to generatre code:");
				if (chooser.showOpenDialog(null) == JFileChooser.APPROVE_OPTION) {
					File srcDir = new File("src-gen");
					FileUtils.copyDirectoryToDirectory(srcDir,
							chooser.getSelectedFile());
				}
			} catch (Exception e) {
				e.printStackTrace();
			}
		}
	}

	private static void saveAllUMLModelsAgainToSolveVersionCompatibility(
			String modelFolderPath) {
		File modelFolder = new File(modelFolderPath);
		for (final File fileEntry : modelFolder.listFiles()) {
			if (fileEntry.getAbsolutePath().endsWith(".uml")) {
				URI modelPath = getModelPath(fileEntry.getAbsolutePath());
				loadModelAndSave(modelPath);
			}
		}
	}

	private static void loadModelAndSave(URI uri) {
		Package modelToLoad = null;
		try {
			// Load the requested resource
			Resource resource = RESOURCE_SET.getResource(uri, true);
			
	        ResourceSet set = new ResourceSetImpl();

	        set.getPackageRegistry().put(UMLPackage.eNS_URI, UMLPackage.eINSTANCE);
	        set.getResourceFactoryRegistry().getExtensionToFactoryMap()
	            .put(UMLResource.FILE_EXTENSION, UMLResource.Factory.INSTANCE);
	        set.createResource(uri);
	        Resource r = set.getResource(uri, true);

	        Model model = (Model) EcoreUtil.getObjectByType(r.getContents(), UMLPackage.Literals.MODEL);
	        try {
				r.save(null);
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
//	        save(model,uri);
			
			
			// Get the first (should be only) package from it
//			save(EcoreUtil.getObjectByType(resource.getContents(),
//					UMLPackage.Literals.PACKAGE), uri);
		} catch (WrappedException we) {
			System.err.println("Fail to load the model. Path: " + uri);
			System.exit(1);
		}
	}

	private static JFileChooser FileChooser(String textMessage) {
		JFileChooser chooser = new JFileChooser();
		chooser.setCurrentDirectory(new java.io.File("."));
		chooser.setFileSelectionMode(JFileChooser.DIRECTORIES_ONLY);
		chooser.setFileHidingEnabled(false);
		chooser.setDialogTitle(textMessage);
		chooser.setAcceptAllFileFilterUsed(false);
		return chooser;
	}

	private static URI getModelPath(String path) {
		URI uri = URI.createFileURI(path);
		System.out.println(uri.toString());
		return uri;
	}

	private static void initResource() {
		RESOURCE_SET = new ResourceSetImpl();
		UMLResourcesUtil.init(RESOURCE_SET);
	}

	protected static void save(Model package_, URI uri) {
		// Create a resource-set to contain the resource(s) that we are saving
		ResourceSet resourceSet = new ResourceSetImpl();

		// Initialize registrations of resource factories, library models,
		// profiles, Ecore metadata, and other dependencies required for
		// serializing and working with UML resources. This is only necessary in
		// applications that are not hosted in the Eclipse platform run-time, in
		// which case these registrations are discovered automatically from
		// Eclipse extension points.
		UMLResourcesUtil.init(resourceSet);

		// Create the output resource and add our model package to it.
		Resource resource = resourceSet.createResource(uri);
		resource.getContents().add(package_);
		
		// And save
		try {
			
			resource.save(null); // no save options needed
		} catch (IOException ioe) {
		}
	}

}
