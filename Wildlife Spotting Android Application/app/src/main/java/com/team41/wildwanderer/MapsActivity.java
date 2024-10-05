package com.team41.wildwanderer;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.graphics.drawable.Drawable;
import android.net.Uri;
import android.provider.Settings;
import android.view.View;
import android.widget.Button;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import com.team41.wildwanderer.database.AnimalSighting;
import org.jetbrains.annotations.NotNull;
import org.osmdroid.api.IMapController;
import org.osmdroid.config.Configuration;
import org.osmdroid.tileprovider.tilesource.TileSourceFactory;
import org.osmdroid.util.GeoPoint;
import org.osmdroid.views.CustomZoomButtonsController;
import org.osmdroid.views.MapView;
import org.osmdroid.views.overlay.Marker;
import org.osmdroid.views.overlay.OverlayItem;
import org.osmdroid.views.overlay.compass.CompassOverlay;
import org.osmdroid.views.overlay.mylocation.GpsMyLocationProvider;
import org.osmdroid.views.overlay.mylocation.MyLocationNewOverlay;

import java.util.List;

/**
 * This class/activity handles the Map view in the app, as well as providing a way to access the user's location.
 * Author: Jacob Pearson
 */
public class MapsActivity extends AppCompatActivity {
    private static final int MY_PERMISSIONS_REQUEST_LOCATION = 1;
    private static final int APP_SETTINGS_REQUEST = 2;
    MapView mMapView = null;
    MyLocationNewOverlay mLocationOverlay = null;


    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Configuration.getInstance().load(this, getPreferences(MODE_PRIVATE));
        Configuration.getInstance().setUserAgentValue(BuildConfig.APPLICATION_ID);
        setContentView(R.layout.activity_maps);

        mMapView = findViewById(R.id.mapview);
        mMapView.setTileSource(TileSourceFactory.MAPNIK);
        mMapView.getZoomController().setVisibility(CustomZoomButtonsController.Visibility.ALWAYS);
        mMapView.setMultiTouchControls(true);

        CompassOverlay compassOverlay = new CompassOverlay(this, mMapView);
        compassOverlay.enableCompass();
        mMapView.getOverlays().add(compassOverlay);

        // Map is centered at the USB to begin with
        GeoPoint startPoint = new GeoPoint(54.973703005590856, -1.6252467305139202);

        // Test sightings for the demonstrative video
        GeoPoint goat = new GeoPoint(54.990811855799066, -1.5929548435885197);
        GeoPoint rabbit = new GeoPoint(54.9910, -1.5936);

        Thread thread = new Thread(new Runnable() {
            @Override
            public void run() {
                List<AnimalSighting> sightings = getAllRecentSighting();
                for (int i = 0; i < sightings.size(); i++){
                    GeoPoint location = new GeoPoint(sightings.get(i).getLongitude(), sightings.get(i).getLatitude());
                    Drawable markerIcon = selectIcon(sightings.get(i).getSpeciesName());
                    String name = sightings.get(i).getSpeciesName();
                    WebpageInfo info = new WebpageInfo();
                    String response = info.wikipediaQuery(name);
                    String description = info.extractSnippet(response);
                    String url = info.extractURL(response);
                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            Marker marker = new Marker(mMapView);
                            marker.setPosition(location);
                            //marker.setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_CENTER);
                            marker.setIcon(markerIcon);
                            marker.setTitle(name + " detected\n" + description + "\n" + url);
                            mMapView.getOverlays().add(marker);
                        }
                    });
                }
            }
        });
        thread.start();


        // Not necessary, here as another addition.
        Marker startMarker = new Marker(mMapView);
        startMarker.setPosition(startPoint);
        startMarker.setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_CENTER);
        startMarker.setIcon(getResources().getDrawable(R.drawable.fish));
        mMapView.getOverlays().add(startMarker);
        startMarker.setTitle("Fish Detected");

        // For the demonstrative video - to avoid animal timeout
        Marker goatMarker = new Marker(mMapView);
        goatMarker.setPosition(goat);
        goatMarker.setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_CENTER);
        goatMarker.setIcon(getResources().getDrawable(R.drawable.mammal));
        mMapView.getOverlays().add(goatMarker);
        goatMarker.setTitle("Goat detected");

        Marker rabbitMarker = new Marker(mMapView);
        rabbitMarker.setPosition(rabbit);
        rabbitMarker.setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_CENTER);
        rabbitMarker.setIcon(getResources().getDrawable(R.drawable.mammal_rodent));
        mMapView.getOverlays().add(rabbitMarker);
        rabbitMarker.setTitle("Rabbit detected");


        IMapController mapController = mMapView.getController();
        mapController.setZoom(17.5);
        mapController.setCenter(startPoint);

        checkLocationPerms();

        mLocationOverlay = new MyLocationNewOverlay(new GpsMyLocationProvider(getApplicationContext()),mMapView);
        mMapView.getOverlays().add(mLocationOverlay);
    }

    @Override
    public void onResume(){
        super.onResume();
        mLocationOverlay.enableMyLocation();
        mLocationOverlay.enableFollowLocation();
    }
    @Override
    public void onPause(){
        super.onPause();
        mLocationOverlay.disableMyLocation();
        mLocationOverlay.disableFollowLocation();
    }

    public void openSubmit(View view){
        Intent intent = new Intent(this, SubmitActivity.class);
        startActivity(intent);
    }

    public void checkLocationPerms(){
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION)
                != PackageManager.PERMISSION_GRANTED) {
            if(shouldShowRequestPermissionRationale(Manifest.permission.ACCESS_FINE_LOCATION)){
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.setMessage("This app needs access to your location to function properly")
                        .setTitle("Location Permission Needed")
                        .setPositiveButton("OK", (dialog, id) ->
                            // Request the permission again
                            ActivityCompat.requestPermissions(MapsActivity.this,
                                    new String[]{Manifest.permission.ACCESS_FINE_LOCATION},
                                    MY_PERMISSIONS_REQUEST_LOCATION));

                AlertDialog dialog = builder.create();
                dialog.show();
            }
            else{
                ActivityCompat.requestPermissions(MapsActivity.this,
                        new String[]{Manifest.permission.ACCESS_FINE_LOCATION},
                        MY_PERMISSIONS_REQUEST_LOCATION);
            }

        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NotNull String[] permissions, @NotNull int[] grantResults){
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == MY_PERMISSIONS_REQUEST_LOCATION) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                // do something
            } else {
                if (!ActivityCompat.shouldShowRequestPermissionRationale(this, Manifest.permission.ACCESS_FINE_LOCATION)) {
                    // User has selected "Don't ask again/Deny", show a dialog that directs the user to the app settings
                    showAppSettingsDialog();

                }
            }
        }
    }

    private void showAppSettingsDialog() {
        // Show a dialog that directs the user to the app settings
        new AlertDialog.Builder(this)
                .setMessage("We need location to fully enhance your experience. You cannot use the app properly without location enabled.")
                .setPositiveButton("Settings", (dialog, which) -> {
                    Intent intent = new Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
                    Uri uri = Uri.fromParts("package", getPackageName(), null);
                    intent.setData(uri);
                    startActivityForResult(intent, APP_SETTINGS_REQUEST);
                })
                .setNegativeButton("Cancel", null)
                .show();
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
}




