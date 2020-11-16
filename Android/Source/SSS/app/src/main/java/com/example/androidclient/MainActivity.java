package com.example.androidclient;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.Socket;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

public class MainActivity extends Activity {

	private static String stName, stID, stBatch, stDept, stmLink;
	private static String sDept, sBatch, sID;
	private static TextView Name, ID, Batch, Dept;
	private EditText editTextAddress, editTextPort;
	private static EditText stream;
	private Button buttonConnect, btnSend;
	private String message = " ";
	//private static String kq;
	private ClientTask myClientTask;
	private OnListener listener;
	private static boolean flag = true;
	private boolean pressed;
	Socket socket = null;

    public interface OnListener {
		void listener(String text);
	}

	public void addListener(OnListener listener) {
		this.listener = listener;
	}

    @SuppressLint("HandlerLeak")
	static Handler handler = new Handler() {
		@Override
		public void handleMessage(Message msg) {
			if (flag) {
				//kq = msg.obj.toString();
				stream.setText(msg.obj.toString());
			}
		}
	};

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);

		Name = (TextView) findViewById(R.id.name);
        ID = (TextView) findViewById(R.id.id);
        Batch = (TextView) findViewById(R.id.batch);
        Dept = (TextView) findViewById(R.id.dept);
		editTextAddress = (EditText) findViewById(R.id.address);
		editTextPort = (EditText) findViewById(R.id.port);
		buttonConnect = (Button) findViewById(R.id.connect);
		btnSend = (Button) findViewById(R.id.button1);
		stream = (EditText) findViewById(R.id.editText1);

        stName = getIntent().getExtras().getString("NAME");
        Name.setText("Name : " + stName);
        stID = getIntent().getExtras().getString("ID");
        ID.setText("ID : " + stID);
        stBatch = getIntent().getExtras().getString("BATCH");
        Batch.setText("Batch : " + stBatch + "th");
        stDept = getIntent().getExtras().getString("DEPT");
        Dept.setText("Dept of " + stDept);

        sDept = Dept.getText().toString();
        sBatch = Batch.getText().toString();
        sID = ID.getText().toString();

		buttonConnect.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				if(!pressed) {
                    myClientTask = new ClientTask(editTextAddress.getText()
                            .toString(), Integer.parseInt(editTextPort.getText()
                            .toString()));
                    myClientTask.execute();

                    buttonConnect.setText("Connected");
                    Toast.makeText(getApplicationContext(), "Click > Send Request > Ready", Toast.LENGTH_LONG).show();
                }
                pressed = true;
			}
		});
	}

    public class ClientTask extends AsyncTask<String, String, String> implements
			OnListener {

		String dstAddress;
		int dstPort;
		PrintWriter out1;


		ClientTask(String addr, int port) {
			dstAddress = addr;
			dstPort = port;
		}

		@Override
		protected void onProgressUpdate(String... values) {
			// TODO Auto-generated method stub
			super.onProgressUpdate(values);

		}

		@Override
		protected String doInBackground(String... params) {
			// TODO Auto-generated method stub

			try {

				socket = new Socket(dstAddress, dstPort);
				out1 = new PrintWriter(socket.getOutputStream(), true);
				out1.flush();

				BufferedReader in1 = new BufferedReader(new InputStreamReader(
						socket.getInputStream()));
				do {
					try {
						if (!in1.ready()) {
							if (message != null && message != "bye") {
								MainActivity.handler.obtainMessage(0, 0, -1,
										 message).sendToTarget();
								message = "";
							}

							if(message == "bye") {
                                Toast.makeText(getApplicationContext(), "You are Approved for the Test", Toast.LENGTH_LONG).show();
                            }
						}
						int num = in1.read();
						message += Character.toString((char) num);
					} catch (Exception classNot) {
					}

				} while (!message.equals("bye"));

				try {
					sendMessage("bye");
				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				} finally {
					socket.close();
				}

			} catch (IOException e) {
				e.printStackTrace();
			}
			return null;
		}

		@Override
		protected void onPostExecute(String result) {
			try {
				if (socket.isClosed()) {
					flag = false;
				}
			} catch (Exception e) {
				Toast.makeText(getApplicationContext(), "Import Error!", Toast.LENGTH_LONG).show();
			}

			super.onPostExecute(result);
		}

		@Override
		public void listener(String text) {
			// TODO Auto-generated method stub
			sendMessage(text);
		}

		void sendMessage(String msg) {
			try {
				out1.print(msg);
				out1.flush();
				if (!msg.equals("bye"))
					MainActivity.handler.obtainMessage(0, 0, -1, "Me: " + msg)
							.sendToTarget();
				//else
				//	MainActivity.handler.obtainMessage(0, 0, -1,
				//			"Disconnected!").sendToTarget();
			} catch (Exception ioException) {
				ioException.printStackTrace();
			}
		}

	}

	public void send(View v) {
	    if(pressed) {
            addListener(myClientTask);
            if (listener != null)
                listener.listener(stDept + " : " + stBatch + "th : " + stID);
            btnSend.setText("Request Sent");
            Toast.makeText(getApplicationContext(), "Click on Ready After Approval!", Toast.LENGTH_LONG).show();
        }

        if (!socket.isConnected()) {
            Toast.makeText(getApplicationContext(), "First Connect to Server!", Toast.LENGTH_LONG).show();
        }

        else {
            Toast.makeText(getApplicationContext(), "Click on Ready After Approval!", Toast.LENGTH_LONG).show();
        }

        pressed = false;
	}

	@Override
	protected void onDestroy() {
		// TODO Auto-generated method stub
		try {
			if (listener != null)
				listener.listener("bye");
			socket.close();
		} catch (Exception e) {
			// TODO: handle exception
		}
		super.onDestroy();
	}

	@Override
	protected void onStop() {
		// TODO Auto-generated method stub
		try {
			if (listener != null)
				listener.listener("bye");
			socket.close();
		} catch (Exception e) {
			// TODO: handle exception
		}
		super.onStop();
	}


	public void onClick(View v) {
        if(message.equals("bye")) {
            Intent i = new Intent(this, Stream.class);
			stmLink = stream.getText().toString();
            i.putExtra("Value", stmLink);
            startActivity(i);
            finish();
        }

        if(!message.equals("bye")) {
            Toast.makeText(getApplicationContext(), "Wait For Admin Approval!", Toast.LENGTH_LONG).show();
        }

        else
        {
            Toast.makeText(getApplicationContext(), "Click > Send Request", Toast.LENGTH_LONG).show();
        }
	}

}
