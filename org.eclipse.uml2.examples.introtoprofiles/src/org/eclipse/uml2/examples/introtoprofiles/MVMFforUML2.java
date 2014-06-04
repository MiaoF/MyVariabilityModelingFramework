package org.eclipse.uml2.examples.introtoprofiles;

import java.io.File;
import java.nio.file.Path;
import java.nio.file.Paths;

import org.eclipse.emf.common.util.EList;
import org.eclipse.emf.common.util.URI;
import org.eclipse.emf.common.util.WrappedException;
import org.eclipse.emf.ecore.resource.Resource;
import org.eclipse.emf.ecore.resource.ResourceSet;
import org.eclipse.emf.ecore.resource.impl.ResourceSetImpl;
import org.eclipse.emf.ecore.util.EcoreUtil;
import org.eclipse.uml2.uml.Element;
import org.eclipse.uml2.uml.Model;
import org.eclipse.uml2.uml.NamedElement;
import org.eclipse.uml2.uml.Stereotype;
import org.eclipse.uml2.uml.UMLPackage;
import org.eclipse.uml2.uml.resource.UMLResource;
import org.eclipse.uml2.uml.resources.util.UMLResourcesUtil;

public class MVMFforUML2 {

	public static boolean DEBUG = true;


	private static final ResourceSet RESOURCE_SET;
	
	static {
		// Create a resource-set to contain the resource(s) that we load and
		// save
		RESOURCE_SET = new ResourceSetImpl();

		// Initialize registrations of resource factories, library models,
		// profiles, Ecore metadata, and other dependencies required for
		// serializing and working with UML resources. This is only necessary in
		// applications that are not hosted in the Eclipse platform run-time, in
		// which case these registrations are discovered automatically from
		// Eclipse extension points.
		UMLResourcesUtil.init(RESOURCE_SET);

	}

	public static void main(String[] args) {
		String modelPath = getModelPath();
		
		Model testProfileModel = (Model) load(URI.createURI(modelPath)
				.appendSegment("TestProfiles")
				.appendFileExtension(UMLResource.FILE_EXTENSION));
		EList<Element> elements = testProfileModel.getOwnedElements();
//		for(if)
//		if(hasStereotype(elements.get(0), "WMSElemenmt"))
//			
//		System.out.println("get all elements:" + elements.size());
//
//		
//		org.eclipse.uml2.uml.Class wmsElement = (org.eclipse.uml2.uml.Class) testProfileModel
//				.getOwnedType("WMSElement");
//		if(wmsElement != null)
//			System.out.println("==");
	}

	private static String getModelPath() {
		Path currentRelativePath = Paths.get("");
		String currentPath = currentRelativePath.toAbsolutePath().toString() + "\\models";
		return "./models";
	}
	protected static org.eclipse.uml2.uml.Package load(URI uri) {
		org.eclipse.uml2.uml.Package package_ = null;

		try {
			// Load the requested resource
			Resource resource = RESOURCE_SET.getResource(uri, true);

			// Get the first (should be only) package from it
			package_ = (org.eclipse.uml2.uml.Package) EcoreUtil
				.getObjectByType(resource.getContents(),
					UMLPackage.Literals.PACKAGE);
		} catch (WrappedException we) {
			err(we.getMessage());
			System.exit(1);
		}

		return package_;
	}

	protected static void out(String format, Object... args) {
		if (DEBUG) {
			System.out.printf(format, args);
			if (!format.endsWith("%n")) {
				System.out.println();
			}
		}
	}

	protected static void err(String format, Object... args) {
		System.err.printf(format, args);
		if (!format.endsWith("%n")) {
			System.err.println();
		}
	}
	public final static Object getTaggedValue(Element umlElement, String stereotypeId, String taggedValue) {
		// Get the stereotype object
		Stereotype appliedStereotype = umlElement.getAppliedStereotype(stereotypeId);

		// Get the tagged value
		Object value = umlElement.getValue(appliedStereotype, taggedValue);
		
		// Return the tagged value
		return value;
	}
	
	public final static Boolean hasStereotype(Element umlElement, String stereotypeId) {
		// Check if stereotype is set for the element
		if (umlElement.getAppliedStereotype(stereotypeId) != null) return true;
		
		// Stereotype has not been set
		return false;
	}

	
}
