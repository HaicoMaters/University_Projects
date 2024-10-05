package Database;

import org.hibernate.SessionFactory;
import org.hibernate.cfg.Configuration;

/**
 *This class is used for starting up the hibernate DB
 *Author: Haico Maters
 */
public class CommonDB {

    private static final SessionFactory sessionFactory = buildSessionFactory();

    //Builds the session factory which is used to open a session to connect to the database
    private static SessionFactory buildSessionFactory(){

        try{
            return new Configuration().configure().buildSessionFactory();
        }
        catch (Exception e){
            System.err.println("Initial SessionFactory creation failed." + e);
            throw new ExceptionInInitializerError(e);
        }
    }

    //Function used to return the session factory to be used to establish a database connection
    public static SessionFactory getSessionFactory() {
        return sessionFactory;
    }
}
