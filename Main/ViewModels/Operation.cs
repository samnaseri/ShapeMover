using System;

namespace Main.ViewModels
{
    public class Operation
    {
        public string Name { get; set; }
        public Action Undo { get; set; }
        public Action Redo { get; set; }
    }
}
