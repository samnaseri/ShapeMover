using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Main.ViewModels
{
    public class OperationHistory
    {
        private readonly Action onStateChanged;

        public OperationHistory(Action onStateChanged)
        {
            this.onStateChanged = onStateChanged;
        }

        public ObservableCollection<Operation> DoneList { get; set; } = new ObservableCollection<Operation>();
        public ObservableCollection<Operation> UndoneList { get; set; } = new ObservableCollection<Operation>();

        public void AddToHistory(string action, Action undoAction, Action redoAction)
        {
            DoneList.Add(new Operation { Name = action, Undo = undoAction, Redo = redoAction });
            UndoneList.Clear();
            onStateChanged?.Invoke();
        }

        public void Undo()
        {
            if (DoneList.Any())
            {
                var operation = DoneList.Last();
                DoneList.Remove(operation);
                operation.Undo();

                UndoneList.Add(operation);
                onStateChanged?.Invoke();
            }
        }

        public void Redo()
        {
            if (UndoneList.Any())
            {
                var operation = UndoneList.Last();
                UndoneList.Remove(operation);
                operation.Redo();

                DoneList.Add(operation);
                onStateChanged?.Invoke();
            }
        }
    }
}
