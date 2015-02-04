package impl.plugin.md;

import com.nomagic.actions.ActionsManager;
import com.nomagic.magicdraw.actions.BrowserContextAMConfigurator;
import com.nomagic.magicdraw.ui.browser.Tree;

import com.nomagic.actions.ActionsCategory;
import com.nomagic.actions.NMAction;
import com.nomagic.magicdraw.actions.MDActionsCategory;
import com.nomagic.actions.AMConfigurator;

public class ModelExportContextConfigurator implements BrowserContextAMConfigurator {
 
    public final static String EXPORT_PLUGIN_MENU_CATEGORY = "WMS Derivation";
    public final static String  EXPORT_MENU_ID = "Export Model to Folder";

    private NMAction action;

    public ModelExportContextConfigurator(NMAction nmAction) {
        this.action = nmAction;
    }

    public void configure(ActionsManager mngr, Tree tree) {

        ActionsCategory category = (ActionsCategory) mngr.getActionFor(EXPORT_MENU_ID);
        if (category == null) {
            category = new MDActionsCategory(EXPORT_MENU_ID, EXPORT_PLUGIN_MENU_CATEGORY);
            category.setNested(true);
            mngr.addCategory(category);
        }
        category.addAction(action);

    }
    
    public int getPriority() {
        return AMConfigurator.MEDIUM_PRIORITY;
    }

}