using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.Storage;

namespace MetroLooper.Model
{
    class RecordingManager
    {
        Timer recTimer;
        int timerStart;
        int timerInterval;
        bool recording = false;
        bool starting = false;
        bool stop = true;
        Action<LOCK_STATE> lockUI = null;
        Action<StorageFile> addTrack = null;
        public enum LOCK_STATE { RECORDING, ALL, NONE };

        public RecordingManager(Action<LOCK_STATE>lockUI, Action<StorageFile> addTrack, int timerStart, int timerInterval)
        {
            this.lockUI = lockUI;
            this.addTrack = addTrack;
            this.timerStart = timerStart;
            this.timerInterval = timerInterval;
            recTimer = new Timer(Record_Go, new object(), 0, System.Threading.Timeout.Infinite);
        }

        public void startRecord(bool one)
        {
            recTimer.Change(timerStart, timerInterval);
            recording = true;
            starting = true;
            if (one)
            {
                stop = false;
            }
        }

        public void stopRecord()
        {
            recording = false;
            stop = true;
            starting = false;
            lockUI(LOCK_STATE.ALL);
        }

        public void Record_Go(object state)
        {
            System.Diagnostics.Debug.WriteLine("Timer ticked, recording is " + recording + ", starting is " + starting + ", and stop is " + stop + ".");
            if (recording)
            {
                if (!starting)
                {
                    //Finalize
                    //Recorder.stop()
                    //StorageFile f;
                    addTrack(null);
                    recording = false;
                    if (stop)
                    {
                        lockUI(LOCK_STATE.NONE);
                    }
                }
                if (!stop || starting)
                {
                    starting = false;
                    //Recorder.startRecording();
                    recording = true;
                    if (!stop)
                    {
                        lockUI(LOCK_STATE.RECORDING);
                    }
                }
            }
            else
            {
                lockUI(LOCK_STATE.NONE);
            }
        }
    }
}
