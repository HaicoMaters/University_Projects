package com.team41.wildwanderer;

import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import androidx.room.Room;
import com.team41.wildwanderer.database.Database;
import com.team41.wildwanderer.database.User;
import com.team41.wildwanderer.database.UserDAO;

/**
 * This class/activity provides the registration view to the user, and handles registration logic,
 * which then communicates with the Server.
 * Author: Jacob Pearson
 */
public class RegisterActivity extends AppCompatActivity {
    private EditText usernameEditText;
    private EditText passwordEditText;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        Button registerButton;
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        usernameEditText = findViewById(R.id.editTextUsername);
        passwordEditText = findViewById(R.id.editTextPassword);
        registerButton = findViewById(R.id.register_button);

        registerButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Thread thread = new Thread(new Runnable() {
                    @Override
                    public void run() {
                        String username = usernameEditText.getText().toString();
                        String password = passwordEditText.getText().toString();
                        ServerRequestGenerator requestGenerator = new ServerRequestGenerator();

                        if (register(username, password, requestGenerator)) {
                            finish();
                        }
                    }
                });
                thread.start();

                }
        });
    }

    public boolean register(String username, String password, ServerRequestGenerator requestGenerator){

        if(ServerRequestGenerator.makeRequest(requestGenerator.generateGetUserRequest(username)).equals("Failed to get user") && isEmailValid(username)){
            ServerRequestGenerator.makeRequest(requestGenerator.generateAddUserRequest(username, password));
            return true;
        } else if (!isEmailValid(username)) {
            runOnUiThread(new Runnable(){
                public void run() {
                    Toast.makeText(getApplicationContext(), "Please enter a valid email address.", Toast.LENGTH_SHORT).show();
                }
            });
            return false;
        } else{
            runOnUiThread(new Runnable(){
                public void run() {
                    Toast.makeText(getApplicationContext(), "This email is taken. Please login instead, or, use a different email.", Toast.LENGTH_SHORT).show();
                }
            });
            return false;
        }

    }
    boolean isEmailValid(CharSequence email) {
        return android.util.Patterns.EMAIL_ADDRESS.matcher(email).matches();
    }
}