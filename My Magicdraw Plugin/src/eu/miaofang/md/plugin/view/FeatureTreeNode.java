package eu.miaofang.md.plugin.view;

import javax.swing.ImageIcon;
import javax.swing.tree.DefaultMutableTreeNode;

import com.jidesoft.swing.TristateCheckBox;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;

public class FeatureTreeNode extends DefaultMutableTreeNode {

	private Element featureElenment;
	private static final long serialVersionUID = 1L;
	private ImageIcon icon ;
	private TristateCheckBox checkBox;
	private boolean isMandatory = false;
	
	public boolean isMandatory() {
		return isMandatory;
	}
	public void setMandatory(boolean isMandatory) {
		this.isMandatory = isMandatory;
	}
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
