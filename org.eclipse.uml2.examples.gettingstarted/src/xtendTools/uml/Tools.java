package xtendTools.uml;

import org.eclipse.uml2.uml.Element;
import org.eclipse.uml2.uml.Stereotype;

/**
 * Tools for handling UML classes
 * 
 * @author kuhn
 */
public class Tools {

		/**
		 * Return true if a specific stereotype is applied to an UML element
		 * 
		 * @param umlElement The UML element that is checked for a stereotype
		 * @param stereotypeId Full qualified name of the stereotype (ProfileName::StereotypeName)
		 * @return True if the stereotype is applied to the element, false otherwise
		 */
		public final static Boolean hasStereotype(Element umlElement, String stereotypeId) {
			// Check if stereotype is set for the element
			if (umlElement.getAppliedStereotype(stereotypeId) != null) return true;
			
			// Stereotype has not been set
			return false;
		}

		
		/**
		 * Get a tagged value from a stereotyped element
		 * 
		 * @param umlElement The UML element that has the stereotype applied
		 * @param stereotypeId Full qualified name of the stereotype (ProfileName::StereotypeName)
		 * @param taggedValue Name of the tagged value
		 * @return The tagged value as Object
		 */
		public final static Object getTaggedValue(Element umlElement, String stereotypeId, String taggedValue) {
			// Get the stereotype object
			Stereotype appliedStereotype = umlElement.getAppliedStereotype(stereotypeId);

			// Get the tagged value
			Object value = umlElement.getValue(appliedStereotype, taggedValue);
			
			// Return the tagged value
			return value;
		}
}
