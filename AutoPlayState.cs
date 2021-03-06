using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoKhoomii
{
    public class AutoPlayState : AbstractPlayState
    {
        public AutoPlayState(MainWindow mainWindow) : base(mainWindow){}
        public override void StateChanged(){
            MainWindow.BabyCryDetector.DetectCry(MainWindow.KhoomiiPlayer.KhoomiiMelody);
        }
        public override void PlayAuto(){
            MainWindow.PlayState = MainWindow.StandbyPlayState;
        }
        public override void PlayManual(){

        }
        public override void RecordCry(){

        }
    }
}
