package eu.miaofang.md.plugin.model.component;

public class WmsComponent {
	protected String componentName = "";
	protected String componentType = "";
	protected WmsComponent parentComponent = null;
	protected boolean isFeatureVariabile = false;
	protected boolean isProcessVariabile = false;
	protected boolean isTopologyVariabile = false;
	
	
	public WmsComponent() {

	}
	public WmsComponent(String name) {
		componentName = name;
	}
	
	public String getComponentName() {
		return componentName;
	}

	public void setComponentName(String componentName) {
		this.componentName = componentName;
	}

	public WmsComponent getParentComponent() {
		return parentComponent;
	}

	public void setParentComponent(WmsComponent parentComponent) {
		this.parentComponent = parentComponent;
	}

	

	public String getComponentType() {
		return componentType;
	}

	public void setComponentType(String componentType) {
		this.componentType = componentType;
	}

	public Object getModelStructure() {
		// TODO Auto-generated method stub
		return null;
	}

}
