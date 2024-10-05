package Database;

import com.sun.istack.NotNull;

import javax.persistence.*;
import java.time.LocalDateTime;

/**
 *Class to represent an animal sighting in the database
 *Author Haico Maters
 */

@Entity(name = "AnimalSighting")
public class AnimalSighting {

    @Id
    @Column
    @GeneratedValue(strategy = GenerationType.AUTO)
    private int sightingID;

    @NotNull
    @Column
    private LocalDateTime timeSpotted;

    @Column
    @NotNull
    private String speciesName;

    @Column
    @NotNull
    private int sightings;

    @Column(scale = 15)
    @NotNull
    private double longitude;

    @Column(scale = 15)
    @NotNull
    private double latitude;

    @NotNull
    @ManyToOne
    @JoinColumn(name = "user", referencedColumnName = "username")
    private User user;

    //use constructor to get data from db into a class instance
   public AnimalSighting(String speciesName, double longitude, double latitude, User user) {
        this.timeSpotted = LocalDateTime.now();
        this.speciesName = speciesName;
        this.sightings = 1;
        this.longitude = longitude;
        this.latitude = latitude;
        this.user = user;
    }

    public AnimalSighting() {
    }

    public String getSpeciesName() {
        return speciesName;
    }

    public int getSightings() {
        return sightings;
    }

    public double getLongitude() {
        return longitude;
    }
    public double getLatitude() {
        return latitude;
    }

    public int getSightingID() {return sightingID;}

    // Time to stay until = timeSpotted + 15 minutes + 5 minutes for each individual sighting
    public boolean isPastExpirationTime(AnimalSighting this){
       LocalDateTime expirationTime = timeSpotted.plusMinutes(((sightings -1) * 5L) + 15);
        return LocalDateTime.now().isAfter(expirationTime);
    }

    @Override
    public String toString() {
        return "AnimalSighting{" +
                "sightingID=" + sightingID +
                ", timeSpotted=" + timeSpotted +
                ", speciesName='" + speciesName + '\'' +
                ", sightings=" + sightings +
                ", longitude=" + longitude +
                ", latitude=" + latitude +
                ", user=" + user.getUsername() +
                '}';
    }

    public void incrementSightings(){
        sightings++;
    }
}
