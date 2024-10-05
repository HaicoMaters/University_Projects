import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.Socket;

/**
 * This is a class used to test the server works
 * Author: Haico Maters
 */
public class ExampleClient {

    public static void main(String[] args) {
        RequestStringGenerator formatString = new RequestStringGenerator();
        System.out.println(makeRequest(formatString.generateAddUserRequest("Client", "wow")));
        System.out.println(makeRequest(formatString.generateAddUserRequest("Guy", "wow")));
        System.out.println(makeRequest(formatString.generateAddSightingRequest
                ("bob", 1 , 5.21321,  "Client")));
        System.out.println(makeRequest(formatString.generateGetAllSightingsRequest()));
        System.out.println(makeRequest(formatString.generateDeleteUserRequest("Guy")));
        System.out.println(makeRequest(formatString.generateGetUserRequest("Guy")));
        System.out.println(makeRequest(formatString.generateUpdateUserStats("bob", "Client")));
        System.out.println(makeRequest(formatString.generateGetUserStatsRequest("Client")));
        System.out.println(makeRequest(formatString.generateGetUserRequest("Client")));
    }

    public static String makeRequest(String request){
        Socket clientSocket;
        String response;
        try {
            clientSocket = new Socket("localhost", 443);
        }
        catch (IOException e)
        {
            throw new RuntimeException(e);
        }
        try {
            PrintWriter out = new PrintWriter(clientSocket.getOutputStream(), true);
            BufferedReader in = new BufferedReader(new InputStreamReader(clientSocket.getInputStream()));
            out.println(request);
            response = in.readLine();
        } catch (IOException e)
        {
            throw new RuntimeException(e);
        }
        return response;
    }
}
