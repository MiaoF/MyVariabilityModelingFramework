package eu.miaofang.md.examplePlugin;

import java.awt.event.ActionEvent;

import javax.swing.JOptionPane;

import com.nomagic.magicdraw.ui.browser.actions.DefaultBrowserAction;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;


/**
 * This class defines the behavior of the action in your plugin.
 * 
 * @author Xavier Seignard
 */
public class SimplePluginAction extends DefaultBrowserAction {
    // Attributes

    // Serial version UID.
    private static final long serialVersionUID = 1L;

    // The name of the action which will be displayed in MagicDraw
    private static final String SIMPLE_PLUGIN_ACTION_NAME = "Simple Plugin Action";
    // The id of the action which will be displayed in MagicDraw
    private static final String SIMPLE_PLUGIN_ACTION_ID = "SimplePluginActionID";

    /**
     * Default constructor.
     * 
     * @throws Exception if the super(...) call fails
     */
    public SimplePluginAction() throws Exception {
        super(SIMPLE_PLUGIN_ACTION_ID, SIMPLE_PLUGIN_ACTION_NAME, null, null);
    }

    // Methods
    /**
     * Here you define what does your action
     * 
     * @param actionEvent the triggering event
     */
    public void actionPerformed(ActionEvent actionEvent) {
        SimplePluginBehavior simplePluginBehavior = new SimplePluginBehavior();
        CountingVisitor countingVisitor = simplePluginBehavior.getVisitor();
        
        // Clear the map for a clean count
        countingVisitor.getMap().clear();
        
        // Visit the children for counting elements of each type
        simplePluginBehavior.visitChildren((Element) getSelectedObject());
        
        // Display the counting results
        String result = simplePluginBehavior.returnResults(countingVisitor.getMap());
        JOptionPane.showMessageDialog(null, result);
    }

    /**
     * Defines when your action is available.
     */
    public void updateState() {
        // This action is only available when your click on an instance of Element in the containement tree
        if (this.getSelectedObject() != null) {
            if (this.getSelectedObject() instanceof Element) {
                setEnabled(true);
            }
            else {
                setEnabled(false);
            }
        }
        else {
            setEnabled(false);
        }
    }
}
