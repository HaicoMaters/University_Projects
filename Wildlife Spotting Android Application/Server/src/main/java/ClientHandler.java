import Database.*;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.Socket;
import org.mindrot.jbcrypt.BCrypt;

/**
 * This is a class used to handle client requests and return a response as a string
 * Author: Haico Maters
 */
public class ClientHandler implements Runnable{
    private final Socket clientSocket;

    private static final String badRequest = "Invalid Request Received";

    public ClientHandler(Socket socket) {
        this.clientSocket = socket;
    }


    //Request Format [DB, FUNCTION, VARIABLES NEEDED SEPERATED BY COMMAS IN CORRECT ORDER]
    public String handleRequest(String request){
        String response;
        switch (formatRequest(request)[0]){
            case "User":
                response = userDBRequests(request);
                break;
            case "Stats":
                response = userStatsDBRequest(request);
                break;
            case "Sighting":
                response = animalSightingDBRequests(request);
                break;
            default:
                response = "Invalid Request Received";
        }

        return response;
    }

    //Split into format [which db, which function, which variable, which variable...]
    public String[] formatRequest(String request){
        return request.split(", ");
    }

    public String userDBRequests(String request){
        String response;
        String[] formattedRequest = formatRequest(request);
        switch (formattedRequest[1]) {
            case "getUser": {
                try {
                    response = UserDB.getUser(formattedRequest[2]).toString();
                } catch (Exception e){
                    e.printStackTrace();
                    response = "Failed to get user";
                }
                break;
            }
            case "addUser":{
                //Hash the user password
                User user = new User(formattedRequest[2], BCrypt.hashpw(formattedRequest[3], BCrypt.gensalt()));
                try {
                    UserDB.addUser(user);
                    response = "New User Added";
                } catch (Exception e) {
                    e.printStackTrace();
                    response = "Failed to add new user";
                }
                break;
            }
            case "deleteUser": {
                try {
                    UserDB.removeUser(formattedRequest[2]);
                    response = "Removed User";
                } catch (Exception e) {
                    e.printStackTrace();
                    response = "Failed to remove user";
                }
                break;
            }
            case "login":{
                try {
                    User user = UserDB.getUser(formattedRequest[2]);
                    if (BCrypt.checkpw(formattedRequest[3], user.getPassword())){
                        response = "User Credentials Correct";
                    }
                    else response = "User Credentials Incorrect";
                }
                catch (Exception e){
                    e.printStackTrace();
                    response = "Login process failed";
                }
                break;
            }
            default: response = badRequest;
        }
        return response;
    }

    //Remove sighting is done automatically by server
    public String animalSightingDBRequests(String request){
        String response;
        String[] formattedRequest = formatRequest(request);
        switch (formattedRequest[1]) {
            case "getAllSightings": {
                try {
                    response = AnimalSightingDB.getAllEntry().toString();
                } catch (Exception e){
                    e.printStackTrace();
                    response = "Failed to get sightings";
                }
                break;
            }
            case "addSighting": {
                //format [Sighting, addSighting, 'speciesName', 'latitude', 'longitude, 'username'
                User user = UserDB.getUser(formattedRequest[5]);
                AnimalSighting animal = new AnimalSighting(formattedRequest[2],
                        Double.parseDouble(formattedRequest[3]), Double.parseDouble(formattedRequest[4]), user);
                try {
                    AnimalSightingDB.addEntry(animal);
                    response = "New Sighting Added";
                } catch (Exception e) {
                    e.printStackTrace();
                    response = "Failed to add new sighting";
                }
                break;
            }
            case "getIndividualSighting":{
                try {
                    response = AnimalSightingDB.getIndividualEntry(Integer.parseInt(formattedRequest[2])).toString();
                } catch (Exception e){
                    e.printStackTrace();
                    response = "Failed to get sighting";
                }
                break;
            }
            case "updateSighting": {
                try {
                    //the app will first get the sightings from the db then make a request with the ids
                    AnimalSightingDB.incrementSighting(Integer.parseInt(formattedRequest[2]));
                    response = "Sighting updated";
                } catch (Exception e) {
                    e.printStackTrace();
                    response = "Failed to update sighting";
                }
                break;
            }
            default: response = badRequest;
        }
        return response;
    }

    public String userStatsDBRequest(String request){
        String response ;
        String[] formattedRequest = formatRequest(request);
        switch (formattedRequest[1]) {
            case "getAllUserStats": {
                //format [Stats, getAllUserStats,  'username']
                try {
                    response = UserStatisticsDB.getAllUserStats(formattedRequest[2]).toString();
                } catch (Exception e){
                    e.printStackTrace();
                    response = "Failed to get user stats";
                }
                break;
            }
            case "updateStats": {
                //format [stats, updateStats, 'speciesName','username'
                try {
                    UserStatisticsDB.updateStats(formattedRequest[2], formattedRequest[3]);
                    response = "User stats updated";
                } catch (Exception e) {
                    e.printStackTrace();
                    response = "Failed to update user stats";
                }
                break;
            }
            default: response = badRequest;
        }
        return response;
    }

    public void run() {
        System.out.println("Running");
        try {
             //Read the client request
            BufferedReader in = new BufferedReader(new InputStreamReader(clientSocket.getInputStream()));
            String request = in.readLine();
            System.out.println("Request: " + request);
            String response = handleRequest(request);
            System.out.println("Response: " + response);

            // Send the response back to the client
            PrintWriter out = new PrintWriter(clientSocket.getOutputStream(), true);
            out.println(response);

            // Close the socket
            clientSocket.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}
