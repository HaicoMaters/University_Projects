package com.team41.wildwanderer.database;

import androidx.room.*;
import com.team41.wildwanderer.database.AnimalSighting;

/**
 * This is an interface which uses room to handle the AnimalSighting database methods
 * Author: Haico Maters
 */
@Dao
public interface AnimalSightingDAO {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    public long addSighting(AnimalSighting sighting);

    @Query("UPDATE animalsighting SET sightings = sightings + 1 WHERE id = :id")
    public void updateSightingNumber(int id);

    @Delete
    public void deleteSighting(AnimalSighting sighting);

    @Query("SELECT * FROM animalsighting WHERE id = :id")
    public AnimalSighting getSighting(int id);


    //add method to update the number of sightings or add a new sighting depending on the distance of same animals
    /*void addOrUpdate(AnimalSighting sighting){
        List<>
    }*/
}
