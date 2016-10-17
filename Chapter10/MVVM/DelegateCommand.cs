using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MVVM
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> cmd;

        public DelegateCommand(Action<object> cmd)
        {
            this.cmd = cmd;
        }

        public void Execute(object parameter)
        {
            cmd(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return canExecute;
        }

        private bool canExecute = true;
        public void SetCanExecute(bool canExecute)
        {
            this.canExecute = canExecute;
            CanExecuteChanged(this, EventArgs.Empty);
        }


        public event EventHandler CanExecuteChanged = delegate { };
    }
}
