package com.example.androidclient;

import android.app.Activity;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import java.io.IOException;
import java.net.InetAddress;
import java.net.UnknownHostException;
import java.util.ArrayList;

public class Login extends Activity {

    private EditText Name;
    private EditText Id;
    private EditText Batch;
    private EditText Dept;

    private Button Submit;

    String string1;
    String string2;
    String string3;
    String string4;




    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        Name = (EditText) findViewById(R.id.stName);
        Id = (EditText) findViewById(R.id.stId);
        Batch = (EditText) findViewById(R.id.stBatch);
        Dept = (EditText) findViewById(R.id.stDept);

        Submit = (Button) findViewById(R.id.stSubmit);

        Submit.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                register();
            }
        });
    }

    public void register() {

        initialize();

        if(!validate()) {
            Toast.makeText(this, "Submission Has Failed!", Toast.LENGTH_SHORT).show();
        }

        else {
            onSubmission();
        }
    }

    public void onSubmission() {
        Intent i = new Intent(this, MainActivity.class);
        string1 = Name.getText().toString();
        string2 = Id.getText().toString();
        string3 = Batch.getText().toString();
        string4 = Dept.getText().toString();
        i.putExtra("NAME", string1);
        i.putExtra("ID", string2);
        i.putExtra("BATCH", string3);
        i.putExtra("DEPT", string4);
        startActivity(i);
        Toast.makeText(this, "Submission Successful!", Toast.LENGTH_SHORT).show();
        finish();
    }

    public boolean validate() {
        boolean valid = true;

        if((string1.isEmpty()) || (string1.length() < 5) || (string1.length() > 32)) {
            Name.setError("Enter a Valid Name ( 5 < Character < 32 )");
            valid = false;
        }

        if((string2.isEmpty()) || (string2.length() < 9)) {
            Id.setError("Enter a Valid Id ( Character > 9 )");
            valid = false;
        }

        if((string3.isEmpty())) {
            Batch.setError("Enter Your Batch Number");
            valid = false;
        }

        if((string4.isEmpty()) || (string4.length() > 10)) {
            Dept.setError("Enter Your Department");
            valid = false;
        }

        return valid;
    }

    public void initialize() {
        string1 = Name.getText().toString().trim();
        string2 = Id.getText().toString().trim();
        string3 = Batch.getText().toString().trim();
        string4 = Dept.getText().toString().trim();
    }
}
