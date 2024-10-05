package com.team41.wildwanderer;

import com.team41.wildwanderer.database.AnimalSighting;
import com.team41.wildwanderer.database.UserStatistics;
import kotlin.collections.ArrayDeque;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.InetAddress;
import java.net.Socket;
import java.net.UnknownHostException;
import java.util.ArrayList;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * This is a class to handle making requests to the server to make a request use the makeRequest function and to get
 * the correct format for a request use the generate for the appropriate request
 * Author: Haico Maters
 */
public class ServerRequestGenerator {

    private static Socket serverSocket;
    private static PrintWriter out;
    private static BufferedReader in;

    /**
     * Makes request to server and returns a server response
     * @param request this is the request to the server make certain to use the generate functions else the response
     *                will be 'Invalid Request Received'
     * @return response returns the server response
     */
    public static String makeRequest(String request) {
        String response;
        try {
            serverSocket = new Socket(InetAddress.getByName("10.0.2.2"), 443);
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
        try {
            out = new PrintWriter(serverSocket.getOutputStream(), true);
            in = new BufferedReader(new InputStreamReader(serverSocket.getInputStream()));
            out.println(request);
            response = in.readLine();
        } catch (UnknownHostException e) {
            throw new RuntimeException(e);
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
        return response;
    }

    /**
     * Gets User from DB using username
     * Expected response in format User{username='username'}
     * Expected response on failure 'Failed to get user'
     */
    public String generateGetUserRequest(String username){
        return "User, getUser, " + username;
    }

    /**
     * Deletes User from DB using username
     * Expected response 'Removed User'
     * Expected response on failure 'Failed to remove user'
     */
    public String generateDeleteUserRequest(String username){
        return "User, deleteUser, " + username;
    }

    /**
     * Adds User to DB using username and password
     * Expected response 'New User Added'
     * Expected response on failure 'Failed to add new user'
     */
    public String generateAddUserRequest(String username, String password){
        return "User, addUser, " + username + ", " + password;
    }

    /**
     * Gets all animal sightings from database
     * Expected response in format '[AnimalSighting{sightingID="id", timeSpotted="time", speciesName='name',
     * sightings='number', longitude='longitude', latitude='latitude', user='username'},
     * AnimalSighting{....}, etc.]'
     * Expected response on failure 'Failed to get sightings'
     */
    public String generateGetAllSightingsRequest(){
        return "Sighting, getAllSightings";
    }

    /**
     * Gets single animal sighting from database
     * Expected response in format 'AnimalSighting{sightingID="id", timeSpotted="time", speciesName='name',sightings='number',
     * longitude='longitude', latitude='latitude', user='username'}'
     * Expected response on failure 'Failed to get sighting'
     */
    public String generateGetIndividualSightingRequest(int sightingID){
        return "Sighting, getIndividualSighting, " + sightingID;
    }

    /**
     * Updates the number of time an animal has been spotted
     * Expected response 'Sighting Updated'
     * Expected response on failure 'Failed to update sighting'
     */
    public String generateUpdateSightingRequest(int sightingID){
        return "Sighting, updateSighting, " + sightingID;
    }

    /**
     * Adds animal sighting to database
     * Expected response 'New Sighting Added'
     * Expected response on failure 'Failed to add new sighting'
     */
    public String generateAddSightingRequest(String species, double latitude, double longitude, String user){
        return "Sighting, addSighting, " + species + ", " + latitude + ", " + longitude + ", " + user;
    }

    /**
     * Gets all of an individual user's user stats to database
     * Expected response in format '[UserStatistics{id='id', speciesName='name', sightingNumber='number',
     *  user=username}, UserStatistics{....}, etc.]'
     * Expected response on failure 'Failed to add new sighting'
     */
    public String generateGetUserStatsRequest(String username){
        return "Stats, getAllUserStats, " + username;
    }
    /**
     * Updates the user stats table either increases number of sightings or adds new animal
     * Expected response 'User stats updated'
     * Expected response on failure 'Failed to update user stats'
     */
    public String generateUpdateUserStats(String species, String username){
        return "Stats, updateStats, " + species + ", " + username;
    }

    /**
     * Receives username and password use
     * Expected response 'User Credentials Correct' if password correct
     * Expected response 'User Credentials Incorrect' if password incorrect
     * Expected response on failure 'Login process failed'
     */
    public String generateLoginRequest(String username, String password){
        return "User, login, " + username + ", " + password;
    }

    /**
     * @param response the response from the server to the get user method
     * @return string containing the username to be used for the account page
     */
    public String responseGetUserUsername(String response){
        int startIndex = response.indexOf("'") + 1;
        int endIndex = response.lastIndexOf("'");
        return response.substring(startIndex, endIndex);
    }

    /**
     * Converts the response from getUserStats into a List of UserStatistics object
     * @param response response from the server request to get user stats
     * @return list of UserStatistics objects
     */
    public List<UserStatistics> responseToALlUserStatistics(String response){
        //Split into parts of UserStatistics objects
        List<UserStatistics> userStats = new ArrayList<UserStatistics>();

        Pattern p = Pattern.compile("UserStatistics\\{([^}]+)\\}");
        Matcher m = p.matcher(response);

        while (m.find()){
            String statsString = m.group(1);
            assert statsString != null;
            UserStatistics userStatistics = responseToUserStatistics(statsString);
            userStats.add(userStatistics);
        }
        return userStats;
    }

    /**
     * Converts a single response of get user statistics into a UserStatistics object
     * @param statsString single toString of UserStatistics from server
     * @return User statistics object that comes from the string
     */
    public UserStatistics responseToUserStatistics(String statsString){
        int id = 0;
        String  speciesName = null;
        int sightingNumber = 0;
        String user = null;

        //Split into parts e.g. id='id'
        String[] individualStatParts = statsString.split(", ");
        
        //Split into just the values and variableNames and assign
        for (String individualStatPart: individualStatParts)
        {
            String[] parts = individualStatPart.split("=");
            String variableName = parts[0];
            String value = parts[1].replace("'", "");

            switch (variableName) {
                case "id":
                    id = Integer.parseInt(value);
                    break;
                case "speciesName":
                    speciesName = value;
                    break;
                case "sightingNumber":
                    sightingNumber = Integer.parseInt(value);
                    break;
                case "user":
                    user = value;
                    break;
            }
        }
        return new UserStatistics(id, speciesName, sightingNumber, user);
    }

    /**
     * Converts the response from getAllAnimalSightings into an AnimalSighting object list
     * @param response response from the server request to get user stats
     * @return list of AnimalSighting objects
     */
    public List<AnimalSighting> responseToAllAnimalSighting(String response){
        //Split into parts of AnimalSighting objects
        List<AnimalSighting> animalSightingList = new ArrayList<AnimalSighting>() {};

        Pattern p = Pattern.compile("AnimalSighting\\{([^}]+)\\}");
        Matcher m = p.matcher(response);

        while (m.find()){
            String sightingString = m.group(1);
            assert sightingString != null;
            AnimalSighting as = responseToAnimalSighting(sightingString);
            animalSightingList.add(as);
        }
        return animalSightingList;
    }

    /**
     * Converts a single response of get animal sighting info into a list of AnimalSighting objects
     * @param sightingString single toString of a AnimalSighting from server
     * @return An AnimalSighting object
     */
    public AnimalSighting responseToAnimalSighting(String sightingString) {
        int id = 0;
        String timeSpotted = null;
        String speciesName = null;
        String user = null;
        int sightings = 0;
        double latitude = 0;
        double longitude = 0;

        //Split into parts e.g. id='id'
        String[] individualStatParts = sightingString.split(", ");

        //Split into just the values and variableNames and assign
        for (String individualStatPart : individualStatParts) {
            String[] parts = individualStatPart.split("=");
            String variableName = parts[0];
            String value = parts[1].replaceAll("'", "");

            switch (variableName) {
                case "sightingID":
                    id = Integer.parseInt(value);
                    break;
                case "timeSpotted":
                    timeSpotted = value;
                    break;
                case "speciesName":
                    speciesName = value;
                    break;
                case "user":
                    user = value;
                    break;
                case "sightings":
                    sightings = Integer.parseInt(value);
                    break;
                case "longitude":
                    longitude = Double.parseDouble(value);
                    break;
                case "latitude":
                    latitude = Double.parseDouble(value);
                    break;
            }
        }
        return new AnimalSighting(id, user, speciesName, latitude, longitude, timeSpotted, sightings);
    }
}