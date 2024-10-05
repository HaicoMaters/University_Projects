package com.team41.wildwanderer.database;

import androidx.room.*;
import org.jetbrains.annotations.NotNull;

import java.time.LocalDateTime;

/**
 * This is a class which uses room to handle the mapping of the AnimalSightings to a database and the methods related
 * to animal sightings which are used by the app
 * Author: Haico Maters
 */
@Entity(foreignKeys = @ForeignKey(entity = User.class, parentColumns = "username",childColumns = "user",
        onDelete = ForeignKey.CASCADE))
public class AnimalSighting {

    @PrimaryKey(autoGenerate = true)
    @NotNull()
    @ColumnInfo(name = "id")
    int sightingID;

    @ColumnInfo(name = "user")
    @NotNull
    String user;

    @ColumnInfo(name = "species_name")
    @NotNull
    String speciesName;

    @ColumnInfo(name = "latitude")
    @NotNull
    double latitude;

    @ColumnInfo(name = "longitude")
    @NotNull
    double longitude;

    @ColumnInfo(name = "time_spotted")
    @NotNull
    String timeSpotted;
    @ColumnInfo(name = "sightings")
    @NotNull
    int sightingNumber;



    public AnimalSighting(@NotNull String speciesName,@NotNull double latitude, double longitude, @NotNull String user) {
        this.speciesName = speciesName;
        this.latitude = latitude;
        this.longitude = longitude;
        this.sightingNumber = 1;
        this.timeSpotted = LocalDateTime.now().toString();
        this.user = user;
    }

    @Ignore
    public AnimalSighting(int sightingID, @NotNull String user, @NotNull String speciesName, double latitude, double longitude, @NotNull String timeSpotted, int sightingNumber) {
        this.sightingID = sightingID;
        this.user = user;
        this.speciesName = speciesName;
        this.latitude = latitude;
        this.longitude = longitude;
        this.timeSpotted = timeSpotted;
        this.sightingNumber = sightingNumber;
    }

    public int getSightingID() {
        return sightingID;
    }

    public String getSpeciesName() {
        return speciesName;
    }

    public double getLatitude() {
        return latitude;
    }

    public double getLongitude() {
        return longitude;
    }

    public String getTimeSpotted() {
        return timeSpotted;
    }

    public int getSightingNumber() {
        return sightingNumber;
    }

    public String getUser() {
        return user;
    }
}
