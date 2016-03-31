package eu.miaofang.md.plugin.report;

import com.nomagic.actions.ActionsManager;
import com.nomagic.magicdraw.actions.BrowserContextAMConfigurator;
import com.nomagic.magicdraw.ui.browser.Tree;

import com.nomagic.actions.ActionsCategory;
import com.nomagic.actions.NMAction;
import com.nomagic.magicdraw.actions.MDActionsCategory;
import com.nomagic.actions.AMConfigurator;

public class AnotherBrowserContextConfigurator implements BrowserContextAMConfigurator {
 
    public final static String REPORTER_PLUGIN_MENU_CATEGORY = "IAM Report Status";
    public final static String REPORTER_MENU_ID = "ModelStatusReporter";

    private NMAction action;

    public AnotherBrowserContextConfigurator(NMAction nmAction) {
        this.action = nmAction;
    }

    public void configure(ActionsManager mngr, Tree tree) {

        ActionsCategory category = (ActionsCategory) mngr.getActionFor(REPORTER_MENU_ID);
        if (category == null) {
            category = new MDActionsCategory(REPORTER_MENU_ID, REPORTER_PLUGIN_MENU_CATEGORY);
            category.setNested(true);
            mngr.addCategory(category);

        }
        category.addAction(action);
    }
    
    public int getPriority() {
        return AMConfigurator.MEDIUM_PRIORITY;
    }

}