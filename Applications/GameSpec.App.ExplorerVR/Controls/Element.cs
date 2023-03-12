using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameSpec.App.ExplorerVR.Controls
{
    public abstract class Element : BindableObject
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public abstract void Step();
    }
}
