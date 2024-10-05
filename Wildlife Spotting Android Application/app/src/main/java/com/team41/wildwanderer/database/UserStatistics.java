package com.team41.wildwanderer.database;

import androidx.room.*;
import org.jetbrains.annotations.NotNull;

/**
 * This is a class which uses room to handle the mapping of the User Statistics to a database, which are previously seen
 * animals + the number of sightings user stats are separate from animal sightings as animal sightings are time-based
 * and seen by multiple users, while user stats are a permanent way of tracking all previously seen for a single user
 * Author: Haico Maters
 */
@Entity(foreignKeys = @ForeignKey(entity = User.class, parentColumns = "username",childColumns = "user",
        onDelete = ForeignKey.CASCADE))
public class UserStatistics {

    @PrimaryKey(autoGenerate = true)
    @NotNull
    int id;

    @ColumnInfo(name = "species_name")
    @NotNull
    String species;


    @ColumnInfo(name = "sighting_number")
    @NotNull
    int sightings;

    @NotNull
    String user;

    public UserStatistics(@NotNull String species, @NotNull String user) {
        this.species = species;
        this.user = user;
        this.sightings = 1;
    }

    @Ignore
    public UserStatistics(int id, @NotNull String species, int sightings, @NotNull String user) {
        this.id = id;
        this.species = species;
        this.sightings = sightings;
        this.user = user;
    }

    public int getId() {
        return id;
    }

    public int getSightings() {
        return sightings;
    }

    @NotNull
    public String getSpecies() {
        return species;
    }

    @NotNull
    public String getUser() {
        return user;
    }
}
