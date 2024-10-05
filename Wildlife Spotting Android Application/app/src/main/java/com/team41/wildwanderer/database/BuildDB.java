package com.team41.wildwanderer.database;

import android.content.Context;
import androidx.room.Room;
import androidx.test.core.app.ApplicationProvider;

public class BuildDB {
    Database db;

    public BuildDB() {
        Context context = ApplicationProvider.getApplicationContext();
        this.db = Room.databaseBuilder(context, Database.class, "db").build();
        db.userDAO().addUser(new User("Greg", "123456"));
    }

    public Database getDB(){
        return db;
    }
}
