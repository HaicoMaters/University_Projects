package com.team41.wildwanderer;

import android.Manifest;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageManager;
import android.location.Location;
import android.location.LocationManager;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import com.team41.wildwanderer.database.AnimalSighting;
import org.osmdroid.util.GeoPoint;

import java.util.List;

/**
 * This call handles the submission of animal sightings
 * Author: Haico Maters
 */
public class SubmitActivity extends AppCompatActivity {


    private EditText speciesEditText;
    private static final int MY_PERMISSIONS_REQUEST_LOCATION = 1;
    private double longitude;
    private double latitude;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        Button submitButton;
        Button socialMediaButton;
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_submit);
        submitButton = findViewById(R.id.submitSighting_button);
        speciesEditText = findViewById(R.id.speciesNameEditText);
        socialMediaButton = findViewById(R.id.social_media_submit_button);
        getLongitudeLatitude();
        SharedPreferences sharedPrefs = getSharedPreferences("MyPrefs", MODE_PRIVATE);
        submitButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (speciesEditText.getText().toString().trim().length() != 0) {
                    Thread thread = new Thread(new Runnable() {
                        @Override
                        public void run() {
                                submitSighting(speciesEditText.getText().toString(), sharedPrefs.getString("username", "Guest"));
                            }

                    });
                    thread.start();
                    Intent intent = new Intent(SubmitActivity.this, MainActivity.class);
                    startActivity(intent);
                    finish();
                }
                else {
                    Toast.makeText(getApplicationContext(), "Please enter the name of the animal species.", Toast.LENGTH_SHORT).show();
                }

            }
        });
        socialMediaButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(SubmitActivity.this, SocialMediaActivity.class);
                startActivity(intent);
                finish();
            }
        });
    }


    @Override
    protected void onResume() {
        super.onResume();
        getLongitudeLatitude();
    }

    public void getLongitudeLatitude(){
        LocationManager lm = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
        checkLocationPerms();
        try {
            Location location = lm.getLastKnownLocation(LocationManager.GPS_PROVIDER);
            if (location != null){
            latitude = location.getLatitude();
            longitude = location.getLongitude();
            }
        } catch (SecurityException e){
            e.printStackTrace();
        }
    }

    public void checkLocationPerms(){
        if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION)
                != PackageManager.PERMISSION_GRANTED) {
            if(shouldShowRequestPermissionRationale(android.Manifest.permission.ACCESS_FINE_LOCATION)){
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.setMessage("This app needs access to your location to function properly")
                        .setTitle("Location Permission Needed")
                        .setPositiveButton("OK", (dialog, id) ->
                                // Request the permission again
                                ActivityCompat.requestPermissions(SubmitActivity.this,
                                        new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION},
                                        MY_PERMISSIONS_REQUEST_LOCATION));
                AlertDialog dialog = builder.create();
                dialog.show();
            }
            else{
                ActivityCompat.requestPermissions(SubmitActivity.this,
                        new String[]{Manifest.permission.ACCESS_FINE_LOCATION},
                        MY_PERMISSIONS_REQUEST_LOCATION);
            }
        }
    }

    public double userDistanceFromLocationInMeters(double dbLatitude, double dbLongitude){
        GeoPoint userLocation = new GeoPoint(latitude, longitude);
        return userLocation.distanceToAsDouble(new GeoPoint(dbLatitude, dbLongitude));
    }

    public void submitSighting(String animalName, String username){
        boolean submitted = false;
        ServerRequestGenerator gen = new ServerRequestGenerator();
        String response = gen.makeRequest(gen.generateGetAllSightingsRequest());
        if (response.equals("Failed to get sightings")){
            runOnUiThread(new Runnable(){
                public void run() {
                    Toast.makeText(getApplicationContext(), "Failed to submit sighting please try again.", Toast.LENGTH_SHORT).show();
                }
            });
        }
        else {
            List<AnimalSighting> currentSightings = gen.responseToAllAnimalSighting(response);
            if (currentSightings!= null) {
                for (AnimalSighting as : currentSightings
                ) {
                    if (as.getSpeciesName().equals(animalName) && userDistanceFromLocationInMeters(
                            as.getLatitude(), as.getLongitude()) >= 300) {
                        gen.makeRequest(gen.generateUpdateSightingRequest(as.getSightingID()));
                        submitted = true;
                    }
                }
            }
            if (!submitted) {
                gen.makeRequest(gen.generateAddSightingRequest(animalName, latitude, longitude, username));
            }
            gen.makeRequest(gen.generateUpdateUserStats(animalName, username));
            runOnUiThread(new Runnable(){
                public void run() {
                    Toast.makeText(getApplicationContext(), "Your animal sighting was submitted.", Toast.LENGTH_SHORT).show();
                }
            });
        }
    }

}