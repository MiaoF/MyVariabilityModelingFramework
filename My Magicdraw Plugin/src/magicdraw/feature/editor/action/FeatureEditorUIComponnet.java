package magicdraw.feature.editor.action;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Component;
import java.awt.Font;
import java.awt.GridLayout;
import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import java.awt.event.MouseEvent;
import java.util.Collection;
import java.util.EventObject;
import java.util.Vector;

import javax.swing.AbstractCellEditor;
import javax.swing.BorderFactory;
import javax.swing.JCheckBox;
import javax.swing.JDialog;
import javax.swing.JFrame;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.JTree;
import javax.swing.UIManager;
import javax.swing.event.ChangeEvent;
import javax.swing.tree.DefaultMutableTreeNode;
import javax.swing.tree.DefaultTreeCellRenderer;
import javax.swing.tree.TreeCellEditor;
import javax.swing.tree.TreeCellRenderer;
import javax.swing.tree.TreePath;

import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Relationship;

public class FeatureEditorUIComponnet {
	private JTree tree;
	private Vector rootVector;

	public FeatureEditorUIComponnet(Element rootFeature) {
		rootFeature.get_relationshipOfRelatedElement();
		Collection<Relationship> subFeatures = rootFeature.get_relationshipOfRelatedElement();
		
		Object rootNodes[] = { rootFeature.getHumanName() };
		Vector rootVector = new NamedVector("Root", rootNodes);
		CheckBoxNode browsingOptions[] = {
				new CheckBoxNode("Notify when downloads complete", true),
				new CheckBoxNode("Disable script debugging", true),
				new CheckBoxNode("Use AutoComplete", true),
				new CheckBoxNode("Browse in a new process", false) };

		Vector browseVector = new NamedVector("Browsing", browsingOptions);
		tree = new JTree(rootVector);
		
	}
	public static void main(final String args[]) {
        final DefaultMutableTreeNode root = new DefaultMutableTreeNode(new CheckBoxNodeData("Root",true));

        final DefaultMutableTreeNode accessibility =
            add(root, "Accessibility", true);
        add(accessibility, "Move system caret with focus/selection changes", false);
        add(accessibility, "Always expand alt text for images", true);
        root.add(accessibility);

        final DefaultMutableTreeNode browsing =
            new DefaultMutableTreeNode(new CheckBoxNodeData("Browsing", null));
        add(browsing, "Notify when downloads complete", true);
        add(browsing, "Disable script debugging", true);
        add(browsing, "Use AutoComplete", true);
        add(browsing, "Browse in a new process", false);
        root.add(browsing);

        final CheckBoxTree tree = new CheckBoxTree(root);
        ((DefaultMutableTreeNode)tree.getModel().getRoot()).add(new DefaultMutableTreeNode(new CheckBoxNodeData("gggg", null)));
        ((DefaultTreeModel)tree.getModel()).reload();
        // listen for changes in the selection
        tree.addTreeSelectionListener(new TreeSelectionListener() {
            @Override
            public void valueChanged(final TreeSelectionEvent e) {
                //System.out.println("selection changed");
            }
        });
        // show the tree on screen
        final JFrame frame = new JFrame("CheckBox Tree");
        final JScrollPane scrollPane = new JScrollPane(tree);
        frame.getContentPane().add(scrollPane, BorderLayout.CENTER);
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setSize(300, 150);
        frame.setVisible(true);
    }

	
	public void open() {
		JDialog dialog = new JDialog();
		CheckBoxNodeRenderer renderer = new CheckBoxNodeRenderer();
		tree.setCellRenderer(renderer);

		tree.setCellEditor(new CheckBoxNodeEditor(tree));
		tree.setEditable(true);

		JScrollPane scrollPane = new JScrollPane(tree);
		dialog.getContentPane().add(scrollPane, BorderLayout.CENTER);
		dialog.setSize(300, 400);
		dialog.setTitle("Feature Configuration");
		dialog.setVisible(true);
		
	}
}

class CheckBoxNodeRenderer implements TreeCellRenderer {
	private JCheckBox leafRenderer = new JCheckBox();

	private DefaultTreeCellRenderer nonLeafRenderer = new DefaultTreeCellRenderer();

	Color selectionBorderColor, selectionForeground, selectionBackground,
			textForeground, textBackground;

	protected JCheckBox getLeafRenderer() {
		return leafRenderer;
	}

