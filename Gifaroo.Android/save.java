package android;

import android.content.Context;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.TextView;
import android.widget.Toast;

import android.os.Bundle;
import android.app.Activity;
import android.widget.*;
import android.view.View.*;
import android.view.View;
import com.gifaroo.gifaroo.R;

import com.github.hiteshsondhi88.libffmpeg.ExecuteBinaryResponseHandler;
import com.github.hiteshsondhi88.libffmpeg.FFmpeg;
import com.github.hiteshsondhi88.libffmpeg.FFmpegExecuteResponseHandler;
import com.github.hiteshsondhi88.libffmpeg.LoadBinaryResponseHandler;
import com.github.hiteshsondhi88.libffmpeg.exceptions.FFmpegCommandAlreadyRunningException;
import com.github.hiteshsondhi88.libffmpeg.exceptions.FFmpegNotSupportedException;

import android.app.ProgressDialog;
import android.app.AlertDialog;
import android.app.AlertDialog.Builder;
import android.content.DialogInterface;
import android.content.Context;
import android.content.Intent;
import android.graphics.BitmapFactory;
import android.graphics.Movie;
import android.net.Uri;
import android.os.*;
import android.os.Process;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.media.MediaScannerConnection;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.util.UUID;

public class save extends Activity {

    Context context;
    ProgressDialog progressD = null;
    Boolean shouldRunffmpeg = true;
    Activity activity = null;
    boolean threadsCanceled = false;
    String finalFileName = "";
    String outputName = "";
    String rawFileName = "";
    String finalFileExtension = "";

