package com.team41.wildwanderer;

import android.content.Context;
import androidx.room.Room;
import androidx.test.core.app.ApplicationProvider;
import com.team41.wildwanderer.database.*;
import com.team41.wildwanderer.database.AnimalSightingDAO;
import com.team41.wildwanderer.database.Database;
import com.team41.wildwanderer.database.UserDAO;
import com.team41.wildwanderer.database.UserStatsDAO;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.robolectric.RobolectricTestRunner;
import java.io.IOException;
import java.util.List;

import static junit.framework.TestCase.assertEquals;
import static junit.framework.TestCase.assertNull;
import static org.junit.Assert.assertNotNull;

/**
 * Tests to test if the room database used for testing functions when off campus wifi work
 * Author: Haico Maters
 */
@RunWith(RobolectricTestRunner.class)
public class RoomDatabaseUnitTests {

    private UserDAO userDAO;
    private Database db;
    private AnimalSightingDAO animalSightingDAO;

    private UserStatsDAO statsDAO;

    User testUser = new User("test", "test");
    User testUser2 = new User("test2", "test");

    AnimalSighting testSighting = new AnimalSighting("test", 1,2, testUser.username);

    UserStatistics testStats = new UserStatistics("test", testUser.username);
    UserStatistics testStatsMultipleSpecies = new UserStatistics("test2", testUser.username);
    UserStatistics testStatsDifferentUsers = new UserStatistics("test", testUser2.username);


    //Creates DB
    @Before
    public void createDB(){
        Context context = ApplicationProvider.getApplicationContext();
        db = Room.inMemoryDatabaseBuilder(context, Database.class).allowMainThreadQueries().build();
        userDAO = db.userDAO();
        animalSightingDAO = db.sightingDAO();
        statsDAO = db.statsDAO();
        userDAO.addUser(testUser);
    }

    //Closes DB
    @After
    public void closeDB() {
        db.close();
    }

    //User database Tests
    //Expected user with username test
    @Test
    public void getUserFromDB() {
        User dbUser = userDAO.getUser("test");
        assertEquals (dbUser.getUsername(), testUser.getUsername());
        assertEquals (dbUser.getPassword(), testUser.getPassword());
    }

    //Expected user with username test
    @Test
    public void getUserFromDBMultipleUsers(){
        userDAO.addUser(testUser2);
        User dbUser = userDAO.getUser("test");
        User dbUser2 = userDAO.getUser("test2");
        assertEquals (dbUser.getUsername(), testUser.getUsername());
        assertEquals(dbUser2.getUsername(), (testUser2.getUsername()));
        assertEquals(dbUser.getPassword(), (testUser.getPassword()));
        assertEquals(dbUser2.getPassword(), (testUser2.getPassword()));
    }

    //Expected remove user with name test
    @Test
    public void deleteUserFromDB() {
        assert (userDAO.getUser("test") != null);
        userDAO.deleteUser(userDAO.getUser("test"));
        assertNull(userDAO.getUser("test"));
    }


    //Animal Sighting Tests
    //Expected equal to testSighting
    @Test
    public void getAnimalSightingFromDB(){
        long sightingid = animalSightingDAO.addSighting(testSighting);
        AnimalSighting dbSighting = animalSightingDAO.getSighting((int) sightingid);
        assertEquals (dbSighting.getLatitude(), (testSighting.getLatitude()));
        assertEquals (dbSighting.getLongitude(), (testSighting.getLongitude()));
        assertEquals (dbSighting.getSpeciesName(), (testSighting.getSpeciesName()));
        assertEquals (dbSighting.getSightingNumber(), testSighting.getSightingNumber());
        assertEquals (dbSighting.getUser(), testSighting.getUser());
        assertEquals (dbSighting.getTimeSpotted(), (testSighting.getTimeSpotted()));
    }