	public CheckBoxNodeRenderer() {
		Font fontValue;
		fontValue = UIManager.getFont("Tree.font");
		if (fontValue != null) {
			leafRenderer.setFont(fontValue);
		}
		Boolean booleanValue = (Boolean) UIManager
				.get("Tree.drawsFocusBorderAroundIcon");
		leafRenderer.setFocusPainted((booleanValue != null)
				&& (booleanValue.booleanValue()));

		selectionBorderColor = UIManager.getColor("Tree.selectionBorderColor");
		selectionForeground = UIManager.getColor("Tree.selectionForeground");
		selectionBackground = UIManager.getColor("Tree.selectionBackground");
		textForeground = UIManager.getColor("Tree.textForeground");
		textBackground = UIManager.getColor("Tree.textBackground");
	}

	public Component getTreeCellRendererComponent(JTree tree, Object value,
			boolean selected, boolean expanded, boolean leaf, int row,
			boolean hasFocus) {

		Component returnValue;
		if (leaf) {

			String stringValue = tree.convertValueToText(value, selected,
					expanded, leaf, row, false);
			leafRenderer.setText(stringValue);
			leafRenderer.setSelected(false);

			leafRenderer.setEnabled(tree.isEnabled());

			if (selected) {
				leafRenderer.setForeground(selectionForeground);
				leafRenderer.setBackground(selectionBackground);
			} else {
				leafRenderer.setForeground(textForeground);
				leafRenderer.setBackground(textBackground);
			}

			if ((value != null) && (value instanceof DefaultMutableTreeNode)) {
				Object userObject = ((DefaultMutableTreeNode) value)
						.getUserObject();
				if (userObject instanceof CheckBoxNode) {
					CheckBoxNode node = (CheckBoxNode) userObject;
					leafRenderer.setText(node.getText());
					leafRenderer.setSelected(node.isSelected());
				}
			}
			returnValue = leafRenderer;
		} else {
			returnValue = nonLeafRenderer.getTreeCellRendererComponent(tree,
					value, selected, expanded, leaf, row, hasFocus);
		}
		return returnValue;
	}
}

class CheckBoxNodeEditor extends AbstractCellEditor implements TreeCellEditor {

	CheckBoxNodeRenderer renderer = new CheckBoxNodeRenderer();

	ChangeEvent changeEvent = null;

	JTree tree;

	public CheckBoxNodeEditor(JTree tree) {
		this.tree = tree;
	}

	public Object getCellEditorValue() {
		JCheckBox checkbox = renderer.getLeafRenderer();
		CheckBoxNode checkBoxNode = new CheckBoxNode(checkbox.getText(),
				checkbox.isSelected());
		return checkBoxNode;
	}

	public boolean isCellEditable(EventObject event) {
		boolean returnValue = false;
		if (event instanceof MouseEvent) {
			MouseEvent mouseEvent = (MouseEvent) event;
			TreePath path = tree.getPathForLocation(mouseEvent.getX(),
					mouseEvent.getY());
			if (path != null) {
				Object node = path.getLastPathComponent();
				if ((node != null) && (node instanceof DefaultMutableTreeNode)) {
					DefaultMutableTreeNode treeNode = (DefaultMutableTreeNode) node;
					Object userObject = treeNode.getUserObject();
					returnValue = ((treeNode.isLeaf()) && (userObject instanceof CheckBoxNode));
				}
			}
		}
		return returnValue;
	}

	public Component getTreeCellEditorComponent(JTree tree, Object value,
			boolean selected, boolean expanded, boolean leaf, int row) {

		Component editor = renderer.getTreeCellRendererComponent(tree, value,
				true, expanded, leaf, row, true);

		// editor always selected / focused
		ItemListener itemListener = new ItemListener() {
			public void itemStateChanged(ItemEvent itemEvent) {
				if (stopCellEditing()) {
					fireEditingStopped();
				}
			}
		};
		if (editor instanceof JCheckBox) {
			((JCheckBox) editor).addItemListener(itemListener);
		}

		return editor;
	}
}

class CheckBoxNode {
	String text;

	boolean selected;

	public CheckBoxNode(String text, boolean selected) {
		this.text = text;
		this.selected = selected;
	}

	public boolean isSelected() {
		return selected;
	}

	public void setSelected(boolean newValue) {
		selected = newValue;
	}

	public String getText() {
		return text;
	}

	public void setText(String newValue) {
		text = newValue;
	}

	public String toString() {
		return getClass().getName() + "[" + text + "/" + selected + "]";
	}
}

class NamedVector extends Vector {
	String name;

	public NamedVector(String name) {
		this.name = name;
	}

	public NamedVector(String name, Object elements[]) {
		this.name = name;
		for (int i = 0, n = elements.length; i < n; i++) {
			add(elements[i]);
		}
	}

	public String toString() {
		return "[" + name + "]";
	}

}
