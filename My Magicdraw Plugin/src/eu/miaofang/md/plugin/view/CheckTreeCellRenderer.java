package eu.miaofang.md.plugin.view;

import java.awt.BorderLayout;
import java.awt.Component;

import javax.swing.JPanel;
import javax.swing.JTree;
import javax.swing.tree.TreeCellRenderer;
import javax.swing.tree.TreePath;

import com.jidesoft.swing.TristateCheckBox;

public class CheckTreeCellRenderer extends JPanel implements TreeCellRenderer {

	/**
	 * 
	 */
	private static final long serialVersionUID = -3235059604137857885L;
	private CheckTreeSelectionModel selectionModel;
	private TreeCellRenderer delegate;
	private TristateCheckBox checkBox = new TristateCheckBox();

	public CheckTreeCellRenderer(TreeCellRenderer treeCellRenderer,
			CheckTreeSelectionModel selectionModel) {
		this.delegate = treeCellRenderer;
		this.selectionModel = selectionModel;
		setLayout(new BorderLayout());
		setOpaque(false);
		checkBox.setOpaque(false);
	}

	public Component getTreeCellRendererComponent(JTree tree, Object value,
			boolean selected, boolean expanded, boolean leaf, int row,
			boolean hasFocus) {
		Component renderer = delegate.getTreeCellRendererComponent(tree, value,
				selected, expanded, leaf, row, hasFocus);

		TreePath path = tree.getPathForRow(row);
		if (path != null) {
			if (selectionModel.isPathSelected(path, false)) {
				checkBox.setSelected(Boolean.TRUE);
			} else if (selectionModel.isPartiallySelected(path)) {
				// checkBox.setSelected(selectionModel.isPartiallySelected(path)
				// ? null : Boolean.FALSE);
				checkBox.setSelected(Boolean.TRUE);
			} else {
				checkBox.setSelected(Boolean.FALSE);
			}

		}
		
		removeAll();
		add(checkBox, BorderLayout.WEST);
		add(renderer, BorderLayout.CENTER);
		FeatureTreeNode theNode = (FeatureTreeNode) value;
		theNode.setCheckBox(checkBox);
		return this;
	}
}
