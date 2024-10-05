package com.team41.wildwanderer.database;

import androidx.room.*;
import com.team41.wildwanderer.database.User;

/**
 * This is an interface which uses room to handle the User database methods
 * Author: Haico Maters
 */
@Dao
public interface UserDAO {
    @Insert(onConflict = OnConflictStrategy.ROLLBACK)
    public void addUser(User user);

    @Update
    public void updateUser(User user);

    @Delete
    public void deleteUser(User user);

    @Query("SELECT * FROM user WHERE username = :username")
    public User getUser(String username);
}
