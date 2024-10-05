package com.team41.wildwanderer;

import android.content.SharedPreferences;
import android.widget.Button;
import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import android.content.Intent;
import android.view.View;

/**
 * This class/activity provides the home/landing page of the app to the user, to allow navigation to the other views.
 * Author: Jacob Pearson
 */

public class MainActivity extends AppCompatActivity {
    Button logoutButton;
    Button loginButton;
    Button accountButton;
    Button registerButton;
    SharedPreferences sharedPreferences;
    boolean isLoggedIn;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        loginButton = findViewById(R.id.login_button);
        accountButton = findViewById(R.id.account_button);
        logoutButton = findViewById(R.id.logout_button);
        registerButton = findViewById(R.id.register_button);
        sharedPreferences = getSharedPreferences("MyPrefs", MODE_PRIVATE);

        isLoggedIn = sharedPreferences.getBoolean("isLoggedIn", false);
        if (isLoggedIn){
            loginButton.setVisibility(View.GONE);
            registerButton.setVisibility(View.GONE);
            accountButton.setVisibility(View.VISIBLE);
            logoutButton.setVisibility(View.VISIBLE);
        }
        else{
            loginButton.setVisibility(View.VISIBLE);
            registerButton.setVisibility(View.VISIBLE);
            accountButton.setVisibility(View.GONE);
            logoutButton.setVisibility(View.GONE);
        }


    }
    public void openMap(View view) {
        Intent intent = new Intent(this, MapsActivity.class);
        startActivity(intent);
    }

    public void openRecent(View view) {
        Intent intent = new Intent(this, RecentActivity.class);
        startActivity(intent);
    }

    public void openLogin(View view) {
        Intent intent = new Intent(this, LoginActivity.class);
        startActivity(intent);
    }

    public void openAccount(View view) {
        Intent intent = new Intent(this, AccountActivity.class);
        startActivity(intent);
    }

    public void openRegister(View view) {
        Intent intent = new Intent(this, RegisterActivity.class);
        startActivity(intent);
    }

    public void logout(View view){
        SharedPreferences.Editor editor = sharedPreferences.edit();
        editor.putBoolean("isLoggedIn", false);
        editor.remove("username");
        editor.apply();
        loginButton.setVisibility(View.VISIBLE);
        registerButton.setVisibility(View.VISIBLE);
        accountButton.setVisibility(View.GONE);
        logoutButton.setVisibility(View.GONE);
    }
}