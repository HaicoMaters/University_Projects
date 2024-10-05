package com.team41.wildwanderer.database;

import androidx.room.RoomDatabase;

@androidx.room.Database(version = 3, entities = {AnimalSighting.class, User.class, UserStatistics.class})
public abstract class Database extends RoomDatabase {
    public abstract AnimalSightingDAO sightingDAO();
    public abstract UserDAO userDAO();
    public abstract UserStatsDAO statsDAO();

}


