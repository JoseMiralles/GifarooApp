package android;

import android.os.Bundle;
import android.app.Activity;
import android.widget.*;
import android.view.View.*;
import android.view.View;

import com.github.hiteshsondhi88.libffmpeg.ExecuteBinaryResponseHandler;
import com.github.hiteshsondhi88.libffmpeg.FFmpeg;
import com.github.hiteshsondhi88.libffmpeg.FFmpegExecuteResponseHandler;
import com.github.hiteshsondhi88.libffmpeg.LoadBinaryResponseHandler;
import com.github.hiteshsondhi88.libffmpeg.exceptions.FFmpegCommandAlreadyRunningException;
import com.github.hiteshsondhi88.libffmpeg.exceptions.FFmpegNotSupportedException;

public class loadffmpeg extends Activity {
    
	@Override
    public void onCreate(Bundle savedInstanceState) {
        
		super.onCreate(savedInstanceState);
		final FFmpeg ffmpeg = FFmpeg.getInstance(this.getApplicationContext());

		final Toast toast = Toast.makeText(
		getApplicationContext(),
		"Running",
		Toast.LENGTH_LONG);

		new Runnable() {
                @Override
                public void run() {
                    //Process.setThreadPriority(Process.THREAD_PRIORITY_BACKGROUND);
                    try {
                        ffmpeg.loadBinary(new LoadBinaryResponseHandler() {

                            @Override
                            public void onFailure() {
								toast.setText("Failed");
								toast.show();
								//finish();
                            }

                            @Override
                            public void onSuccess() {
                                try {
									//finish();
									//boolean test = true;
									toast.setText("Success");
									toast.show();
                                } catch (Exception e) {
                                    e.printStackTrace();
                                }
                            }

                            @Override
                            public void onFinish() {
                            }
                        });
                    } catch (FFmpegNotSupportedException e) {
                        e.printStackTrace();
						throw new RuntimeException(e);
					}
                }
            }.run();

    }

	@Override
    protected void onPause(){
        super.onPause();
    }

    @Override
    public void onBackPressed(){
    }
}