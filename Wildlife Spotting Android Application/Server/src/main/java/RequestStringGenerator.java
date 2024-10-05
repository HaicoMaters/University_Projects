/**
 * This is a class used by the example client to generate requests
 * Author: Haico Maters
 */
public class RequestStringGenerator {
    public String generateGetUserRequest(String username){
        return "User, getUser, " + username;
    }

    public String generateDeleteUserRequest(String username){
        return "User, deleteUser, " + username;
    }

    public String generateAddUserRequest(String username, String password){
        return "User, addUser, " + username + ", " + password;
    }

    public String generateGetAllSightingsRequest(){
        return "Sighting, getAllSightings";
    }

    //sightingID gotten from getAllSightings
    public String generateUpdateSightingRequest(int sightingID){
        return "Sighting, updateSighting, " + sightingID;
    }
    public String generateAddSightingRequest(String species, double latitude, double longitude, String user){
        return "Sighting, addSighting, " + species + ", " + latitude + ", " + longitude + ", " + user;
    }

    public String generateGetUserStatsRequest(String username){
        return "Stats, getAllUserStats, " + username;
    }
    public String generateUpdateUserStats(String species, String username){
        return "Stats, updateStats, " + species + ", " + username;
    }
}
