using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoKhoomii
{
    public class ManualPlayState : AbstractPlayState
    {
        public ManualPlayState(MainWindow mainWindow) : base(mainWindow){}
        public override void StateChanged(){
            MainWindow.ButtonAuto.IsEnabled = false;
            MainWindow.ButtonRecordAmbient.IsEnabled = false;
            MainWindow.ButtonManual.IsEnabled = true;
            MainWindow.ButtonManual.Content = "Stop";
            MainWindow.KhoomiiPlayer.PlayLooping();
        }
        public override void PlayAuto(){
        }
        public override void PlayManual(){
            MainWindow.KhoomiiPlayer.Stop();
            MainWindow.PlayState = MainWindow.StandbyPlayState;
        }
        public override void RecordAmbient(){
        }
        public override void RecordCry(){
        }
    }

}
