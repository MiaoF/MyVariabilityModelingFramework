package eu.miaofang.md.plugin.view;

import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;
import java.util.ArrayList;
import java.util.List;

import javax.swing.JCheckBox;
import javax.swing.JTree;
import javax.swing.event.TreeSelectionEvent;
import javax.swing.event.TreeSelectionListener;
import javax.swing.tree.TreeNode;
import javax.swing.tree.TreePath;


public class CheckTreeManager extends MouseAdapter implements
		TreeSelectionListener {

	private CheckTreeSelectionModel selectionModel; 
    private JTree tree = new JTree(); 
    int hotspot = new JCheckBox().getPreferredSize().width;
 
    public CheckTreeManager(JTree tree){ 
        this.tree = tree; 
        selectionModel = new CheckTreeSelectionModel(tree.getModel()); 
        tree.setCellRenderer(new CheckTreeCellRenderer(tree.getCellRenderer(), selectionModel)); 
        tree.addMouseListener(this); 
        selectionModel.addTreeSelectionListener(this); 
    } 
 
    public void mouseClicked(MouseEvent me){ 
        TreePath path = tree.getPathForLocation(me.getX(), me.getY()); 
        if(path==null) 
            return; 
        if(me.getX()>tree.getPathBounds(path).x+hotspot) 
            return; 
        boolean selected = selectionModel.isPathSelected(path, false); 
        selectionModel.removeTreeSelectionListener(this); 
        try{ 
            if(selected) 
                selectionModel.removeSelectionPath(path); 
            else 
                selectionModel.addSelectionPath(path); 
        } finally{ 
            selectionModel.addTreeSelectionListener(this); 
            tree.treeDidChange(); 
        } 
        
    } 
 
    public CheckTreeSelectionModel getSelectionModel(){ 
        return selectionModel; 
    } 
 
    public void valueChanged(TreeSelectionEvent e){ 
        tree.treeDidChange(); 
    }

	public void setTreeNodeSelectionByName(String elementName) {
		TreePath path = null;
		FeatureTreeNode root = (FeatureTreeNode) selectionModel.getModel().getRoot();
		FeatureTreeNode node = findFeatureWithTheSameName(root, elementName);
		if(node != null) {
			path = getPath(node);
			boolean selected = selectionModel.isPathSelected(path, false); 
			selectionModel.removeTreeSelectionListener(this); 
	        try{ 
	            if(!selected)
	                selectionModel.addSelectionPath(path); 
	        } finally{ 
	            selectionModel.addTreeSelectionListener(this); 
	            tree.treeDidChange(); 
	        }
		}        
	} 
	
	private FeatureTreeNode findFeatureWithTheSameName(FeatureTreeNode treeNode, String elmentName) {
		if(elmentName.contains(treeNode.toString()))
			return treeNode;
		int numberOfChildren = treeNode.getChildCount();
		for(int i = 0; i< numberOfChildren; i++) {
			FeatureTreeNode child = (FeatureTreeNode) treeNode.getChildAt(i);
			if(elmentName.contains(child.toString()))
				return child;
			if(child.getChildCount() > 0) {
				FeatureTreeNode node = findFeatureWithTheSameName(child, elmentName);
				if(node != null)
					return node;
			}
		}
		return null;
	}

	private TreePath getPath(TreeNode treeNode) {
	    List<Object> nodes = new ArrayList<Object>();
	    if (treeNode != null) {
	      nodes.add(treeNode);
	      treeNode = treeNode.getParent();
	      while (treeNode != null) {
	        nodes.add(0, treeNode);
	        treeNode = treeNode.getParent();
	      }
	    }
	    return nodes.isEmpty() ? null : new TreePath(nodes.toArray());
	  }
}
