package com.example.androidclient;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;

public class Welcome extends Activity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_welcome);
    }

    public void login(View view) {

        Intent l = new Intent(this, Login.class);
        startActivity(l);
    }

    public void help(View view) {

        Intent h = new Intent(this, Help.class);
        startActivity(h);
    }

    public void developer(View view) {

        Intent d = new Intent(this, Developer.class);
        startActivity(d);
    }
}
