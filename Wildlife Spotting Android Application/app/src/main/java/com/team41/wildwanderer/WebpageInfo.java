package com.team41.wildwanderer;

import android.os.Build;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import org.jsoup.Jsoup;
import org.jsoup.nodes.Document;
import org.jsoup.nodes.Element;
import org.jsoup.select.Elements;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
/**
 *This class handles the retrieval of data using the wikipedia api
 *Author: Haico Maters
 */
public class WebpageInfo {

    public String wikipediaQuery(String searchQuery){
        StringBuilder response = new StringBuilder();
        String animalInfo = null;
        try {
            String urlStr = "https://en.wikipedia.org/w/api.php?action=query&format=json&list=search&formatversion=2&srsearch=" + searchQuery;
            URL url = new URL(urlStr);

            HttpURLConnection connection = (HttpURLConnection) url.openConnection();

            connection.setRequestMethod("GET");

            int responseCode = connection.getResponseCode();
            if (responseCode == HttpURLConnection.HTTP_OK) {
                BufferedReader reader = new BufferedReader(new InputStreamReader(connection.getInputStream()));
                String line;

                while ((line = reader.readLine()) != null) {
                    response.append(line);
                }
                reader.close();

                animalInfo = parseAnimalInfo(response.toString());
            } else {
                System.err.println("Error: " + responseCode);
            }
        } catch (IOException e) {
            e.printStackTrace();
        } catch (JSONException e) {
            throw new RuntimeException(e);
        }
        return animalInfo;
    }

    private String parseAnimalInfo(String wikipediaString) throws JSONException {
        JSONObject responseObj = new JSONObject(wikipediaString);

        if (responseObj.has("query")) {
            JSONObject queryObj = responseObj.getJSONObject("query");

            StringBuilder animalInfo = null;
            if (queryObj.has("search")) {
                JSONArray searchResults = queryObj.getJSONArray("search");

                animalInfo = new StringBuilder();
                JSONObject result = searchResults.getJSONObject(0);
                String title = result.getString("title");
                String snippet = result.getString("snippet");
                String pageId = result.getString("pageid");
                String url = getURL(pageId);
                String photoUrl = getPhotoURL(pageId);

                Document doc = Jsoup.parse(snippet);
                Elements elements = doc.select("span");
                for (Element element : elements) {
                    element.remove();
                }
                snippet = doc.text();

                animalInfo.append("Title: ").append(title).append("\n");
                animalInfo.append("Snippet: ").append(snippet).append("\n");
                animalInfo.append("URL: ").append(url).append("\n");
                animalInfo.append("Photo URL: ").append(photoUrl);
                return animalInfo.toString();
            } else {
                return animalInfo.toString();
            }
        }

        return "No search results found.";
    }

    private static String getPhotoURL(String pageId) {

        try {
            String urlStr = "https://en.wikipedia.org/w/api.php?action=query&prop=pageimages&pageids=" + pageId + "&pithumbsize=300&format=json&formatversion=2";
            URL url = new URL(urlStr);
            HttpURLConnection connection = (HttpURLConnection) url.openConnection();

            connection.setRequestMethod("GET");

            int responseCode = connection.getResponseCode();
            if (responseCode == HttpURLConnection.HTTP_OK) {
                BufferedReader reader = new BufferedReader(new InputStreamReader(connection.getInputStream()));
                String line;
                StringBuilder response = new StringBuilder();

                while ((line = reader.readLine()) != null) {
                    response.append(line);
                }

                reader.close();

                //Format the wikipedia api response correctly in
                JSONObject responseObj = new JSONObject(response.toString());
                JSONObject queryObj = responseObj.getJSONObject("query");
                JSONArray pagesArray = queryObj.getJSONArray("pages");
                JSONObject pageObj = pagesArray.getJSONObject(0);
                JSONObject thumbnailObj = pageObj.optJSONObject("thumbnail");
                if (thumbnailObj != null) {
                    return thumbnailObj.getString("source");
                }
            } else {
                System.out.println("Error: " + responseCode);
            }
        } catch (IOException e) {
            e.printStackTrace();
        } catch (JSONException e) {
            throw new RuntimeException(e);
        }

        return "";
    }

