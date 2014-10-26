package eu.miaofang.md.plugin.view;

import javax.swing.tree.DefaultTreeSelectionModel;
import javax.swing.tree.TreeModel;
import javax.swing.tree.TreePath;
import javax.swing.tree.TreeSelectionModel;

public class CheckTreeSelectionModel extends DefaultTreeSelectionModel {

	private static final long serialVersionUID = -3030117501567974691L;
	
	private TreeModel model; 
	 
    public CheckTreeSelectionModel(TreeModel model){ 
        this.model = model; 
        setSelectionMode(TreeSelectionModel.DISCONTIGUOUS_TREE_SELECTION); 
    } 
 
    public TreeModel getModel() {
		return model;
	}

    public boolean isPartiallySelected(TreePath path){ 
        return false; 
    } 
 
    public boolean isPathSelected(TreePath path, boolean dig){ 
        if(!dig) 
            return super.isPathSelected(path); 
        while(path!=null && !super.isPathSelected(path)) 
            path = path.getParentPath(); 
        return path!=null; 
    }
 
    public void addSelectionPath(TreePath path) {
    	super.addSelectionPath(path);
    	while(path!=null && isPathSelected(path, false)) {
    		path = path.getParentPath();
    		super.addSelectionPath(path); 
    	}
    }
   
 
    public void addSelectionPaths(TreePath[] paths){ 
        //select all the parent path
        for(int i = 0; i<paths.length; i++){
        	TreePath path = paths[i];
        	super.addSelectionPaths(new TreePath[]{ path}); 
        	while(path!=null && !super.isPathSelected(path)) {
        		path = path.getParentPath();
        		super.addSelectionPaths(new TreePath[]{path});
        	}		
        }
    }
}
