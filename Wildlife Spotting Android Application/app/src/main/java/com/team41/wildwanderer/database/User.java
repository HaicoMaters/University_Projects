package com.team41.wildwanderer.database;

import androidx.room.ColumnInfo;
import androidx.room.Entity;
import androidx.room.PrimaryKey;
import org.jetbrains.annotations.NotNull;

/**
 * This is a class which uses room to handle the mapping of the User Accounts to a database the methods related
 * to user accounts which are used by the app
 * Author: Haico Maters
 */
@Entity
public class User {

    //Usernames are unique
    @PrimaryKey
    @NotNull
    @ColumnInfo(name = "username")
    public String username;

    @ColumnInfo(name = "password")
    public String password;


    public User(String username, String password) {
        this.username = username;
        this.password = password;
    }

    public String getUsername() {return username;}


    public String getPassword() {return password;}
}
