package com.team41.wildwanderer;

import android.content.DialogInterface;
import android.content.SharedPreferences;
import android.graphics.drawable.Drawable;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import com.team41.wildwanderer.R;
import com.team41.wildwanderer.database.User;
import com.team41.wildwanderer.database.UserStatistics;

import java.util.List;

/**
 * This class/activity provides the user with a way to see their own statistics whilst logged in, as well as delete
 * their account.
 * Author: Haico Maters + Jacob Pearson
 */

public class AccountActivity extends AppCompatActivity {
    private TextView username_title;
    private TextView most_species_name;
    private TextView most_species_amount;
    private ImageView most_species_logo;
    private List<UserStatistics> stats;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_account);
        Button delete_button = findViewById(R.id.delete_button);
        username_title = findViewById(R.id.username_title);
        most_species_amount = findViewById(R.id.most_species_times);
        most_species_logo = findViewById(R.id.most_logo);
        most_species_name = findViewById(R.id.most_species_name);
        SharedPreferences sharedPreferences = getSharedPreferences("MyPrefs", MODE_PRIVATE);
        String username = sharedPreferences.getString("username", "Username");
        Thread thread = new Thread(new Runnable() {
            @Override
            public void run() {
                List<UserStatistics> stats = getAllUserStats(username);
                UserStatistics most_common = getMostCommonUserStat(stats);
                Drawable icon = selectIcon(most_common.getSpecies());
                runOnUiThread(new Runnable(){
                    public void run() {
                        if (stats != null && !stats.isEmpty()){
                            most_species_name.setText(most_common.getSpecies());
                            most_species_amount.setText(String.valueOf(most_common.getSightings()));
                            most_species_logo.setImageDrawable(icon);
                        }
                    }
                });

            }
        });
        thread.start();
        int atIndex = username.indexOf("@");
        String name = username.substring(0, atIndex);
        username_title.setText(name);
        delete_button.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                showDeleteConfirm(username);
            }
        });
    }

    public List<UserStatistics> getAllUserStats(String username){
        ServerRequestGenerator gen = new ServerRequestGenerator();
        return gen.responseToALlUserStatistics(gen.makeRequest(gen.generateGetUserStatsRequest(username)));
    }

    public UserStatistics getMostCommonUserStat(List<UserStatistics> stats){
        int currentHighest = 0;
        UserStatistics currentHighestStat = null;
        for (UserStatistics stat: stats
             ) {
            if (stat.getSightings() > currentHighest){
                currentHighest = stat.getSightings();
                currentHighestStat = stat;
            }
        }
        return currentHighestStat;
    }

    /**
     * This is a function that displays a dialog box for deletion of an account.
     * @param username - grabbed from sharedprefs
     *                 Author: Jacob Pearson
     */
    private void showDeleteConfirm(String username){ //TODO need way to logout user when delete happens.
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle("WARNING")
                .setMessage("Deleting your account is irreversible - are you sure you want to continue?")
                .setPositiveButton("Yes", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        Thread thread = new Thread(new Runnable() {
                            @Override
                            public void run() {
                                ServerRequestGenerator gen = new ServerRequestGenerator();
                                String response = gen.makeRequest(gen.generateDeleteUserRequest(username));
                                runOnUiThread(new Runnable() {
                                    @Override
                                    public void run() {
                                        System.out.println(response);
                                        dialog.dismiss();
                                    }
                                });
                            }
                        });
                        thread.start();
                        finish();
                    }
                })
                .setNegativeButton("No", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        dialog.dismiss();
                    }
                });
        AlertDialog dialog = builder.create();
        dialog.show();
    }

    public void setStats(List<UserStatistics> stats) {
        this.stats = stats;
    }

    public Drawable selectIcon(String speciesName){
        WebpageInfo info = new WebpageInfo();
        String url = info.extractURL(info.wikipediaQuery(speciesName));
        String webText = info.getWholePageText(url);
        String classFilter = info.filterClass(url);
        switch (classFilter){
            case "Fish-Like":
                return getResources().getDrawable(R.drawable.fish);
            case "Bird-Like":
                if (webText.toLowerCase().contains("bird of prey") || webText.toLowerCase().contains("birds of prey"))
                {return getResources().getDrawable(R.drawable.bird_prey);}
                if (webText.toLowerCase().contains("aquatic") || webText.toLowerCase().contains("waterbird"))
                {return getResources().getDrawable(R.drawable.bird_aq);}
                if (webText.toLowerCase().contains("songbird")) {return getResources().getDrawable(R.drawable.bird_song);}
                return getResources().getDrawable(R.drawable.bird_common);
            case "Mammal-Like":
                if (webText.toLowerCase().contains("rodentia")){return getResources().getDrawable(R.drawable.mammal_rodent);}
                if (webText.toLowerCase().contains("aquatic")){return getResources().getDrawable(R.drawable.mammal_aq);}
                return getResources().getDrawable(R.drawable.mammal);
            case "Amphibian-Like":
                return getResources().getDrawable(R.drawable.amphibian);
            case "Bug-Like":
                return getResources().getDrawable(R.drawable.insect);
            case "Reptile-Like":
                return getResources().getDrawable(R.drawable.reptile);
            default: return getResources().getDrawable(R.drawable.empty);
        }
    }
}