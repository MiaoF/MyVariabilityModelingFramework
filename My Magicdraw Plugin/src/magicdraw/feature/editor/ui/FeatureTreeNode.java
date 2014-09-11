package magicdraw.feature.editor.ui;

import javax.swing.ImageIcon;
import javax.swing.tree.DefaultMutableTreeNode;

import com.jidesoft.swing.TristateCheckBox;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;

public class FeatureTreeNode extends DefaultMutableTreeNode {

	private Element featureElenment;
	private static final long serialVersionUID = 1L;
	private ImageIcon icon ;
	private TristateCheckBox checkBox ;
	
	public FeatureTreeNode(String name, Element e) {
		super(name);
		this.setFeatureElenment(e);
	}
	public Element getFeatureElenment() {
		return featureElenment;
	}
	public void setFeatureElenment(Element featureElenment) {
		this.featureElenment = featureElenment;
	}
	public ImageIcon getIcon() {
		return icon;
	}
	public void setIcon(ImageIcon icon) {
		this.icon = icon;
	}
	public TristateCheckBox getCheckBox() {
		return checkBox;
	}
	public void setCheckBox(TristateCheckBox checkBox) {
		this.checkBox = checkBox;
	}
}
