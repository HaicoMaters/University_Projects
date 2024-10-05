package com.team41.wildwanderer;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Looper;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;

/**
 * This class/activity provides the login view to the user, which communicates with the Server.
 * Author: Jacob Pearson
 */

public class LoginActivity extends AppCompatActivity {
    private EditText usernameEditText;
    private EditText passwordEditText;
    private Button loginButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        usernameEditText = findViewById(R.id.editTextUsername);
        passwordEditText = findViewById(R.id.editTextPassword);
        loginButton = findViewById(R.id.buttonLogin);

        loginButton.setOnClickListener(new View.OnClickListener(){
            @Override
            public void onClick(View v){
                Thread thread = new Thread(new Runnable() {
                    @Override
                    public void run() {
                        login();
                    }

                });
                thread.start();
            }
        });
    }

    private void login(){
        String username = usernameEditText.getText().toString();
        String password = passwordEditText.getText().toString();
        ServerRequestGenerator requestGenerator = new ServerRequestGenerator();
        String response = ServerRequestGenerator.makeRequest(requestGenerator.generateLoginRequest(username, password));
        if (!response.equals("Login process failed")){
            if (response.equals("User Credentials Correct")) {
                // Login successful
                runOnUiThread(new Runnable(){
                    public void run() {
                        Toast.makeText(getApplicationContext(), "Login successful", Toast.LENGTH_SHORT).show();
                    }
                });

                SharedPreferences sharedPreferences = getSharedPreferences("MyPrefs", MODE_PRIVATE);
                SharedPreferences.Editor editor = sharedPreferences.edit();
                editor.putBoolean("isLoggedIn", true);
                editor.putString("username", username);
                editor.apply();
                // Start the next activity (e.g., MainActivity)
                Intent intent = new Intent(LoginActivity.this, MainActivity.class);
                startActivity(intent);


                // Finish the login activity
                finish();

            } else {
                // Login failed
                runOnUiThread(new Runnable(){
                    public void run() {
                        Toast.makeText(getApplicationContext(), "Login failed. Please try again.", Toast.LENGTH_SHORT).show();
                    }
                });
            }

        }
        else{
            runOnUiThread(new Runnable(){
                public void run() {
                    Toast.makeText(getApplicationContext(), "This account does not exist, please register first.", Toast.LENGTH_SHORT).show();;
                }
            });
        }


    }
}