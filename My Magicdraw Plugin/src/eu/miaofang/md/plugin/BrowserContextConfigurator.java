package eu.miaofang.md.plugin;

import com.nomagic.actions.ActionsManager;
import com.nomagic.magicdraw.actions.BrowserContextAMConfigurator;
import com.nomagic.magicdraw.ui.browser.Tree;

import com.nomagic.actions.ActionsCategory;
import com.nomagic.actions.NMAction;
import com.nomagic.magicdraw.actions.MDActionsCategory;
import com.nomagic.actions.AMConfigurator;

public class BrowserContextConfigurator implements BrowserContextAMConfigurator {
 
    public final static String FEATURE_PLUGIN_MENU_CATEGORY = "IAM Variability Modeling";
    public final static String FEATURE_PLUGIN_MENU_ID = "FeatureEditor";

    private NMAction action;

    public BrowserContextConfigurator(NMAction nmAction) {
        this.action = nmAction;
    }

    public void configure(ActionsManager mngr, Tree tree) {

        ActionsCategory category = (ActionsCategory) mngr.getActionFor(FEATURE_PLUGIN_MENU_ID);
        if (category == null) {
            category = new MDActionsCategory(FEATURE_PLUGIN_MENU_ID, FEATURE_PLUGIN_MENU_CATEGORY);
            category.setNested(true);
            mngr.addCategory(category);
        }
        category.addAction(action);

   }
    
    public int getPriority() {
        return AMConfigurator.MEDIUM_PRIORITY;
    }

}