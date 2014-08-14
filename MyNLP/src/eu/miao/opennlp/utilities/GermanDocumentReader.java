package eu.miao.opennlp.utilities;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.Scanner;

public class GermanDocumentReader {

	public String readEncodedFile(String pathname) throws IOException {
		File fileDir = new File(pathname);
		String lineSeparator = System.getProperty("line.separator");
		BufferedReader in = new BufferedReader(new InputStreamReader(
				new FileInputStream(fileDir), "UTF8"));
		String line;
		StringBuilder stringBuilder = new StringBuilder();
		while ((line = in.readLine()) != null) {
			stringBuilder.append(line + lineSeparator);
		}
		in.close();
		return stringBuilder.toString();

	}

	public static String readFile(String pathname) throws IOException {

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
