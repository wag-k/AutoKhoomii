using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoKhoomii
{
    public class RecordCryState : AbstractPlayState
    {
        public RecordCryState(MainWindow mainWindow) : base(mainWindow){}
        public override void StateChanged(){
            MainWindow.ButtonAuto.IsEnabled = false;
            MainWindow.ButtonManual.IsEnabled = false;
            MainWindow.ButtonRecordAmbient.IsEnabled = false;
            MainWindow.ButtonRecord.IsEnabled = true;
            MainWindow.ButtonRecord.Content = "Stop";
            MainWindow.BabyCryDetector.StartSamplingCry();
        }
        public override void PlayAuto(){
        }
        public override void PlayManual(){
        }
        public override void RecordAmbient(){
        }
        public override void RecordCry(){
            MainWindow.BabyCryDetector.StopSamplingCry();
            MainWindow.PlayState = MainWindow.StandbyPlayState;

        }
    }
}
