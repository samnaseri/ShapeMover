using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Main.ViewModels
{
    internal class MainWindowViewModel
    {
        OperationHistory history;
        public static Size DefaultSize = new Size(30, 30);
        Random random;

        public MainWindowViewModel(int seed)
        {
            history = new OperationHistory(RefreshHistoryCommands);
            random = new Random(seed);
            Shapes = new ObservableCollection<Shape>();
        }

        public ObservableCollection<Shape> Shapes { get; set; }

        public ICommand AddNewShapeCommand => new RelayCommand(AddNewShape, ()=>true);
        public RelayCommand UndoCommand => new RelayCommand(history.Undo, ()=>history.DoneList.Any());
        public RelayCommand RedoCommand => new RelayCommand(history.Redo, ()=>history.UndoneList.Any());

        public void RefreshHistoryCommands()
        {
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }
        public void AddNewShape()
        {
            var coordinates = new Rect(new Point(random.Next(100), random.Next(100)), DefaultSize);
            var shape = new Shape() { Coordinates = coordinates , Color = Brushes.AliceBlue };
            var createShape = ()=> Shapes.Add(shape);
            Action deleteShape = () => Shapes.Remove(shape);
            createShape();
            history.AddToHistory($"created new shape at {coordinates}", deleteShape, createShape);
            RefreshHistoryCommands();
        }

        public void ShapeMoved(object sender, ItemMovedEventArgs args)
        {
            history.AddToHistory("circle moved", 
                undoAction: () => {
                    var shape = (Shape)args.Item;
                    shape.Coordinates = new Rect(args.OldLocation, shape.Coordinates.Size);
                }, 
                redoAction: () => {
                    var shape = (Shape)args.Item;
                    shape.Coordinates = new Rect(args.NewLocation, shape.Coordinates.Size);
                });
            RefreshHistoryCommands();
        }
    }

    
}
