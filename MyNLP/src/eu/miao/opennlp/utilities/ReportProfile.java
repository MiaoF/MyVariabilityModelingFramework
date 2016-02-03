package eu.miao.opennlp.utilities;

import java.lang.reflect.Array;
import java.util.ArrayList;
import java.util.HashMap;

import eu.miao.opennlp.utilities.math.Statistics;

public class ReportProfile {
	
	private ArrayList<String> verbCollection = new ArrayList<String>();
	private ArrayList<String> subjectCollection = new ArrayList<String>();
	private ArrayList<String> objectCollection = new ArrayList<String>();
	private ArrayList<String> verbDirectObjectCollection = new ArrayList<String>();
	private ArrayList<String> subjectVerbCollection = new ArrayList<String>();
	private ArrayList<String> combiCollection = new ArrayList<String>();
	
	private HashMap<String, Integer> subjects = new HashMap<String, Integer>();
	private HashMap<String, Integer> verbs = new HashMap<String, Integer>();
	private HashMap<String, Integer> verbDirectObject = new HashMap<String, Integer>();
	private HashMap<String, Integer> subjectVerbs = new HashMap<String, Integer>();
	private HashMap<String, Integer> objects = new HashMap<String, Integer>();
	private HashMap<String, Integer> subject_verb_objects = new HashMap<String, Integer>();
	
	

	public ArrayList<String> getVerbCollection() {
		return verbCollection;
	}

	public void setVerbCollection(ArrayList<String> verbCollection) {
		this.verbCollection = verbCollection;
	}

	public ArrayList<String> getSubjectCollection() {
		return subjectCollection;
	}

	public void setSubjectCollection(ArrayList<String> subjectCollection) {
		this.subjectCollection = subjectCollection;
	}

	public ArrayList<String> getObjectCollection() {
		return objectCollection;
	}

	public void setObjectCollection(ArrayList<String> objectCollection) {
		this.objectCollection = objectCollection;
	}

	public HashMap<String, Integer> getSubject_verb_objects() {
		return subject_verb_objects;
	}

	public void setSubject_verb_objects(HashMap<String, Integer> subject_verb_objects) {
		this.subject_verb_objects = subject_verb_objects;
	}

	public HashMap<String, Integer> getSubjects() {
		return subjects;
	}

	public void setSubjects(HashMap<String, Integer> subjects) {
		this.subjects = subjects;
	}

	public HashMap<String, Integer> getObjects() {
		return objects;
	}

	public void setObjects(HashMap<String, Integer> objects) {
		this.objects = objects;
	}

	public HashMap<String, Integer> getVerbs() {
		return verbs;
	}

	public void setVerbs(HashMap<String, Integer> verbs) {
		this.verbs = verbs;
	}

	public ArrayList<String> getCombiCollection() {
		return combiCollection;
	}

	public void setCombiCollection(ArrayList<String> combiCollection) {
		this.combiCollection = combiCollection;
	}

	
	public void printProfile() {

		System.out.println("==========Report=============");
//		printWithCollectionAndMap(this.objectCollection, this.objects);
		
		System.out.println("Number of Verbs: " + this.verbCollection.size());
		printWithCollectionAndMap(this.verbCollection, this.verbs);

//		System.out.println("==========Objects=============");
//		printWithCollectionAndMap(this.subjectCollection, this.subjects);
//		
//		System.out.println("=======================");
//		System.out.println("==========subject-verb combination=============");
//		printWithCollectionAndMap(this.subjectVerbCollection, this.subjectVerbs);
//		
		
		System.out.println("===========Verb-Direct Objects============");
		printWithCollectionAndMap(this.verbDirectObjectCollection, this.verbDirectObject);

		
		
//		System.out.println("=======================");
//		System.out.println("===========combi============");
//		printWithCollectionAndMap(this.combiCollection, this.subject_verb_objects);
	}

	private void printWithCollectionAndMap(ArrayList<String> collection, HashMap<String, Integer> map) {
		int index = 0;
		double threshold = 1;
		double counter = 0;
		double totalHits = 0;
		double numbersBiggerThanthreshold = 1;
		double[] statistics = new double[collection.size()];
		
		while(index < collection.size()){
			String object = collection.get(index);
			Integer occurence = map.get(object);
			statistics[index] = occurence;
			index ++;
			if(occurence != null) {
				totalHits += occurence;
				if(occurence > threshold) {
					System.out.println(index + ": " + object + ": " + occurence);
					counter++;
					numbersBiggerThanthreshold += occurence;
				}
			}
			
		}
		double rate = numbersBiggerThanthreshold / totalHits;
		System.out.println("********************Found Hits in Total: " + totalHits + "*************");
		System.out.println("********************Number of Unique Hits: " + index + "*************");
		System.out.println("********************Threshold: " + threshold + "*************");
		System.out.println("************* Items Bigger Than Threshold: " + counter + "*************");
//		System.out.println("************* Sum of All Items Bigger Than Threshold: " + numbersBiggerThanthreshold + "*************(" + rate + ")");
		
		Statistics stat = new Statistics(statistics);
		
		System.out.println("************* Mean: " + stat.getMean() + "*************");
		System.out.println("************* Medium: " + stat.median() + "*************");
		System.out.println("************* Deviation: " + stat.getStdDev() + "*************");
		
		
	}

	public ArrayList<String> getVerbDirectObjectCollection() {
		return verbDirectObjectCollection;
	}

	public void setVerbDirectedObjectCollection(
			ArrayList<String> verbDirectedObjectCollection) {
		this.verbDirectObjectCollection = verbDirectedObjectCollection;
	}

	public HashMap<String, Integer> getVerbDirectObject() {
		return verbDirectObject;
	}

	public void setVerbDirectedObject(HashMap<String, Integer> verbDirectedObject) {
		this.verbDirectObject = verbDirectedObject;
	}

	public ArrayList<String> getSubjectVerbCollection() {
		return subjectVerbCollection;
	}

	public void setSubjectVerbCollection(ArrayList<String> subjectVerbCollection) {
		this.subjectVerbCollection = subjectVerbCollection;
	}

	public HashMap<String, Integer> getSubjectVerbs() {
		return subjectVerbs;
	}

	public void setSubjectVerbs(HashMap<String, Integer> subjectVerbs) {
		this.subjectVerbs = subjectVerbs;
	}
}
