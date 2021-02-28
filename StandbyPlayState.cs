﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoKhoomii
{
    public class StandbyPlayState: AbstractPlayState
    {
        public StandbyPlayState(MainWindow mainWindow) : base(mainWindow){}
        public override void StateChanged(){
            MainWindow.ButtonAuto.IsEnabled = true;
            MainWindow.ButtonAuto.Content = "Auto";
            MainWindow.ButtonManual.IsEnabled = true;
            MainWindow.ButtonManual.Content = "Manual";
        }
        public override void PlayAuto(){
            MessageBox.Show("Under construction", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            
        }
        public override void PlayManual(){
            MainWindow.PlayState = MainWindow.ManualPlayState;
        }
    }
}
