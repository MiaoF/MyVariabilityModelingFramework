package eu.miaofang.md.plugin.init;

import java.io.File;

import com.nomagic.magicdraw.core.Application;
import com.nomagic.magicdraw.core.Project;
import com.nomagic.magicdraw.core.project.ProjectDescriptor;
import com.nomagic.magicdraw.core.project.ProjectDescriptorsFactory;
import com.nomagic.magicdraw.core.project.ProjectsManager;

public class ModuleLoader {
	public ModuleLoader(Project project) {
		String moduleFilePath = "";
		ProjectsManager projectsManager = Application.getInstance()
				.getProjectsManager();
		File file = new File(moduleFilePath);
		ProjectDescriptor projectDescriptor = ProjectDescriptorsFactory
				.createProjectDescriptor(file.toURI());
		projectsManager.reloadModule(project, projectDescriptor);
	}
}
