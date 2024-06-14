using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Map_A_Milepost.Utils
{
    /// <summary>
    /// -   Grants INotifyPropertyChanged to each model property. Performs the same function as ViewModelBase, but with a different class name.
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
