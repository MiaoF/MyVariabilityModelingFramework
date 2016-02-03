package eu.miaofang.md.plugin.model.component;

import eu.miaofang.wms.plugin.config.PlugInConstants;

public class AtomicComponent extends WmsComponent{
	
	public AtomicComponent(String name) {
		super(name);
		componentType = PlugInConstants.ATOMIC_COMPONENT;
	}

}