    //Used to keep the phone from going to sleep.
    PowerManager pm = null;
    PowerManager.WakeLock wl = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.save);
        //TODO: do not remove the above two lines

        pm = (PowerManager) getSystemService(Context.POWER_SERVICE);
        wl = pm.newWakeLock(PowerManager.SCREEN_DIM_WAKE_LOCK, "My Tag");

        getActionBar().setTitle("");

        Button shareButton = (Button) findViewById(R.id.shareButton);

        activity = save.this;
        context = this.getApplicationContext();
        final FFmpeg ffmpeg = FFmpeg.getInstance(this.getApplicationContext());

        finalFileExtension = ".mp4";
        rawFileName = getFullFileNameFromArray("my_file", getFilesDir().list());
        finalFileName = "Gifaroo" + UUID.randomUUID() + finalFileExtension;
        String commandDimensions = getResizeCommandDimensions(context);
        createTxtFile(context);
        String internalPath = getFilesDir().getPath();
        outputName = "final" + finalFileExtension;

        //Attempt to add the mute mp3 file called "10sec.mp3" to private dir if it does not exist yet.
        if (new File(getFilesDir().getPath(), "10sec.mp3").exists() == false){
            try {
                CopyFromAssetsToStorage(context, "10sec.mp3", "10sec.mp3");
            } catch (IOException e) {
                e.printStackTrace();
            }
        }

        shareButton.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {

                createVideoShareIntent11(context, outputName);

            }
        });

        // removed libx264 -r 30 .........after -codec:a
        final String[] commands = new String[]{
                "-y -i "+internalPath+"/"+rawFileName+" -vf scale="+ commandDimensions +" -preset superfast "+internalPath+"/output_mp4.mp4",
                "-y -i "+internalPath+"/background.jpg -vf scale=700:700 "+internalPath+"/background_scaled.jpg",
                "-y -i "+internalPath+"/top.png -vf scale=700:700 "+internalPath+"/top_scaled.png",
                "-y -loop 1 -i "+internalPath+"/background_scaled.jpg -i "+internalPath+"/output_mp4.mp4 -filter_complex overlay=(main_w-overlay_w)/2:(main_h-overlay_h)/2:shortest=1 -preset superfast -codec:a copy -movflags +faststart "+internalPath+"/output_1.mp4",
                "-y -i "+internalPath+"/output_1.mp4 -loop 1 -i "+internalPath+"/top_scaled.png -filter_complex overlay=(main_w-overlay_w)/2:(main_h-overlay_h)/2:shortest=1 -preset superfast -codec:a copy -movflags +faststart "+internalPath+"/final_unlooped.mp4",
                "-y -f concat -i "+internalPath+"/myList.txt -c copy "+internalPath+"/mute_final.mp4",
                "-i "+internalPath+"/mute_final.mp4 -i "+internalPath+"/10sec.mp3 -c:v libx264 -crf 19 -preset ultrafast -shortest -c:a aac -strict experimental -pix_fmt yuv420p -f mp4 -b:a 192k -y " +internalPath+ "/" +outputName
        };
        final String[] progressMessages = {
                "Doing some work...",
                "Making progress...",
                "Just a little bit more...",
                "Adding some final touches..."
        };

        final FFmpegExecuteResponseHandler handler7 = new FFmpegExecuteResponseHandler() {
            @Override
            public void onSuccess(String s) {
                createVideoShareIntent11(context, outputName);
            }

            @Override
            public void onProgress(String s) {
            }

            @Override
            public void onFailure(String s) {
            	Log.d("LAST_COMMAND", "FAILED:\n" + s);
                Toast.makeText(context, "Sorry, something went wrong.", Toast.LENGTH_LONG);
                finish();
            }

            @Override
            public void onStart() {
            }

            @Override
            public void onFinish() {
                progressD.dismiss();
                wl.release();
            }
        };
        final FFmpegExecuteResponseHandler handler6 = new FFmpegExecuteResponseHandler() {
            @Override
            public void onSuccess(String s) {
                //createVideoShareIntent11(context, outputName);
            }

            @Override
            public void onProgress(String s) {
            }

            @Override
            public void onFailure(String s) {
                Toast.makeText(context, "Sorry, something went wrong.", Toast.LENGTH_LONG);
                finish();
            }

            @Override
            public void onStart() {
            }

            @Override
            public void onFinish() {

                if(threadsCanceled == false)
                    new Runnable() {
                        @Override
                        public void run() {
                            Process.setThreadPriority(Process.THREAD_PRIORITY_BACKGROUND);
                            try {
                                ffmpeg.execute(commands[6], handler7);
                            } catch (FFmpegCommandAlreadyRunningException e) {
                                e.printStackTrace();
                            }
                        }
                    }.run();

            }
        };
        final FFmpegExecuteResponseHandler handler5 = new FFmpegExecuteResponseHandler() {
            @Override
            public void onSuccess(String s) {
            }

            @Override
            public void onProgress(String s) {

            }

            @Override
            public void onFailure(String s) {
            }

            @Override
            public void onStart() {
            }

            @Override
            public void onFinish() {
                progressD.setMessage(progressMessages[3]);

                try{
                    FileInputStream fis = context.openFileInput("myList.txt");
                    InputStreamReader isr = new InputStreamReader(fis);
                    BufferedReader bufferedReader = new BufferedReader(isr);
                    StringBuilder sb = new StringBuilder();
                    String line;
                    while ((line = bufferedReader.readLine()) != null) {
                        sb.append(line);
                    }
                    fis.close();

                } catch(Exception e){
                    e.printStackTrace();
                }

                if(threadsCanceled == false)
                new Runnable() {
                    @Override
                    public void run() {
                        Process.setThreadPriority(Process.THREAD_PRIORITY_BACKGROUND);
                        try {
                            ffmpeg.execute(commands[5], handler6);
                        } catch (FFmpegCommandAlreadyRunningException e) {
                            e.printStackTrace();
                        }
                    }
                }.run();

            }
        };
        final FFmpegExecuteResponseHandler handler4 = new FFmpegExecuteResponseHandler() {
            @Override
            public void onSuccess(String s) {
            }

            @Override
            public void onProgress(String s) {

            }

            @Override
            public void onFailure(String s) {
            }

            @Override
            public void onStart() {
            }

            @Override
            public void onFinish() {
                progressD.setMessage(progressMessages[2]);

                if(threadsCanceled == false)
                new Runnable() {
                    @Override
                    public void run() {
                        Process.setThreadPriority(Process.THREAD_PRIORITY_BACKGROUND);
                        try {
                            ffmpeg.execute(commands[4], handler5);
                        } catch (FFmpegCommandAlreadyRunningException e) {
                            e.printStackTrace();
                        }
                    }
                }.run();
            }
        };
        final FFmpegExecuteResponseHandler handler3 = new FFmpegExecuteResponseHandler() {
            @Override
            public void onSuccess(String s) {
            }

            @Override
            public void onProgress(String s) {

            }

            @Override
            public void onFailure(String s) {
            }

            @Override
            public void onStart() {
            }

            @Override
            public void onFinish() {
                progressD.setMessage(progressMessages[1]);

                if(threadsCanceled == false)
                new Runnable() {
                    @Override
                    public void run() {
                        Process.setThreadPriority(Process.THREAD_PRIORITY_BACKGROUND);
                        try {
                            ffmpeg.execute(commands[3], handler4);
                        } catch (FFmpegCommandAlreadyRunningException e) {
                            e.printStackTrace();
                        }
                    }
                }.run();
            }
        };
        final FFmpegExecuteResponseHandler handler2 = new FFmpegExecuteResponseHandler() {
            @Override
            public void onSuccess(String s) {
            }

            @Override
            public void onProgress(String s) {

            }

            @Override
            public void onFailure(String s) {
            }

            @Override
            public void onStart() {
            }

            @Override
            public void onFinish() {
                progressD.setMessage(progressMessages[1]);

                if(threadsCanceled == false)
                new Runnable() {
                    @Override
                    public void run() {
                        Process.setThreadPriority(Process.THREAD_PRIORITY_BACKGROUND);
                        try {
                            ffmpeg.execute(commands[2], handler3);
                        } catch (FFmpegCommandAlreadyRunningException e) {
                            e.printStackTrace();
                        }
                    }
                }.run();
            }
        };
        final FFmpegExecuteResponseHandler handler1 = new FFmpegExecuteResponseHandler() {

            String shitToPrint = "";

            @Override
            public void onSuccess(String s) {
                shitToPrint += s;
            }

            @Override
            public void onProgress(String s) {

            }

            @Override
            public void onFailure(String s) {
                shitToPrint += s;
            }

            @Override
            public void onStart() {
            }

            @Override
            public void onFinish() {

                if(threadsCanceled == false)
                new Runnable() {
                    @Override
                    public void run() {
                        Process.setThreadPriority(Process.THREAD_PRIORITY_BACKGROUND);
                        try {
                            ffmpeg.execute(commands[1], handler2);
                        } catch (FFmpegCommandAlreadyRunningException e) {
                            e.printStackTrace();
                        }
                    }
                }.run();

            }
        };

        ///Start the ffmpeg loader which in turn starts the execute commands when it's finished.
        if (shouldRunffmpeg == true) { //prevents the activity from re running this bit of code when it returns from other apps
        	wl.acquire();
            progressD = new ProgressDialog(this);
            progressD.setMessage(progressMessages[0]);
            //progressD.getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
            progressD.setCanceledOnTouchOutside(false);
            progressD.setOnCancelListener(new DialogInterface.OnCancelListener() {
                @Override
                public void onCancel(DialogInterface dialog) {
                    AlertDialog.Builder builder = new AlertDialog.Builder(activity);
                    builder.setMessage("Are you sure that you want to cancel?")
                            .setTitle("Cancel?");
                    builder.setPositiveButton("Yes", new DialogInterface.OnClickListener() {
                        public void onClick(DialogInterface dialog, int id) {
                        	threadsCanceled = true;
                            finish(); //run onDestroy() and get rid of variables
                        }
                    });
                    builder.setNegativeButton("No", new DialogInterface.OnClickListener() {
                        public void onClick(DialogInterface dialog, int id) {
                            progressD.show();
                        }
                    });
                    AlertDialog aDialog = builder.create();
                    aDialog.show();}
            });
            progressD.show();
            new Runnable() {
                @Override
                public void run() {
                    Process.setThreadPriority(Process.THREAD_PRIORITY_BACKGROUND);
                    try {
                        ffmpeg.loadBinary(new LoadBinaryResponseHandler() {

                            @Override
                            public void onFailure() {
                            }

                            @Override
                            public void onSuccess() {
                                try {
                                    ffmpeg.execute(commands[0], handler1);
                                } catch (FFmpegCommandAlreadyRunningException e) {
                                    e.printStackTrace();
                                }
                            }

                            @Override
                            public void onFinish() {
                            }
                        });
                    } catch (FFmpegNotSupportedException e) {
                        e.printStackTrace();
                    }
                }
            }.run();
        }

    }

    @Override
    protected void onPause(){
        shouldRunffmpeg = false;
        super.onPause();
    }

    @Override
    public void onBackPressed(){
    	finish();
    }

    /*
     * Used so that this activity supports all types of extensions instead of only gifs (.gif, .mp4 ...).
      * */
    private String getFullFileNameFromArray(String fileNameStart, String[] fileNameList) {

        for (int x = 0; x < fileNameList.length; x++){
            if (fileNameList[x].startsWith(fileNameStart))
                return fileNameList[x];
        }
        return null;

    }

    private void createVideoShareIntent11(Context c, String fileName) {
        File publicDir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_MOVIES).getPath()
                + File.separator
                + "Gifaroo");
        publicDir.mkdirs();
        final File externalFile = new File(publicDir.getPath() + File.separator,
                finalFileName);

        try {
            CopyFileFromInternalToExternalDirectory(context, fileName, externalFile);
        } catch (IOException e) {
            e.printStackTrace();
            Log.d("EXTERNAL_CACHE", "Exception creating intent.");
        }
        
        MediaScannerConnection.scanFile(this,
                new String[]{externalFile.toString()},
                null,
                new MediaScannerConnection.OnScanCompletedListener() {
                    @Override
                    public void onScanCompleted(String path, Uri uri) {
                        Log.d("EXTERNALSTORAGE_SCANNER", "Scanned " + path);
                        Log.d("EXTERNALSTORAGE_SCANNER", "URI: " + uri);

                        Uri vidUri = Uri.fromFile(externalFile);

                        Intent shareIntent = new Intent();
                        shareIntent.setAction(Intent.ACTION_SEND);
                        shareIntent.putExtra(Intent.EXTRA_STREAM, vidUri);
                        shareIntent.setType("video/*");
                        startActivity(Intent.createChooser(shareIntent, "Finished! Open with..."));

                    }
                });
    }

    private boolean CopyFileFromInternalToExternalDirectory(
            Context contexts,
            String SourceFile,
            File DestinationFile) throws IOException {
        boolean success = false;
        InputStream IS = new FileInputStream(contexts.getFilesDir().getPath() + File.separator + SourceFile);
        OutputStream OS = new FileOutputStream(DestinationFile);
        success = CopyStream(IS, OS);
        OS.flush();
        OS.close();
        IS.close();
        Toast.makeText(context, "Saved", Toast.LENGTH_LONG).show();
        return success;
    }
    private boolean CopyStream(InputStream Input, OutputStream Output) throws IOException {
        byte[] buffer = new byte[5120];
        int length = Input.read(buffer);
        while (length > 0) {
            Output.write(buffer, 0, length);
            length = Input.read(buffer);
        }
        return true;
    }

    //creates a text file that ffmpeg can use to loop the final video.
    private boolean createTxtFile(Context context) {
        long duration = 1;
        int amountOfLoops = 5;

        try {
            FileInputStream fis = openFileInput(rawFileName);
            Movie movie = Movie.decodeStream(fis);
            duration = movie.duration();
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        }

        if(duration < 15000 && duration > 0)
            amountOfLoops = (int) (15000/duration);
            

        String fileString = "";
        String unloopedFileName = "final_unlooped.mp4";
        for (int i = 1; i <= amountOfLoops; i++){
            fileString += "file '"+ unloopedFileName + "'\n";
        }

        try{
            new File(context.getFilesDir(), "myList.txt").delete();
        }catch(Exception ee){
        }

        try{
            FileOutputStream fileout = openFileOutput("myList.txt", MODE_PRIVATE);
            OutputStreamWriter outputWriter = new OutputStreamWriter(fileout);
            outputWriter.write(fileString);
            outputWriter.close();

            File file = context.getFileStreamPath("myList.txt");
            if(file.exists()) {
                return true;
            } else {
            }
        } catch(Exception e){
            e.printStackTrace();
        }

        return false;
    }

    //checks if the height or the width of the gif is the biggest and returns a piece of the command.
    private String getResizeCommandDimensions(Context context) {
        BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = true;

        String dim = "";
        BitmapFactory.decodeFile(context.getFilesDir() +"/"+  rawFileName, options);
        if (options.outWidth > options.outHeight)
            dim = "700:-1";
        else
            dim = "-1:700";
        return dim;
    }

    private boolean CopyFromAssetsToStorage(Context contexts, String SourceFile, String DestinationFile) throws IOException {
        boolean success = false;
        InputStream IS = contexts.getAssets().open(SourceFile);
        OutputStream OS = new FileOutputStream(contexts.getFilesDir().getPath() + File.separator + DestinationFile);
        success = CopyStream(IS, OS);
        OS.flush();
        OS.close();
        IS.close();
        return success;
    }

/*
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_save, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }
*/
}
