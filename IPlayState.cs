using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoKhoomii
{
    public interface IPlayState
    {
        void StateChanged();
        void PlayAuto();
        void PlayManual();
        void RecordCry();
    }

    public abstract class AbstractPlayState : IPlayState{
        protected static MainWindow MainWindow{get;set;}
        public AbstractPlayState(MainWindow mainWindow){
            AbstractPlayState.MainWindow = mainWindow;
        }
        
        public abstract void StateChanged();
        public abstract void PlayAuto();
        public abstract void PlayManual();
        public abstract void RecordCry();
    }
}
