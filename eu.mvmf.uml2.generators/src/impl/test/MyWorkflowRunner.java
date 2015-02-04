package impl.test;

import java.util.HashMap;
import java.util.Map;

import org.eclipse.emf.mwe.core.WorkflowRunner;

public class MyWorkflowRunner {

	public static void main(String[] args) {
		String wfFile = "src\\workflow\\generator0.mwe";
		Map properties = new HashMap();
		Map slotContents = new HashMap();
		try {
			new WorkflowRunner().main(new String[] {"src/workflow/generator0.mwe"});
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

}
