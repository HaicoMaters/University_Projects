package Database;

/*
Class to handle things related to user account database
Author Haico Maters
 */


import org.hibernate.HibernateException;
import org.hibernate.Session;

/**
 * Class used to create database queries for the user table
 * Author: Haico Maters
 */
public class UserDB {

    //get username
    public static User getUser(String username){
        Session session = CommonDB.getSessionFactory().openSession();
        User user = null; // = the query
        try{
            session.beginTransaction();
            user = session.get(User.class, username);
            session.getTransaction().commit();
        }
        catch (HibernateException e){
            session.getTransaction().rollback();
            System.err.println("* Error: User was not retrieved from the database *");
            e.printStackTrace();
        }
        finally {
            session.close();
        }
        return user;
    }

    //add user to the database
    public static void addUser(User newUser){
        Session session = CommonDB.getSessionFactory().openSession();
        try {
            session.beginTransaction();
            session.save(newUser);
            session.getTransaction().commit();
        }
        catch (HibernateException e){
            session.getTransaction().rollback();
            System.err.println("* Error: User was not added to database *");
            e.printStackTrace();
        }
        finally {
            session.close();
        }
    }

    public static void removeUser(String username){
        //Remove User from User Table
        Session session = CommonDB.getSessionFactory().openSession();
        try {
            session.beginTransaction();
            User removeSighting = session.get(User.class, username);
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