    //Expected testsighting has updated sighting number
    @Test
    public void updateAnimalSighting(){
        long sightingid = animalSightingDAO.addSighting(testSighting);
        assertEquals (animalSightingDAO.getSighting((int) sightingid).getSightingNumber(), 1);
        animalSightingDAO.updateSightingNumber((int) sightingid);
        assertEquals (animalSightingDAO.getSighting((int) sightingid).getSightingNumber(), 2);
        animalSightingDAO.updateSightingNumber((int) sightingid);
        assertEquals (animalSightingDAO.getSighting((int) sightingid).getSightingNumber(), 3);
    }

    //Expected no testsighting in db
    public void removeAnimalSighting(){
        long sightingid = animalSightingDAO.addSighting(testSighting);
        assertNotNull (animalSightingDAO.getSighting((int) sightingid));
        animalSightingDAO.deleteSighting(animalSightingDAO.getSighting((int) sightingid));
        assertNull(animalSightingDAO.getSighting((int) sightingid));
    }


    //User Statistics Tests

    //Expected all stats of one user from db
    @Test
    public void getAllIndividualUserStatsFromDB(){
        statsDAO.addUserStat(testStats);
        statsDAO.addUserStat(testStatsMultipleSpecies);
        List<UserStatistics> userStats = statsDAO.getAllIndividualUserStats(testUser.username);
        assertEquals (userStats.get(0).getSpecies(), (testStats.getSpecies()));
        assertEquals (userStats.get(1).getSpecies(), (testStatsMultipleSpecies.getSpecies()));
    }

    //Expected only testUser stats returned none of testUser 2 stats returned
    @Test
    public void getAllIndividualUserStatsFromDBMultipleUsers(){
        userDAO.addUser(testUser2);
        statsDAO.addUserStat(testStats);
        statsDAO.addUserStat(testStatsMultipleSpecies);
        statsDAO.addUserStat(testStatsDifferentUsers);
        List<UserStatistics> userStats = statsDAO.getAllIndividualUserStats(testUser.username);
        assertEquals (userStats.size(), 2);
    }

    //Expected delete all ser stats from individual user
    @Test
    public void deleteAllIndividualUserStatsFromDB(){
        statsDAO.addUserStat(testStats);
        statsDAO.addUserStat(testStatsMultipleSpecies);
        List<UserStatistics> userStats = statsDAO.getAllIndividualUserStats(testUser.username);
        assertEquals (userStats.size(), 2);
        statsDAO.removeUserStats(userStats);
        assertEquals(statsDAO.getAllIndividualUserStats(testUser.username).size(), 0);
    }

    //Only one user stat deleted
    @Test
    public void deleteOnlySingleUserStatsFromDB(){
        userDAO.addUser(testUser2);
        statsDAO.addUserStat(testStats);
        statsDAO.addUserStat(testStatsMultipleSpecies);
        statsDAO.addUserStat(testStatsDifferentUsers);
        List<UserStatistics> userStats = statsDAO.getAllIndividualUserStats(testUser.username);
        List<UserStatistics> userStats2 = statsDAO.getAllIndividualUserStats(testUser2.username);
        assertEquals (userStats2.size(), 1);
        statsDAO.removeUserStats(userStats2);
        assertEquals (userStats.size(), 2);
        assertEquals (statsDAO.getAllIndividualUserStats(testUser2.username).size(), 0);
    }

    //Number of sighting incremented by one for one stat
    @Test
    public void updateNumberOfSightings(){
        statsDAO.addUserStat(testStats);
        List<UserStatistics> stats = statsDAO.getAllIndividualUserStats(testStats.getUser());
        assertEquals (stats.get(0).getSightings(), 1);
        statsDAO.updateNumberOfSightings(testStats.getUser(), testStats.getSpecies());
        stats = statsDAO.getAllIndividualUserStats(testStats.getUser());
        assertEquals (stats.get(0).getSightings(), 2);
        statsDAO.updateNumberOfSightings(testStats.getUser(), testStats.getSpecies());
        stats = statsDAO.getAllIndividualUserStats(testStats.getUser());
        assertEquals (stats.get(0).getSightings(), 3);
    }
}
