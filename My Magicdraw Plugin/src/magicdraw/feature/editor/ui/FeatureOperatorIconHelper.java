package magicdraw.feature.editor.ui;

import javax.swing.ImageIcon;

import com.nomagic.uml2.ext.jmi.helpers.StereotypesHelper;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Relationship;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Stereotype;

public class FeatureOperatorIconHelper {
	private static ImageIcon mandatoryIcon = new ImageIcon(FeatureOperatorIconHelper.class.getResource("/images/Mandatory_16.png"));
	private static ImageIcon optionalIcon = new ImageIcon(FeatureOperatorIconHelper.class.getResource("/images/Optional_16.png"));
	private static ImageIcon alternativeIcon = new ImageIcon(FeatureOperatorIconHelper.class.getResource("/images/Alternative_16.png"));
	private static ImageIcon orIcon = new ImageIcon(FeatureOperatorIconHelper.class.getResource("/images/Or_16.png"));
	
	public static ImageIcon getIconByStereoTyoe(Relationship relation) {
		Stereotype mandatoryStereoType = StereotypesHelper
				.getAppliedStereotypeByString(relation, "Mandatory");
		if(mandatoryStereoType != null)
			return mandatoryIcon;
		
		Stereotype optinalStereoType = StereotypesHelper
				.getAppliedStereotypeByString(relation, "Optional");
		if(optinalStereoType != null)
			return optionalIcon;
		
		Stereotype alternativeStereoType = StereotypesHelper.getAppliedStereotypeByString(
				relation, "Alternative");
		if(alternativeStereoType != null)
			return alternativeIcon;
		
		Stereotype orStereoType = StereotypesHelper
				.getAppliedStereotypeByString(relation, "Or");
		if(orStereoType != null)
			return orIcon;
		
		return null;
		
	}

}
