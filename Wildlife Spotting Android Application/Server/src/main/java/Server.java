import java.io.IOException;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

import org.hibernate.*;

/**
 *Establishes server connection to the app and retrieves data from the app/database and transfers it to the other
 *Author: Haico Maters
 */
public class Server{
    public static void main(String[] args) {
        System.out.println("Running");
        ScheduledExecutorService scheduler = Executors.newSingleThreadScheduledExecutor();
        scheduler.scheduleAtFixedRate(new SightingTimeChecker(), 0, 1, TimeUnit.MINUTES);
        try{
            ServerSocket serverSocket = new ServerSocket(443);
            while (true){
                Socket clientSocket = serverSocket.accept();
                new Thread(new ClientHandler(clientSocket)).start();
            }
        }
        catch (IOException e){
            e.printStackTrace();
        }
    }
}