package myplugin;

import java.util.HashMap;

import com.nomagic.magicdraw.core.Application;
import com.nomagic.uml2.ext.magicdraw.classes.mdkernel.Element;

/**
 * The behavior of our MagicDraw plugin
 * 
 * @author Xavier Seignard
 */
public class SimplePluginBehavior {
    
    // Our visitor to count the type of each element.
    private CountingVisitor visitor = new CountingVisitor();

    /**
     * Returns a string containing the counting results
     * 
     * @param map the HashMap<String, Integer> containing the counting results
     * @return string describing the counting results
     */
    public String returnResults(HashMap<String, Integer> map)
    {
        // The string to return
        String textToReturn = "";

        textToReturn += "Number of elements for each types :\n";
        for (String key : map.keySet()) {
            textToReturn += "\t"  + key + " : " + (map.get(key)).intValue() + "\n";
        }


        // Calculation for some means (the real interesting part)
        Integer packages = map.get("Package");
        Integer classes = map.get("Class");
        Integer features = map.get("FeatureNodes");
        Integer attributes = map.get("Property");
        Integer operations = map.get("Operation");

        
        if (features!=null && operations!=null) {
            textToReturn += "Average operations per class : " + (features.doubleValue()/features.doubleValue()) + "\n";
        }
        if (packages != null && classes != null) {
            textToReturn += "Average classes per package : " + (classes.doubleValue()/packages.doubleValue()) + "\n";
        }
        
        if (classes!=null && operations!=null) {
            textToReturn += "Average operations per class : " + (operations.doubleValue()/classes.doubleValue()) + "\n";
        }

        if (classes!=null && attributes!=null)
            textToReturn += "Average attributes per class : " + (attributes.doubleValue()/classes.doubleValue()) + "\n";

        // if map was empty and no elements was found.
        if (textToReturn.length()==0)
        {
            textToReturn = "No elements found!";

        }

        return textToReturn;
    }

    /**
     * We parse all the children of the given root element.
     * For each element, we visit it to count the types of the elements
     * 
     * @param rootElement the root element to start counting
     */
    public void visitChildren(Element rootElement)
    {
        try {
            rootElement.accept(visitor);
        }
        catch (Exception e) {
            // If something goes wrong we log it to the MagicDraw GUI logger
            Application.getInstance().getGUILog().log(
                    "[Simple Plugin] Exception occured : " + e.toString());
        }
        // We visit the children elements
        for (Element child : rootElement.getOwnedElement()) {
              visitChildren(child);
        }
    }

    /**
     * Getter for the visitor.
     * @return the visitor
     */
    public CountingVisitor getVisitor() {
        return visitor;
    }
}
