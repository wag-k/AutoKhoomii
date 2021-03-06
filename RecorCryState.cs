using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoKhoomii
{
    public class RecorCryState : AbstractPlayState
    {
        public RecorCryState(MainWindow mainWindow) : base(mainWindow){}
        public override void StateChanged(){
            MainWindow.ButtonAuto.IsEnabled = false;
            MainWindow.ButtonManual.IsEnabled = false;
            MainWindow.ButtonRecord.IsEnabled = true;
            MainWindow.ButtonManual.Content = "Stop";
            MainWindow.KhoomiiPlayer.PlayLooping();
        }
        public override void PlayAuto(){
        }
        public override void PlayManual(){
        }
        public override void RecordCry(){
            MainWindow.PlayState = MainWindow.StandbyPlayState;
        }
    }
}
