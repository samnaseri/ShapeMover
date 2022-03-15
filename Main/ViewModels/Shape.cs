using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Main.ViewModels
{
    public class Shape : INotifyPropertyChanged
    {
        private Rect coordinates;
        private Brush? color;

        public Brush? Color
        {
            get => color; set
            {
                color = value;
                RaisePropertyChanged();
            }
        }
        public Rect Coordinates
        {
            get => coordinates; set
            {
                coordinates = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}
