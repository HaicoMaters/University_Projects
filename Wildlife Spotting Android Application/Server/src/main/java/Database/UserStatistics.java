package Database;

import com.sun.istack.NotNull;

import javax.persistence.*;

/**
 *Class to handle how user statistics are stored within the database
 *Author: Haico Maters
 */
@Entity(name = "UserStatistics")
public class UserStatistics {

    @MapKeyColumn
    @Id
    @GeneratedValue(strategy = GenerationType.AUTO)
    private int id;

    @Column
    @NotNull
    private String speciesName;

    @Column
    @NotNull
    private int sightingNumber;

    @NotNull
    @ManyToOne
    @JoinColumn(name = "user", referencedColumnName = "username")
    private User user;

    public UserStatistics(String speciesName, User user) {
        this.speciesName = speciesName;
        this.user = user;
        this.sightingNumber = 1;
    }

    public int getId() {
        return id;
    }

    public String getSpeciesName() {
        return speciesName;
    }

    public int getSightingNumber() {
        return sightingNumber;
    }

    public User getUser() {
        return user;
    }

    public UserStatistics() {

    }

    @Override
    public String toString() {
        return "UserStatistics{" +
                "id=" + id +
                ", speciesName='" + speciesName + '\'' +
                ", sightingNumber=" + sightingNumber +
                ", user=" + user.getUsername() +
                '}';
    }

    public void incrementSightings(){
        sightingNumber++;
    }
}
