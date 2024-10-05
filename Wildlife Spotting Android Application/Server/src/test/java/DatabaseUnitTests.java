import Database.*;
import org.hibernate.Session;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import java.util.List;

import static org.junit.Assert.*;

/**
 * Tests that the hibernate database are working
 * Author: Haico Maters
 */
public class DatabaseUnitTests {

    //Make Sure that there are no users with this name currently in the DB before running
    User testUser = new User("test1", "test");
    User testUser2 = new User("test2", "test");

    AnimalSighting testSighting = new AnimalSighting("test", 12.4564332,
            14.2323455343, testUser);

    AnimalSighting testSighting2= new AnimalSighting("test", 7.4564332,
            18.2323455343, testUser2);

    UserStatistics testStat1 = new UserStatistics("test1", testUser);
    UserStatistics testStat2 = new UserStatistics("test2", testUser);
    UserStatistics testStatOtherUser = new UserStatistics("test1", testUser2);

    //Pre-populates db the add methods have been tested visually
    @Before
    public void setUpDB(){
        UserDB.addUser(testUser);
        UserDB.addUser(testUser2);
        AnimalSightingDB.addEntry(testSighting);
        AnimalSightingDB.addEntry(testSighting2);
        UserStatisticsDB.newAnimalStat(testStat1);
        UserStatisticsDB.newAnimalStat(testStat2);
        UserStatisticsDB.newAnimalStat(testStatOtherUser);
    }

    //Clean the DB after each test
    @After
    public void cleanDB(){
        UserDB.removeUser(testUser.getUsername());
        UserDB.removeUser(testUser2.getUsername());
    }


    @Test
    public void successfulDbConnection() {
        Session session = CommonDB.getSessionFactory().openSession();
        assertNotNull (session);
    }

    //User DB tests
    //Expected to get user with username test1 and password test
    @Test
    public void getUser(){
        User dbuser = UserDB.getUser(testUser.getUsername());
        assertEquals (dbuser.getUsername(), testUser.getUsername());
        assertEquals (dbuser.getPassword(), testUser.getPassword());
   }

   //Expected to get test user 1 rather than test user 2
    @Test
    public void getCorrectUser(){
        User dbuser = UserDB.getUser("test1");
        assertEquals (dbuser.getPassword(), testUser.getPassword());
        assertEquals (dbuser.getUsername(), testUser.getUsername());
    }

    //Animal Sighting tests
    // Expected to get a list of sightings where the sightings include test sighting 1 and test sighting 2
    @Test
    public void getAllSighting(){
        List<AnimalSighting> sightings = AnimalSightingDB.getAllEntry();
        assertTrue(sightings.stream().anyMatch(p -> p.getSightingID() == testSighting.getSightingID()));
        assertTrue( sightings.stream().anyMatch(p -> p.getSightingID() == testSighting2.getSightingID()));
    }

    //Expected to get correct sighting from the database which is equal to test sighting 1
    @Test
    public void getIndividualSighting(){
        assertEquals( AnimalSightingDB.getIndividualEntry(testSighting.getSightingID()).getSightingID(),
                testSighting.getSightingID());
        assertEquals( AnimalSightingDB.getIndividualEntry(testSighting.getSightingID()).getLatitude(),
                testSighting.getLatitude());
        assertEquals( AnimalSightingDB.getIndividualEntry(testSighting.getSightingID()).getLongitude(),
                testSighting.getLongitude());
        assertEquals(AnimalSightingDB.getIndividualEntry(testSighting.getSightingID()).getSightings(),
                testSighting.getSightings());
        assertEquals( AnimalSightingDB.getIndividualEntry(testSighting.getSightingID()).getSpeciesName(),
                (testSighting.getSpeciesName()));
    }

    //Expected to increase the number of sightings test sighting 1 from the database by one
   @Test
   public void incrementSighting(){
        AnimalSightingDB.incrementSighting(testSighting.getSightingID());
        assertEquals (AnimalSightingDB.getIndividualEntry(testSighting.getSightingID()).getSightings(),
                testSighting.getSightings() + 1);
    }

    //Expected to remove sighting from db and then return null when trying to get the sighting after deletion
    @Test
    public void removeSighting(){
        AnimalSightingDB.removeEntry(testSighting.getSightingID());
        assertNull (AnimalSightingDB.getIndividualEntry(testSighting.getSightingID()));
    }

    //User Statistics Tests
    //Expected to get all user stats for individual user testuser and testuser2
    @Test
    public void getUserStats(){
        assertEquals (UserStatisticsDB.getAllUserStats(testUser.getUsername()).size(), 2);
        assertEquals (UserStatisticsDB.getAllUserStats(testUser2.getUsername()).size(), 1);
    }

    //Expected to increase the number of times an animal has been spotted by one for a specific user
    @Test
    public void incrementUserStat(){
        UserStatisticsDB.incrementAnimalStat(testStat1.getSpeciesName(), testStat1.getUser().getUsername());
        boolean updated = false;
        for (UserStatistics stat: UserStatisticsDB.getAllUserStats(testUser.getUsername())
        ) {
            if (stat.getId() == testStat1.getId() && stat.getSightingNumber() == testStat1.getSightingNumber() + 1) {
                updated = true;
                break;
            }
        }
        assertTrue (updated);
    }

    //Expected to add a new user statistic to the database for testuser
    @Test
    public void updateUserStatNotAlreadyExists(){
        UserStatistics notExist = new UserStatistics("test3", testUser);
        UserStatisticsDB.updateStats(notExist.getSpeciesName(), testUser.getUsername());
        assertEquals (UserStatisticsDB.getAllUserStats(testUser.getUsername()).size(), 3);
    }

    //Expected to increment a user stat is the database for testuser
    @Test
    public void updateUserStatAlreadyExists(){
        UserStatisticsDB.updateStats(testStat2.getSpeciesName(), testUser.getUsername());
        boolean updated = false;
        for (UserStatistics stat: UserStatisticsDB.getAllUserStats(testUser.getUsername())
        ) {
            if (stat.getId() == testStat2.getId() && stat.getSightingNumber() == testStat2.getSightingNumber() + 1) {
                updated = true;
                break;
            }
        }
        assertTrue (updated);
    }
}
