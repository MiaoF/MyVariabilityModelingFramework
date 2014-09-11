package magicdraw.feature.models.details;

public class WarehouseModelType {
	
	private String modelName;
	private String modelType;
	private String modelTemplate;
	
	public WarehouseModelType(String name, String diagramType) {
		modelName = name;
		modelType = diagramType;
		// TODO Auto-generated constructor stub
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
