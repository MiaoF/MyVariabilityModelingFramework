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
import org.eclipse.uml2.uml.PrimitiveType;
import org.eclipse.uml2.uml.Profile;
import org.eclipse.uml2.uml.Property;
import org.eclipse.uml2.uml.Stereotype;
import org.eclipse.uml2.uml.Type;
import org.eclipse.uml2.uml.UMLFactory;
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

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		String modelPath = getModelPath();

		Model testProfileModel = (Model) load(URI.createURI(modelPath)
				.appendSegment("TestProfiles")
				.appendFileExtension(UMLResource.FILE_EXTENSION));
		Profile wmsTopologyProfile = (Profile) load(URI
				.createURI(modelPath)
				.appendSegment(
						"AE_Phase_Configuration_Meta-Model.Topology_Configuration_Editor_Profile.profile")
				.appendFileExtension(UMLResource.FILE_EXTENSION));

		PrimitiveType stringPrimitiveType = importPrimitiveType(
				wmsTopologyProfile, "String");

		EList<Element> elements = testProfileModel.getOwnedElements();
		for (Element e : elements) {
			if(hasStereotype(e, "WMSElement"))
				System.out.println("YES");
			EList<Element> childElements = e.getOwnedElements();
			System.out.println(childElements.size());
			if (childElements.size() > 10) {
				for (Element grandChild : childElements) {
					NamedElement myElement = (NamedElement) grandChild;
					System.out.println("I am checking the no. of stereotypes: " + myElement.getName());
					
//					EList<Stereotype> stereotypeList = wmsTopologyProfile
//							.getApplicableStereotypes();
//					for (Stereotype stereotype : stereotypeList) {
//						System.out.println(stereotype.getName());

						// Property isCritical =
						// wmsElementType.getAttribute("",
						// stringPrimitiveType);
						// String value = (String)
						// getStereotypePropertyValue((NamedElement)
						// childElements.get(5), wmsElementType, isCritical);
						// System.out.println(value);
					// }
				}

			}
		}

		System.out.println("get all elements:" + elements.size());

		org.eclipse.uml2.uml.Class wmsElement = (org.eclipse.uml2.uml.Class) testProfileModel
				.getOwnedType("WMSElement");
		if (wmsElement != null)
			System.out.println("==" + wmsElement.getName());
	}

	protected static Object getStereotypePropertyValue(
			NamedElement namedElement, Stereotype stereotype, Property property) {
		Object value = namedElement.getValue(stereotype, property.getName());

		out("Value of stereotype property '%s' on element '%s' is %s.",
				property.getQualifiedName(), namedElement.getQualifiedName(),
				value);

		return value;
	}

	private static String getModelPath() {
		Path currentRelativePath = Paths.get("");
		String currentPath = currentRelativePath.toAbsolutePath().toString()
				+ "\\models";
		return "./models/v4.x";
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

	public final static Object getTaggedValue(Element umlElement,
			String stereotypeId, String taggedValue) {
		// Get the stereotype object
		Stereotype appliedStereotype = umlElement
				.getAppliedStereotype(stereotypeId);

		// Get the tagged value
		Object value = umlElement.getValue(appliedStereotype, taggedValue);

		// Return the tagged value
		return value;
	}

	public final static Boolean hasStereotype(Element umlElement,
			String stereotypeId) {
		// Check if stereotype is set for the element
		if (umlElement.getAppliedStereotype(stereotypeId) != null)
			return true;

		// Stereotype has not been set
		return false;
	}

	protected static Profile createProfile(String name, String nsURI) {
		Profile profile = UMLFactory.eINSTANCE.createProfile();
		profile.setName(name);
		profile.setURI(nsURI);

		out("Profile '%s' created.", profile.getQualifiedName());

		return profile;
	}

	protected static PrimitiveType importPrimitiveType(
			org.eclipse.uml2.uml.Package package_, String name) {

		org.eclipse.uml2.uml.Package umlLibrary = (org.eclipse.uml2.uml.Package) load(URI
				.createURI(UMLResource.UML_PRIMITIVE_TYPES_LIBRARY_URI));

		PrimitiveType primitiveType = (PrimitiveType) umlLibrary
				.getOwnedType(name);

		package_.createElementImport(primitiveType);

		out("Primitive type '%s' imported.", primitiveType.getQualifiedName());

		return primitiveType;
	}
}
