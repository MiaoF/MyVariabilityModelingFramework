package eu.miaofang.md.plugin.report;

import java.awt.BorderLayout;
import java.awt.Component;
import java.awt.Dimension;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;

import javax.swing.JFrame;
import javax.swing.JScrollPane;
import javax.swing.JTable;
import javax.swing.table.TableCellRenderer;

public class ReportTable {
	
	public ReportTable() {
		JFrame frame = new JFrame();
		Object[][] modelInfo={
				{"","","","",""},
                {"AKL_ToDo","Package","N/A","N/A","N/A"},
                {"AKL-OrderProcess","Process","Code Generation","Finished","4"},
                {"AKL-GoodsIn-CreateBox","Process","Expression Only","N/A","0"},
                {"AKL-HandleEmptyBin","Process","Expression Only","N/A","0"},
                {"AKL-RequestMaterials","Process","Code Generation","Finished","6"},
                {"AKL-RequrestTablar","Process","Code Generation","Finished","5"},
                {"AKL-Topology","Topology","Code Generation","Finished","21"},
                {"","","","",""},
                {"MPL_ToDo","Package","N/A","N/A","N/A"},
                {"MPL-OrderProcess","Process","Code Generation","Finished","5"},
                {"MPL-GoodsIn-CreateBox","Process","Expression Only","N/A","0"},
                {"MPL-HandleEmptyBin","Process","Expression Only","N/A","0"},
                {"MPL-RequestMaterials","Process","Code Generation","N/A","0"},
                
                {"","","","",""},
                {"BKS_ToDo","Package","N/A","N/A","N/A"},
                {"BKSL-OrderProcess","Process","Code Generation","N/A","0"},
                {"BKS-GoodsIn-CreateBox","Process","Expression Only","N/A","0"},                
		 };
		String[] Names={"Package/Model Name","Type","Purpose","Status","Number of Element"};
		JTable table=new JTable(modelInfo,Names);
		table.setDefaultRenderer(Object.class,new IAMReportRender());
		table.setPreferredScrollableViewportSize(new Dimension(550,60));
	    JScrollPane scrollPane=new JScrollPane(table);
	    frame.getContentPane().add(scrollPane,BorderLayout.CENTER);
	    frame.setTitle("The Status of IAM Models");
	    frame.pack();
	    frame.show();

	    frame.addWindowListener(new WindowAdapter() {
	                        public void windowClosing(WindowEvent e) {
	                          System.exit(0);
	                        }
	                      });
	}
	
	
	private void buildTableModel() {
		
	}


	public static void main(String[] args){
	     ReportTable b=new ReportTable();
	   }

}
