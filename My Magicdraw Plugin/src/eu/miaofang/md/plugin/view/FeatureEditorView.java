package eu.miaofang.md.plugin.view;

import java.awt.BorderLayout;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Enumeration;
import java.util.Iterator;
import java.util.List;

import javax.swing.JButton;
import javax.swing.JDialog;
import javax.swing.JOptionPane;
import javax.swing.JScrollPane;
import javax.swing.JTree;
import javax.swing.tree.TreePath;

import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.uml.BaseElement;
import com.nomagic.magicdraw.uml.symbols.DiagramPresentationElement;
import com.nomagic.magicdraw.uml.symbols.PresentationElement;
import com.nomagic.uml2.ext.jmi.helpers.StereotypesHelper;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Diagram;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Relationship;
import com.nomagic.uml2.ext.magicdraw.mdprofiles.Stereotype;

import eu.miaofang.md.plugin.control.ModelInstanceGenerator;

public class FeatureEditorView {
	private JScrollPane scrollPane;
	private CheckTreeManager checkTreeManager;
	private BaseElement selectedLayoutDiagram;
	private Stereotype requireStereoType;
	private Stereotype conflictStereoType;
	private JTree tree;
	private FeatureTreeNode rootNode;

	public FeatureEditorView(final Project project,
			final BaseElement selectedElement) {
		tree = InitializeTree((Element) selectedElement);
		// add a checkBox to every tree node
		checkTreeManager = new CheckTreeManager(tree);

		// This is the button to save configuration and create model or code
		// structure.
		JButton saveConfigurationButton = createButtonToSaveConfiguration(
				project, selectedElement);
		scrollPane = new JScrollPane(tree);

		JDialog dialog = new JDialog();
		dialog.getContentPane()
				.add(saveConfigurationButton, BorderLayout.SOUTH);
		dialog.getContentPane().add(scrollPane, BorderLayout.CENTER);
		dialog.setSize(300, 400);
		dialog.setTitle("Feature Configuration");
		dialog.setAlwaysOnTop(true);
		dialog.setVisible(true);
		checkTreeBoxForMandatoryFeatures(rootNode);
	}

	public FeatureEditorView(final Project project, final Element rootFeature,
			BaseElement selectedElementInMD) {
		this(project, rootFeature);
		selectedLayoutDiagram = (BaseElement) selectedElementInMD;
		bindKnownVariability();
	}

	private void checkTreeBoxForMandatoryFeatures(FeatureTreeNode node) {
		if (node.isMandatory()) {
			checkTreeManager.setTreeNodeSelectionByName(node.toString());
			Enumeration<FeatureTreeNode> childNodes = node.children();
			while (childNodes.hasMoreElements())
				checkTreeBoxForMandatoryFeatures(childNodes.nextElement());
		}
	}

	private String getNameToShow(Element magicdrawElement) {
		// getHumanName() in MagicDraw always contains the type of the element
		// e.g. Class ABC, the idea of this method is to remove the type and get
		// only "ABC"
		int firstSpace = magicdrawElement.getHumanName().indexOf(" ");
		String name = magicdrawElement.getHumanName().substring(firstSpace + 1);
		return name;
	}

	private void buildSubTreeWithFeatures(FeatureTreeNode nodeToAppend,
			Element aFeature, Element parentFeature) {
		FeatureTreeNode node;
		Collection<Relationship> relations = aFeature
				.get_relationshipOfRelatedElement();
		if (relations.size() > 1) {
			// if there is only one, it is the relations
			// to its parent. Then don't do anything.
			for (Relationship relation : aFeature
					.get_relationshipOfRelatedElement()) {
				if (requireStereoType == null && conflictStereoType == null) {
					Iterator<Element> itr = relation.getRelatedElement()
							.iterator();
					Element firstElement = itr.next();
					Element secondElement = itr.next();
					String firstName = getNameToShow(firstElement);
					String secondName = getNameToShow(secondElement);
					String parentName = "";
					if (parentFeature != null)
						parentName = getNameToShow(parentFeature);
					if (!firstName.equalsIgnoreCase(parentName)
							&& !secondName.equalsIgnoreCase(parentName)) {
						node = new FeatureTreeNode(getNameToShow(firstElement),
								firstElement);
						node.setIcon(FeatureOperatorIconHelper
								.getIconByStereoType(relation));
						buildSubTreeWithFeatures(node, firstElement, aFeature);
						nodeToAppend.add(node);
						Stereotype mandatoryStereoType = StereotypesHelper
								.getAppliedStereotypeByString(relation,
										"Mandatory");
						if (mandatoryStereoType != null) {
							node.setMandatory(true);
						}

					}
				}
			}
		}
	}

	public void bindKnownVariability() {
		Diagram diagram = (Diagram) selectedLayoutDiagram;
		DiagramPresentationElement diagramPresentationElement = Project
				.getProject(diagram).getDiagram(diagram);
		for (PresentationElement e : diagramPresentationElement
				.getPresentationElements()) {
			checkCorrespondingTreeNode(e);
		}

	}

	private void checkCorrespondingTreeNode(PresentationElement e) {
		Element elementToCheckInTree = e.getElement();
		if (elementToCheckInTree != null) {
			Stereotype type = StereotypesHelper.getStereotypes(
					elementToCheckInTree).get(0);
			// have to remove "stereotype " from the human name
			checkTreeManager.setTreeNodeSelectionByName(getNameToShow(type));
		}
	}

	private JTree InitializeTree(final Element rootFeature) {
		rootNode = new FeatureTreeNode(getNameToShow(rootFeature), rootFeature);
		rootNode.setMandatory(true);
		buildSubTreeWithFeatures(rootNode, rootFeature, null);
		JTree tree = new JTree(rootNode);
		tree.setCellRenderer(new IconRenderer());
		tree.setEditable(false);
		return tree;
	}

	private JButton createButtonToSaveConfiguration(final Project project,
			final BaseElement selectedElement) {
		JButton okButton = new JButton("Create Modeling Space");
		okButton.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				TreePath checkedPaths[] = checkTreeManager.getSelectionModel()
						.getSelectionPaths();
				ModelInstanceGenerator modelCreator = new ModelInstanceGenerator(
						project, selectedLayoutDiagram);
//				JOptionPane.showMessageDialog(null, checkedPaths.length, "", JOptionPane.ERROR_MESSAGE); 
				modelCreator.createModelStructureInMagicdraw(checkedPaths);
			}
		});
		return okButton;
	}
}
