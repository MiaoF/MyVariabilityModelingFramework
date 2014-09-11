/*
 * ModelHierarchy keeps a tree-like structure for to generate the model structures
 */
package magicdraw.feature.models;

import javax.swing.tree.TreePath;

import com.nomagic.magicdraw.openapi.uml.PresentationElementsManager;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;

import magicdraw.feature.models.details.AutomaticSmallPartsStoragePackage;
import magicdraw.feature.models.details.LargePartsStoragePackage;
import magicdraw.feature.models.details.ModelPackage;
import magicdraw.feature.models.details.PickByLightStoragePackage;
import magicdraw.feature.models.details.WarehouseModelType;

public class ModelHierarchy {

	private ModelPackage rootPackage;
	private final String ROOT_FEATURE_NAME = "New_SiemensWMS";
	private final String AKL_FEATURE_NAME = "SmallPartsStorage";
	private final String MPL_FEATURE_NAME = "LargePartsStorage";
	private final String BKS_FEATURE_NAME = "PickByLightStorage";

	public ModelHierarchy(TreePath[] checkedPaths, Diagram selectedDiagram) {
		rootPackage = new ModelPackage();
		rootPackage.setPackageName(ROOT_FEATURE_NAME);

		if (isfeatureSelected(checkedPaths, AKL_FEATURE_NAME)) {
			rootPackage
					.addNewSubPackage(new AutomaticSmallPartsStoragePackage());
		}
		if (isfeatureSelected(checkedPaths, MPL_FEATURE_NAME)) {
			rootPackage.addNewSubPackage(new LargePartsStoragePackage());
		}
		if (isfeatureSelected(checkedPaths, BKS_FEATURE_NAME)) {
			rootPackage.addNewSubPackage(new PickByLightStoragePackage());
		}
	}

	private boolean isfeatureSelected(TreePath[] checkedPaths, String string) {
		for (int i = 0; i < checkedPaths.length; i++) {
			if (checkedPaths[i].getLastPathComponent().toString()
					.equalsIgnoreCase(string))
				return true;
		}
		return false;
	}

	public ModelPackage getRootPackage() {
		return rootPackage;
	}

	public void setRootPackage(ModelPackage rootPackage) {
		this.rootPackage = rootPackage;
	}

}
