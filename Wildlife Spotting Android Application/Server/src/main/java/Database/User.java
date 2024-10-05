package Database;

import com.sun.istack.NotNull;

import javax.persistence.*;
import java.util.List;

/**
 *Class to represent a user in the database
 *Author: Haico Maters
 */
@Entity(name="User")
public class User {

    @Id
    @Column
    @NotNull
    private String username;
    @Column
    @NotNull
    private String password;

    //Used to delete objects using user as a foreign key
    @OneToMany(mappedBy = "user", cascade = CascadeType.ALL)
    private List<UserStatistics> userStatisticsList;
    //Used to delete objects using user as a foreign key
    @OneToMany(mappedBy = "user", cascade = CascadeType.ALL)
    private List<AnimalSighting> animalSightingList;


    public User(String username, String password) {
        this.username = username;
        this.password = password;
    }

    public User() {

    }

    @Override
    public String toString() {
        return "User{" +
                "username='" + username + '\'' +
                '}';
    }

    public String getUsername() {
        return username;
    }


    public String getPassword() {return password;}
}
