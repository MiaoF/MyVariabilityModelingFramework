package test;

import opennlp.tools.cmdline.PerformanceMonitor;
import opennlp.tools.cmdline.postag.POSModelLoader;
import opennlp.tools.postag.POSModel;
import opennlp.tools.postag.POSSample;
import opennlp.tools.postag.POSTaggerME;
import opennlp.tools.sentdetect.SentenceDetectorME;
import opennlp.tools.sentdetect.SentenceModel;
import opennlp.tools.tokenize.WhitespaceTokenizer;
import opennlp.tools.util.ObjectStream;
import opennlp.tools.util.PlainTextByLineStream;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Scanner;

public class OpenNLPTest {

	static DocumentProfile profile = new DocumentProfile();

	public static void main(String[] args) {

		try {
			// parseBySentences("goodsIn.txt");
			// parseByPOS("goodsIn.txt");
			// parseBySentences("goodsOut.txt");
			// parseByPOS("goodsOut.txt");
//
//			System.out.println("====Summary====");
//			parseBySentences("all.txt");
//			parseByPOS("all.txt");
			
			
//			parseBySentences("AllThreeFull.txt");
			parseByPOS("example.txt");
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	private static void parseByPOS(String pathOfInputFile) throws IOException {
		POSModel model = new POSModelLoader()
				.load(new File("en-pos-maxent.bin"));
		PerformanceMonitor perfMon = new PerformanceMonitor(System.err, "sent");
		POSTaggerME tagger = new POSTaggerME(model);
		String input = readFile(pathOfInputFile);
		ObjectStream<String> lineStream = new PlainTextByLineStream(
				new StringReader(input));

		perfMon.start();
		String line;
		while ((line = lineStream.read()) != null) {

			String whitespaceTokenizerLine[] = WhitespaceTokenizer.INSTANCE
					.tokenize(line);
			String[] tags = tagger.tag(whitespaceTokenizerLine);
			POSSample sample = new POSSample(whitespaceTokenizerLine, tags);
			System.out.println(sample.toString());
			understandSentence(sample.toString());
			perfMon.incrementCounter();
		}
		perfMon.stopAndPrintFinalResult();
		profile.printProfile();
	}

	private static void understandSentence(String sample) {
		sample = avoidMissingSomeDomainWords(sample);
		if (sample.contains("_NN")
				&& (sample.contains("VBZ") || sample.contains("_VB"))) {
			// stores in POS is recognized as "_NNS"
			String subject = getSubject(sample);
			String verb = getVerb(sample);
			String objects = getObjects(sample);
			String verbDirectObjects = verb + "-" + objects;
			String hashKey = subject + "-" + verb + "-" + objects;
			if (!verb.isEmpty() && !subject.isEmpty() && notStopWords(verb) ) {
				addToProfile(subject + "-" + verb, profile.getSubjectVerbs(),
						profile.getSubjectVerbCollection());
			}
			
			if (!verb.isEmpty() && !objects.isEmpty() && notStopWords(verb)) {
				addToProfile(subject, profile.getObjects(),
						profile.getObjectCollection());
				addToProfile(verb, profile.getVerbs(),
						profile.getVerbCollection());
				addToProfile(objects, profile.getSubjects(),
						profile.getSubjectCollection());
				addToProfile(verbDirectObjects, profile.getVerbDirectObject(),
						profile.getVerbDirectObjectCollection());
				addToProfile(hashKey, profile.getSubject_verb_objects(),
						profile.getCombiCollection());
			}
		}
	}

	private static boolean notStopWords(String verb) {
		if(verb.contains("have") || verb.contains("is")  || verb.contains("has") || verb.contains("are") || verb.length() < 2  || verb.contains("be"))
			return false;
		return true;
	}

	private static String avoidMissingSomeDomainWords(String sample) {
		sample = sample.replace("stores_NNS", "stores_VBZ");
		sample = sample.replace("stocks-in_NN", "stocks-in_VBZ");
		return sample;
	}

	private static void addToProfile(String string,
			HashMap<String, Integer> map, ArrayList<String> list) {
		if (map.containsKey(string)) {
			Integer counter = map.get(string);
			counter = counter + 1;
			map.put(string, counter);
		} else {
			map.put(string, 1);
			list.add(string);
		}
	}

	private static String getObjects(String sample) {
		String subject = "";
		int first_VB = sample.indexOf("_VBZ");
		if (first_VB == 0)
			first_VB = sample.indexOf("_VB");
		if (first_VB > 0)
			subject = getSubjectString(sample.substring(first_VB,
					sample.length()));
		return subject;

	}

	private static String getSubjectString(String string) {
		// System.out.println(string);
		String temp = "";
		String restString = "";
		int fisrt_NN = string.indexOf("_NN");
		int lastWhiteSpaceBefore_NN = string.lastIndexOf(" ", fisrt_NN);
		if (fisrt_NN > 0 && lastWhiteSpaceBefore_NN > 0
				&& lastWhiteSpaceBefore_NN < fisrt_NN) {
			temp = string.substring(lastWhiteSpaceBefore_NN, fisrt_NN);
			restString = string.substring(fisrt_NN + 3, string.length());
			if (restString.contains("_NN"))
				return temp + "-" + getSubjectString(restString);
		}
		// System.out.println(temp);
		return temp;
	}

	private static String getVerb(String sample) {
		String orignialSamole = sample;
		int first_VB = sample.indexOf("_VB");
		int lastWhiteSpaceBefore_VBZ = sample.lastIndexOf(" ", first_VB) + 1;
		String verb = "";
		if (first_VB > 0)
			verb = sample.substring(lastWhiteSpaceBefore_VBZ, first_VB);
//		int next_VBZ = sample.indexOf("_VBZ", first_VB);
//		if (next_VBZ > 0) {
//			String restString = sample.substring(first_VB + 4, sample.length());
//			int lastWhiteSpaceBefore2nd_VBZ = restString.lastIndexOf(" ",
//					restString.indexOf("_VBZ"));
//			int indexOfAND = restString.indexOf("and_CC");
//			if (indexOfAND == lastWhiteSpaceBefore2nd_VBZ - 6) {
//				String theMainSubject = getSubject(orignialSamole);
//				String newSentence = theMainSubject
//						+ " "
//						+ restString.substring(indexOfAND + 3,
//								restString.length());
//				understandSentence(newSentence);
//				System.out.println(newSentence);
//			}
//		}
		return verb;
	}

	private static String getSubject(String sample) {
		int first_NN = sample.indexOf("_NN");
		int lastWhiteSpaceBefore_NN = sample.lastIndexOf(" ", first_NN) + 1;
		String subject = "";
		if (first_NN > 2)
			subject = sample.substring(lastWhiteSpaceBefore_NN, first_NN);
		int next_NN = sample.indexOf("_NN", first_NN + 3);
		int first_VB = sample.indexOf("_VB");
		if (next_NN > 1 && first_VB > 1 && first_VB > next_NN) {
			int lastWhiteSpaceBeforeSecond_NN = sample
					.lastIndexOf(" ", next_NN) + 1;
			subject += sample.substring(lastWhiteSpaceBeforeSecond_NN, next_NN);
		}
		return subject;
	}

	public static void parseBySentences(String pathOfInputFile)
			throws IOException {
		String paragraph = readFile(pathOfInputFile);

		// always start with a model, a model is learned from training data
		InputStream is = new FileInputStream("en-sent.bin");
		SentenceModel model = new SentenceModel(is);
		SentenceDetectorME sdetector = new SentenceDetectorME(model);

		String sentences[] = sdetector.sentDetect(paragraph);
		int i = 0;
		while (i < sentences.length) {
//			System.out.println(sentences[i]);
//			System.out.println("------------");
			i++;
		}
		System.out.println("Detected Sentences in Total:" + i);
		is.close();
	}

	private static String readFile(String pathname) throws IOException {

		File file = new File(pathname);
		StringBuilder fileContents = new StringBuilder((int) file.length());
		Scanner scanner = new Scanner(file);
		String lineSeparator = System.getProperty("line.separator");

		try {
			while (scanner.hasNextLine()) {
				fileContents.append(scanner.nextLine() + lineSeparator);
			}
			return fileContents.toString();
		} finally {
			scanner.close();
		}
	}
}