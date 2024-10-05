package com.team41.wildwanderer;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.provider.MediaStore;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import com.facebook.*;
import com.facebook.appevents.AppEventsLogger;
import com.facebook.login.LoginResult;
import com.facebook.login.widget.LoginButton;
import com.facebook.share.model.SharePhoto;
import com.facebook.share.model.SharePhotoContent;
import com.facebook.share.widget.ShareDialog;

/**
 * Class for handling app social media interaction
 * Author: Haico Maters
 */
public class SocialMediaActivity extends AppCompatActivity {

    private static final int REQUEST_IMAGE_CAPTURE = 1;
    private static final int CAMERA_PERMISSION_REQUEST_CODE = 100;

    private Button postButton;
    CallbackManager callbackManager;

    private ShareDialog shareDialog;
    private AccessTokenTracker accessTokenTracker;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_social_media);

        // Initialize the Facebook SDK
        FacebookSdk.sdkInitialize(getApplicationContext());
        AppEventsLogger.activateApp(getApplication());

        LoginButton loginButton = findViewById(R.id.btnConnectFacebook);
        postButton = findViewById(R.id.postButton);

        shareDialog = new ShareDialog(this);

        // Set up Facebook login callback
        callbackManager = CallbackManager.Factory.create();
        loginButton.registerCallback(callbackManager, new FacebookCallback<LoginResult>() {
            @Override
            public void onSuccess(LoginResult loginResult) {
                AccessToken accessToken = loginResult.getAccessToken();
                String callbackUrl = accessToken.getGraphDomain();
                // Use the callbackUrl as needed
                System.out.println(callbackUrl);
                postButton.setVisibility(View.VISIBLE); // Show the button
            }

            @Override
            public void onCancel() {
                Toast.makeText(getApplicationContext(), "Failed to login.", Toast.LENGTH_SHORT).show();
            }

            @Override
            public void onError(FacebookException error) {
                Toast.makeText(getApplicationContext(), "There was an error please try again.", Toast.LENGTH_SHORT).show();
            }
        });
        if (isLoggedIn()){
            postButton.setVisibility(View.VISIBLE);
        }
        postButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (checkCameraPermission()) {
                    captureImage();
                    Intent intent = new Intent(SocialMediaActivity.this, SubmitActivity.class);
                    startActivity(intent);
                    finish();
                } else {
                    requestCameraPermission();
                }
            }
        });
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (isLoggedIn()){
            postButton.setVisibility(View.VISIBLE);
        }
        else {postButton.setVisibility(View.GONE);}
    }

    public boolean isLoggedIn() {
        AccessToken accessToken = AccessToken.getCurrentAccessToken();
        return accessToken != null;
    }

    private void captureImage() {
        Intent takePictureIntent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
        if (takePictureIntent.resolveActivity(getPackageManager()) != null) {
            startActivityForResult(takePictureIntent, REQUEST_IMAGE_CAPTURE);
        }
        else {
            Toast.makeText(this, "No camera app available", Toast.LENGTH_SHORT).show();
        }
    }

    private boolean checkCameraPermission() {
        int cameraPermission = ContextCompat.checkSelfPermission(this, android.Manifest.permission.CAMERA);
        return cameraPermission == PackageManager.PERMISSION_GRANTED;
    }

    private void requestCameraPermission() {
        ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.CAMERA}, CAMERA_PERMISSION_REQUEST_CODE);
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == CAMERA_PERMISSION_REQUEST_CODE) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                captureImage();
            } else {
                Toast.makeText(getApplicationContext(), "Permission denied", Toast.LENGTH_SHORT);
            }
        }
    }

    //After taking photo automatically passes image to new post
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == REQUEST_IMAGE_CAPTURE && resultCode == RESULT_OK) {
            // Get the captured image as a Bitmap
            Bundle extras = data.getExtras();
            Bitmap imageBitmap = (Bitmap) extras.get("data");

            SharePhoto photo = new SharePhoto.Builder()
                    .setBitmap(imageBitmap).setCaption("I found this animal seen in the photo" )
                    .build();

            SharePhotoContent content = new SharePhotoContent.Builder()
                    .addPhoto(photo)
                    .build();


            shareDialog.show(content);
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        accessTokenTracker.stopTracking();
    }

}