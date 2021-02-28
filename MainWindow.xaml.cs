using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoKhoomii
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private IPlayState playState;
        public KhoomiiPlayer KhoomiiPlayer{get;set;}
        public IPlayState PlayState{
            get{return this.playState;}
            set{
                this.playState = value;
                this.playState.StateChanged();
            }
        }
        public StandbyPlayState StandbyPlayState{get;set;}
        public AutoPlayState AutoPlayState{get;set;}
        public ManualPlayState ManualPlayState{get;set;}
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.KhoomiiPlayer = new KhoomiiPlayer();
            this.StandbyPlayState = new StandbyPlayState(this);
            this.AutoPlayState = new AutoPlayState(this);
            this.ManualPlayState = new ManualPlayState(this);
            this.PlayState = this.StandbyPlayState;

            this.KhoomiiPlayer.LoadKhoomiiMelody();
        }
        private void ButtonAuto_Click(object sender, RoutedEventArgs e)
        {
            this.PlayState.PlayAuto();
        }
        private void ButtonManual_Click(object sender, RoutedEventArgs e)
        {
            this.PlayState.PlayManual();
        }

    }
}