    private static String getURL(String pageId) {
        String apiUrl = "https://en.wikipedia.org/w/api.php";
        try {
            String urlStr = apiUrl + "?action=query" + "&prop=info&pageids=" + pageId + "&inprop=url" + "&format=json&formatversion=2";
            URL url = new URL(urlStr);
            HttpURLConnection connection = (HttpURLConnection) url.openConnection();
            connection.setRequestMethod("GET");

            int responseCode = connection.getResponseCode();
            if (responseCode == HttpURLConnection.HTTP_OK) {
                BufferedReader reader = new BufferedReader(new InputStreamReader(connection.getInputStream()));
                String line;
                StringBuilder response = new StringBuilder();

                while ((line = reader.readLine()) != null) {
                    response.append(line);
                }

                reader.close();

                // Extract the URL from the API response
                JSONObject responseObj = new JSONObject(response.toString());
                JSONObject queryObj = responseObj.getJSONObject("query");
                JSONArray pagesArray = queryObj.getJSONArray("pages");
                JSONObject pageObj = pagesArray.getJSONObject(0);
                return pageObj.getString("fullurl");
            } else {
                System.out.println("Error: " + responseCode);
            }
        } catch (IOException e) {
            e.printStackTrace();
        } catch (JSONException e) {
            throw new RuntimeException(e);
        }

        return "";
    }

    public String getWholePageText(String url){
            String text = null;
            try {
                Document doc = Jsoup.connect(url).get();
                text = doc.wholeText();
            }
            catch (Exception e){
                e.printStackTrace();
            }
            assert null !=text ;
            return text;
    }

    public String extractURL(String wikipediaQueryResponse){
        int startIndex = wikipediaQueryResponse.indexOf("URL: ") + 5;  //length of string is 5 so end is + 5
        int endIndex = wikipediaQueryResponse.indexOf("Photo") -1; // -1 due to newline character
        return wikipediaQueryResponse.substring(startIndex, endIndex);
    }
    public String extractSnippet(String wikipediaQueryResponse){
        int startIndex = wikipediaQueryResponse.indexOf("Snippet: ") + 5;  //length of string is 5 so end is + 5
        int endIndex = wikipediaQueryResponse.indexOf("URL") -1; // -1 due to newline character
        return wikipediaQueryResponse.substring(startIndex, endIndex);
    }

    //Filters classes more general to reduce number of icons required
    public String filterClass(String url){
      String animalType = "";
        try {
            Document doc = Jsoup.connect(url).get();
            Elements tdElements = doc.select("td");
            Element tdElement = null;
            for (Element element : tdElements) {
                if (element.text().equals("Class:")) {
                    tdElement = element;
                    break;
                }
            }
            String animalClass = tdElement.nextElementSibling().text();
            if (animalClass.contains("Asteroidea") || animalClass.contains("Bivalvia") || animalClass.contains("Branchiopoda")
                    || animalClass.contains("Cephalopoda") || animalClass.contains("Anthozoa") || animalClass.contains("Amniota")
                    || animalClass.contains("Chondrichthyes") || animalClass.contains("Cubozoa") || animalClass.contains("Demospongiae")
                    || animalClass.contains("Echinoidea") || animalClass.contains("Hydrozoa") || animalClass.contains("Hyperoartia")
                    || animalClass.contains("Malacostraca") || animalClass.contains("Myxini") || animalClass.contains("Scyphozoa")
                    || animalClass.contains("Actinopterygii")) {
                animalType = "Fish-Like";
            } else if (animalClass.contains("Amphibia")) {
                animalType = "Amphibian-Like";
            } else if (animalClass.contains("Chilopoda") || animalClass.contains("Diplopoda") || animalClass.contains("Gastropoda")
                    || animalClass.contains("Insecta") || animalClass.contains("Polychaeta") || animalClass.contains("Polyplacophora")
                    || animalClass.contains("Arachnida") || animalClass.contains("Clitellata")) {
                animalType = "Bug-Like";
            } else if (animalClass.contains("Aves")) {
                animalType = "Bird-Like";
            } else if (animalClass.contains("Mammalia")) {
                animalType = "Mammal-Like";
            } else if (animalClass.contains("Reptilia")) {
                animalType = "Reptile-Like";
            }
        } catch (IOException e) {
            e.printStackTrace();
        }

        return animalType;
    }

}


