using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AutoKhoomii
{
    public class AutoPlayState : AbstractPlayState
    {
        private Timer TimerDetecting{get; set;}
        public AutoPlayState(MainWindow mainWindow) : base(mainWindow){
            this.TimerDetecting = CreateTimer();
        }
        public override void StateChanged(){
            MainWindow.ButtonAuto.IsEnabled = true;
            MainWindow.ButtonManual.IsEnabled = false;
            MainWindow.ButtonRecord.IsEnabled = false;
            MainWindow.ButtonAuto.Content = "Stop";
            MainWindow.BabyCryDetector.StartDetectingCry();
            this.TimerDetecting.Enabled = true;
        }
        public override void PlayAuto(){
            MainWindow.BabyCryDetector.StopDetectingCry();
            MainWindow.PlayState = MainWindow.StandbyPlayState;
        }
        public override void PlayManual(){

        }
        public override void RecordCry(){

        }
        public Timer CreateTimer(){
            Timer timer = new Timer();
            timer.Elapsed += DetectCry;
            timer.Interval = 0.5;
            timer.Enabled = false;
            return timer;
        }
        public void DetectCry(object sender, System.Timers.ElapsedEventArgs e){
            this.TimerDetecting.Enabled = false; // やってる間は止めます
            if(MainWindow.BabyCryDetector.DetectCry()){
                this.TimerDetecting.Enabled = false;
                MainWindow.KhoomiiPlayer.Play();
            }
            this.TimerDetecting.Enabled = true;
        }
    }
}
