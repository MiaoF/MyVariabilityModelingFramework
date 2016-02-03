package eu.miao.opennlp;

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
import eu.miao.opennlp.utilities.GermanDocumentReader;
import eu.miao.opennlp.utilities.GermanStopWordList;
import eu.miao.opennlp.utilities.ReportProfile;

public class MyOpenNLP_DE {

	static ReportProfile profile = new ReportProfile();
	static GermanDocumentReader myReader; 
	static private eu.miao.opennlp.utilities.GermanStopWordList stopwordsList = GermanStopWordList
			.getGermanStopWordList();

	public static void main(String[] args) {
		GermanDocumentReader myReader = new GermanDocumentReader();
		try {	
//			parseBySentences("de-sgm.txt");
			parseByPOS("de-sgm.txt");
//			parseByPOS("demo-de-processes.txt");
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	private static void parseByPOS(String pathOfInputFile) throws IOException {
		POSModel model = new POSModelLoader()
				.load(new File("de-pos-maxent.bin"));
		PerformanceMonitor perfMon = new PerformanceMonitor(System.err, "sent");
		POSTaggerME tagger = new POSTaggerME(model);
		GermanDocumentReader myReader = new GermanDocumentReader();
		String input = myReader.readEncodedFile(pathOfInputFile);
		ObjectStream<String> lineStream = new PlainTextByLineStream(
				new StringReader(input));

		perfMon.start();
		String line;
		while ((line = lineStream.read()) != null) {

			String whitespaceTokenizerLine[] = WhitespaceTokenizer.INSTANCE
					.tokenize(line);
			String[] tags = tagger.tag(whitespaceTokenizerLine);
			POSSample sample = new POSSample(whitespaceTokenizerLine, tags);
			understandSentence(sample.toString());
			perfMon.incrementCounter();
		}
		perfMon.stopAndPrintFinalResult();
		profile.printProfile();
	}

	private static void understandSentence(String sample) {
		if (sample.contains("VVFIN") || sample.contains("VVIMP")
				|| sample.contains("VVINF") || sample.contains("VVIZU")
				|| sample.contains("VVPP") || sample.contains("VAFIN")
				|| sample.contains("VAIMP") || sample.contains("VAINF")
				|| sample.contains("VAPP") || sample.contains("VMFIN")
				|| sample.contains("VMFIN") || sample.contains("VMPP")) {
			String subject = getSubject(sample);
			String verb = getVerb(sample);
			String objects = getObjects(sample);
			String verbDirectObjects = verb + "-" + objects;
			String hashKey = subject + "-" + verb + "-" + objects;

			if (!verb.isEmpty() && !subject.isEmpty()
					&& !stopwordsList.isStopword(verb)) {
				addToProfile(subject + "-" + verb, profile.getSubjectVerbs(),
						profile.getSubjectVerbCollection());
			}

			if (!verb.isEmpty() && !objects.isEmpty()
					&& !stopwordsList.isStopword(verb)) {
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
		int first_VB = sample.indexOf("_V");
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
		int first_VB = sample.indexOf("_V");
		int lastWhiteSpaceBefore_VBZ = sample.lastIndexOf(" ", first_VB) + 1;
		String verb = "";
		if (first_VB > 0)
			verb = sample.substring(lastWhiteSpaceBefore_VBZ, first_VB);
		// int next_VBZ = sample.indexOf("_VBZ", first_VB);
		// if (next_VBZ > 0) {
		// String restString = sample.substring(first_VB + 4, sample.length());
		// int lastWhiteSpaceBefore2nd_VBZ = restString.lastIndexOf(" ",
		// restString.indexOf("_VBZ"));
		// int indexOfAND = restString.indexOf("and_CC");
		// if (indexOfAND == lastWhiteSpaceBefore2nd_VBZ - 6) {
		// String theMainSubject = getSubject(orignialSamole);
		// String newSentence = theMainSubject
		// + " "
		// + restString.substring(indexOfAND + 3,
		// restString.length());
		// understandSentence(newSentence);
		// System.out.println(newSentence);
		// }
		// }
		return verb;
	}

	private static String getSubject(String sample) {
		int first_NN = sample.indexOf("_NN");
		int lastWhiteSpaceBefore_NN = sample.lastIndexOf(" ", first_NN) + 1;
		String subject = "";
		if (first_NN > 2)
			subject = sample.substring(lastWhiteSpaceBefore_NN, first_NN);
		int next_NN = sample.indexOf("_NN", first_NN + 3);
		int first_VB = sample.indexOf("_V");
		if (next_NN > 1 && first_VB > 1 && first_VB > next_NN) {
			int lastWhiteSpaceBeforeSecond_NN = sample
					.lastIndexOf(" ", next_NN) + 1;
			subject += sample.substring(lastWhiteSpaceBeforeSecond_NN, next_NN);
		}
		return subject;
	}

	public static void parseBySentences(String pathOfInputFile)
			throws IOException {
		String paragraph = GermanDocumentReader.readFile(pathOfInputFile);

		// always start with a model, a model is learned from training data
		InputStream is = new FileInputStream("de-sent.bin");
		SentenceModel model = new SentenceModel(is);
		SentenceDetectorME sdetector = new SentenceDetectorME(model);

		String sentences[] = sdetector.sentDetect(paragraph);
		int i = 0;
		while (i < sentences.length) {
			i++;
		}
		System.out.println("Detected Sentences in Total:" + i);
		is.close();
	}

	

	// File file = new File(pathname);
	// StringBuilder fileContents = new StringBuilder((int) file.length());
	// Scanner scanner = new Scanner(file);
	// String lineSeparator = System.getProperty("line.separator");
	//
	// try {
	// while (scanner.hasNextLine()) {
	// String tem = scanner.nextLine() + lineSeparator;
	// System.out.println(tem);
	// fileContents.append(Charset.forName("UTF-8").encode(tem));
	// }
	// return fileContents.toString();
	// } finally {
	// scanner.close();
	// }
}