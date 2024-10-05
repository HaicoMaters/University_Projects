package Database;

import org.hibernate.Session;
import org.hibernate.query.Query;

import java.util.List;

/**
 * Class used for UserStatistics table queries
 * Author: Haico Maters
 */
public class UserStatisticsDB {

    //Update stats in the database adds new entry if animal not spotted by user and increments number of sightings if has
    //been spotted already
    public static void updateStats(String species, String username){
        UserStatistics stat = null;
        Session session = CommonDB.getSessionFactory().openSession();
        try {
            session.beginTransaction();
            Query<UserStatistics> query = session.createQuery("FROM UserStatistics us WHERE us.user.username" +
                    " = :username AND us.speciesName = :species", UserStatistics.class);
            query.setParameter("username", username);
            query.setParameter("species", species);
            stat = query.uniqueResult();
            session.getTransaction().commit();
        }
        catch (Exception e){
            session.getTransaction().rollback();
            System.err.println("* Error: Failed to update user statistics *");
            e.printStackTrace();
        }
        finally {
            session.close();
        }
        if(stat != null){
            incrementAnimalStat(species, username);
        }
        else {
            UserStatistics animal = new UserStatistics(species, UserDB.getUser(username));
            newAnimalStat(animal);
        }
    }

    //Increments the number of animal spotted
    public static void incrementAnimalStat(String animal, String username){
        UserStatistics update;
        Session session = CommonDB.getSessionFactory().openSession();
        try {
            session.beginTransaction();
            Query<UserStatistics> query = session.createQuery("FROM UserStatistics us WHERE us.user.username" +
                    " = :username AND us.speciesName = :animal", UserStatistics.class);
            query.setParameter("username", username);
            query.setParameter("animal", animal);
            update = query.uniqueResult();
            if (update != null) {
                update.incrementSightings();
                session.update(update);
                session.getTransaction().commit();
            }
        }
        catch (Exception e){
            session.getTransaction().rollback();
            System.err.println("* Error: Failed to update user statistics *");
            e.printStackTrace();
        }
        finally {
            session.close();
        }
    }

    //Creates new entry of animal spotted
    public static void newAnimalStat(UserStatistics animal){
        Session session = CommonDB.getSessionFactory().openSession();
        try {
            session.beginTransaction();
            session.save(animal);
            session.getTransaction().commit();
        }
        catch (Exception e){
            session.getTransaction().rollback();
            System.err.println("* Error: Animal was not added to database *");
            e.printStackTrace();
        }
        finally {
            session.close();
        }
    }

    //Returns all the statistics of a specific user by looking at user stats
    public static List<UserStatistics> getAllUserStats(String username){
        List<UserStatistics> stats = null;
        Session session = CommonDB.getSessionFactory().openSession();
        try {
            session.beginTransaction();
            Query<UserStatistics> query = session.createQuery("FROM UserStatistics us WHERE us.user.username" +
                    " = :username", UserStatistics.class);
            query.setParameter("username", username);
            stats = query.getResultList();
            session.getTransaction().commit();
        } catch (Exception e) {
            session.getTransaction().rollback();
            System.err.println("* Error: User statistics couldn't be retrieved *");
            e.printStackTrace();
        } finally {
            session.close();
        }
        return stats;
    }

}
