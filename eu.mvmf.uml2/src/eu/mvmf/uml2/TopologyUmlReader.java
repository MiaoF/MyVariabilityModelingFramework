package eu.mvmf.uml2;

import java.io.File;
import java.util.ArrayList;
import java.util.List;


import org.eclipse.emf.common.util.EList;
import org.eclipse.emf.common.util.WrappedException;
import org.eclipse.emf.ecore.resource.Resource;
import org.eclipse.emf.ecore.resource.ResourceSet;
import org.eclipse.emf.ecore.resource.impl.ResourceSetImpl;
import org.eclipse.emf.ecore.util.EcoreUtil;
import org.eclipse.emf.common.util.URI;
import org.eclipse.uml2.uml.Model;
import org.eclipse.uml2.uml.NamedElement;
import org.eclipse.uml2.uml.Stereotype;
import org.eclipse.uml2.uml.UMLPackage;
import org.eclipse.uml2.uml.resources.util.UMLResourcesUtil;

public class TopologyUmlReader {

	private static ResourceSet RESOURCE_SET;

	public static void main(String[] args) {
		initResource();
		URI modelPath = getModelPath(args);
		Model topoModel = loadModel(modelPath);

		// at the momente hardcode
		readAKLTopology(topoModel);
	}

	private static void readAKLTopology(Model topoModel) {
		List<org.eclipse.uml2.uml.Element> nonCriticalLocations = new ArrayList<org.eclipse.uml2.uml.Element>();

		EList<NamedElement> elements = topoModel.getOwnedMembers();
		for (NamedElement ele : elements) {
			if (isCriticalLoaction(ele)) {
				nonCriticalLocations.add(ele);
			}
		}
		System.out.println("No# of this type of Locations: "
				+ nonCriticalLocations.size());
	}

	private static boolean isCriticalLoaction(
			NamedElement element) {
		if (element.getAppliedStereotypes().size() > 0) {
			Stereotype stereotype = element.getAppliedStereotypes().get(0);
			if(element.hasValue(stereotype, "isForkliftDeliveryLocation")) {
				Object isForkliftDeliveryLocation = element.getValue(stereotype,
						"isForkliftDeliveryLocation");
					boolean isTheForkliftDeliveryLocation = (Boolean) isForkliftDeliveryLocation;
					if (isTheForkliftDeliveryLocation) {
						String locationID = (String) element.getValue(stereotype,
								"locationID");
						System.out.println("isForkliftDeliveryLocation == true: locationID " + locationID);
						return true;
					}
			}
		}
		return false;
	}

	private static URI getModelPath(String[] args) {
		// String modelPath = "D://test//ProfileUsingProject.uml";
		String filePath;
		if (args.length > 0)
			filePath = args[0];
		else
			filePath = "models//sgm//SgmConfiguration.uml";

		filePath = new File(filePath).getAbsolutePath();

		URI uri = URI.createFileURI(filePath);
		System.out.println(uri.toString());
		return uri;
	}

	private static Model loadModel(URI uri) {
		Model modelToLoad = null;
		try {
			// Load the requested resource
			Resource resource = RESOURCE_SET.getResource(uri, true);

			// Get the first (should be only) package from it
			modelToLoad = (Model) EcoreUtil.getObjectByType(
					resource.getContents(), UMLPackage.Literals.PACKAGE);
		} catch (WrappedException we) {
			System.err.println("Fail to load the model. Path: " + uri);
			System.exit(1);
		}
		return modelToLoad;
	}

	private static void initResource() {
		RESOURCE_SET = new ResourceSetImpl();
		UMLResourcesUtil.init(RESOURCE_SET);
	}

}
