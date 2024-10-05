package com.team41.wildwanderer.database;

import androidx.room.*;
import com.team41.wildwanderer.database.UserStatistics;

import java.util.List;

/**
 * This is an interface which uses room to handle the User Statistics database methods
 * Author: Haico Maters
 */
@Dao
public interface UserStatsDAO {
    @Insert(onConflict = OnConflictStrategy.ROLLBACK)
    public void addUserStat(UserStatistics userStat);

    // Used when deleting a user account removes all related user stats to save db space
    @Delete
    public void removeUserStats(List<UserStatistics> userStats);

    @Query("UPDATE userstatistics SET sighting_number = sighting_number + 1 WHERE user = :user AND species_name = :species")
    public void updateNumberOfSightings(String user, String species);

    @Query("SELECT * FROM userstatistics WHERE user = :user")
    public List<UserStatistics> getAllIndividualUserStats(String user);
}
