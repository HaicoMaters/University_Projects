package com.team41.wildwanderer;

import android.graphics.drawable.Drawable;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import com.team41.wildwanderer.database.AnimalSighting;

import java.time.LocalDateTime;
import java.time.temporal.ChronoUnit;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;

/**
 * This class/activity displays the 5 most recent sightings by other users to the user. It also contains
 * logic for selecting the correct icon to display.
 * Author: Jacob Pearson + Haico Maters
 */
public class RecentActivity extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_recent);
        Thread thread = new Thread(new Runnable() {
            @Override
            public void run() {
                List<AnimalSighting> sightings;
                sightings = getAllRecentSighting();
                sightings.sort(Comparator.comparing(AnimalSighting::getSightingID));
                List<Drawable> icons = new ArrayList<>();

                if (sightings.isEmpty()){
                    runOnUiThread(new Runnable(){
                        public void run() {
                            Toast.makeText(getApplicationContext(),"No sightings to show!", Toast.LENGTH_SHORT).show();
                        }
                    });
                }
                else{
                    for (int i = 0; i<sightings.size(); i++){
                        icons.add(selectIcon(sightings.get(i).getSpeciesName()));
                    }
                    runOnUiThread(new Runnable(){
                        public void run() {
                            int numSightings = Math.min(sightings.size(), 5); // Limit the number of sightings to display
                            for (int i = 0; i < numSightings; i++) {
                                AnimalSighting sighting = sightings.get(i); // Get the current sighting

                                // Get references to the UI elements dynamically based on the loop index
                                TextView sightingName = findViewById(getResources().getIdentifier("sighting_name" + (i + 1), "id", getPackageName()));
                                TextView sightingInfo = findViewById(getResources().getIdentifier("sighting_info" + (i + 1), "id", getPackageName()));
                                ImageView sightingLogo = findViewById(getResources().getIdentifier("sighting_logo" + (i + 1), "id", getPackageName()));

                                // Set the properties of the UI elements
                                sightingName.setText(sighting.getSpeciesName());
                                sightingName.setVisibility(View.VISIBLE);
                                sightingInfo.setText(minutesAgoSpotted(sighting.getTimeSpotted()));
                                sightingInfo.setVisibility(View.VISIBLE);
                                sightingLogo.setImageDrawable(icons.get(i));
                                sightingLogo.setVisibility(View.VISIBLE);
                            }

                        }
                    });

                }

            }

        });
        thread.start();


    }

    public List<AnimalSighting> getAllRecentSighting(){
        ServerRequestGenerator gen = new ServerRequestGenerator();
        return gen.responseToAllAnimalSighting(gen.makeRequest(gen.generateGetAllSightingsRequest()));
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


    public String minutesAgoSpotted(String timeSpotted){
        return "Spotted: " + ChronoUnit.MINUTES.between(LocalDateTime.now(), LocalDateTime.parse(timeSpotted)) + " minutes ago";
    }


}