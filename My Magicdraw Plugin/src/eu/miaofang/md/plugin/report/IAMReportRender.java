package eu.miaofang.md.plugin.report;

import java.awt.Color;
import java.awt.Component;

import javax.swing.CellEditor;
import javax.swing.JTable;
import javax.swing.JTextField;
import javax.swing.table.DefaultTableCellRenderer;
import javax.swing.table.TableCellRenderer;

public class IAMReportRender implements TableCellRenderer {
	public static final DefaultTableCellRenderer DEFAULT_RENDERER = new DefaultTableCellRenderer();

	public Component getTableCellRendererComponent(JTable table, Object value,
			boolean isSelected, boolean hasFocus, int row, int column) {

		Component renderer =
				DEFAULT_RENDERER.getTableCellRendererComponent(table, value,
				isSelected, hasFocus, row, column);
		if(value.toString().equalsIgnoreCase("finished")) {
			renderer.setForeground(Color.decode("#00CED1"));
		} else 
			renderer.setForeground(Color.black);
		
		if(value.toString().equalsIgnoreCase("")) {
			renderer.setBackground((Color.decode("#87CEFA")));
		} else
			renderer.setBackground(Color.white);
		return renderer;

	}

}
