package eu.miaofang.md.plugin.model;

public class WmsModel {
	
	private String modelName;
	private String modelType;
	private String modelTemplate = null;
	
	public WmsModel(String name, String diagramType) {
		modelName = name;
		modelType = diagramType;
	}
	
	public WmsModel(String name, String diagramType, String template) {
		modelName = name;
		modelType = diagramType;
		modelTemplate = template;
	}
	public String getModelName() {
		return modelName;
	}
	public void setModelName(String modelName) {
		this.modelName = modelName;
	}
	public String getModelType() {
		return modelType;
	}
	public void setModelType(String modelType) {
		this.modelType = modelType;
	}
	public String getModelTemplate() {
		return modelTemplate;
	}
	public void setModelTemplate(String modelTemplate) {
		this.modelTemplate = modelTemplate;
	}
	
	

}
