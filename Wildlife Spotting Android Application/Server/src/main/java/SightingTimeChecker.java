import Database.AnimalSighting;
import Database.AnimalSightingDB;

import java.util.List;

/**
 * This is a class that is run with the server to remove old sightings from the database
 */
public class SightingTimeChecker implements Runnable{
    @Override
    public void run() {
        List<AnimalSighting> sightings = AnimalSightingDB.getAllEntry();
        for (AnimalSighting sighting: sightings){
            if (sighting.isPastExpirationTime()){
                System.out.println("Removed old sighting");
                AnimalSightingDB.removeEntry(sighting.getSightingID());
            }
        }
    }
}
