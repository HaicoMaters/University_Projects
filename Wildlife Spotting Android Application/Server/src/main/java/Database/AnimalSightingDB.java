package Database;

/**
 *Class to handle queries related to animal sighting database
 *Author: Haico Maters
 */


import org.hibernate.HibernateException;
import org.hibernate.Session;

import java.util.List;

public class AnimalSightingDB {

    //Find animal of sightings
    public static List<AnimalSighting> getAllEntry(){
        Session session = CommonDB.getSessionFactory().openSession();
        List<AnimalSighting> sightings = null;
        try{
            session.beginTransaction();
            sightings = (List<AnimalSighting>) session.createSQLQuery("SELECT * FROM AnimalSighting")
                    .addEntity(AnimalSighting.class).list();
            session.getTransaction().commit();

        }
        catch (HibernateException e){
            session.getTransaction().rollback();
            System.err.println("* Error: Animal Sighting was not retrieved from the database *");
            e.printStackTrace();
        }
        finally {
            session.close();
        }

        //query database by sighting id
        return sightings;
    }

    //Find animal of sightings
    public static AnimalSighting getIndividualEntry(int id){
        Session session = CommonDB.getSessionFactory().openSession();
        AnimalSighting sighting = null;
        try{
            session.beginTransaction();
            sighting = session.get(AnimalSighting.class, id);
            session.getTransaction().commit();

        }
        catch (HibernateException e){
            session.getTransaction().rollback();
            System.err.println("* Error: Animal Sighting was not retrieved from the database *");
            e.printStackTrace();
        }
        finally {
            session.close();
        }

        //query database by sighting id
        return sighting;
    }

    //Add a new sighting to the database
    public static void addEntry(AnimalSighting sighting){

        Session session = CommonDB.getSessionFactory().openSession();

        try {
            session.beginTransaction();
            session.save(sighting);
            session.getTransaction().commit();
        }
        catch (HibernateException e){
            session.getTransaction().rollback();
            System.err.println("* Error: Animal Sighting was not added to database *");
            e.printStackTrace();
        }
        finally {
            session.close();
        }
    }

    //Increment number of sightings to extend the time limit
    public static void incrementSighting(int entryID){
        Session session = CommonDB.getSessionFactory().openSession();
        try {
            session.beginTransaction();
            AnimalSighting incrementSighting = session.get(AnimalSighting.class, entryID);
            try {
                incrementSighting.incrementSightings();
                session.update(incrementSighting);
            }
            catch (IllegalArgumentException e){
                System.err.println(" Sighting couldn't be found with inputted ID");
            }
            session.getTransaction().commit();
        }
        catch (HibernateException e){
            session.getTransaction().rollback();
            System.err.println("* Error: Animal Sighting was not updated in the database *");
            e.printStackTrace();
        }
        finally {
            session.close();
        }
    }

    //Remove a sighting from the database
    public static void removeEntry(int entryID){

        Session session = CommonDB.getSessionFactory().openSession();
        try {
            session.beginTransaction();
            AnimalSighting removeSighting = session.get(AnimalSighting.class, entryID);
            try {
                session.delete(removeSighting);
            }
            catch (IllegalArgumentException e){
                System.err.println(" Sighting couldn't be found with inputted ID");
            }
            session.getTransaction().commit();
        }
        catch (HibernateException e){
            session.getTransaction().rollback();
            System.err.println("* Error: Animal Sighting was not removed from the database *");
            e.printStackTrace();
        }
        finally {
            session.close();
        }
    }


}
