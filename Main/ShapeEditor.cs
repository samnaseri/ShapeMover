using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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

namespace Main
{
    public class ShapeEditor : Canvas
    {
        private List<UIElement> shapes = new List<UIElement>();
        Point? startPoint = null;
        UIElement draggingElement = null;
        Point? initialLocation = null;
        Random random = new Random();
        Dictionary<object,UIElement> itemToUIElementMap = new Dictionary<object,UIElement>();
        Dictionary<UIElement, object> uiElementToItemMap = new Dictionary<UIElement, object>();
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ShapeEditor), new PropertyMetadata(null));
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ShapeEditor), new PropertyMetadata(OnItemsSourceChanged));
        private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var me = sender as ShapeEditor;
            me?.BindItemSource();
        }
        private void BindItemSource()
        {
            foreach (var item in ItemsSource)
            {
                AddItem(item);
            }

            if (ItemsSource is INotifyCollectionChanged)
            {
                var itemsSource = (INotifyCollectionChanged)ItemsSource;
                itemsSource.CollectionChanged += ItemsSource_CollectionChanged;
            }
        }
        private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            e.OldItems?.Cast<object>().ToList().ForEach(RemoveItem);
            e.NewItems?.Cast<object>().ToList().ForEach(AddItem);           
        }
        static ShapeEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ShapeEditor), new FrameworkPropertyMetadata(typeof(ShapeEditor)));
        }
        public ShapeEditor()
        {
            this.Focus();
        }
        private void AddItem(object item)
        {
            var itemContent = ItemTemplate.LoadContent() as FrameworkElement;
            itemContent.DataContext = item;

            AddElement(itemContent);
            itemToUIElementMap.Add(item, itemContent);
            uiElementToItemMap.Add(itemContent, item);

            var changingItem = item as INotifyPropertyChanged;
            changingItem.PropertyChanged += Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        { 
            if(e.PropertyName == "Coordinates")
            {
                // for some reasons I couldn't find out the XAML Binding for Canvas.Left and Canvas.Top didn't work
                // so trying to wrap up the assignment I just did this dirty hack to manaully set the location of 
                // element. removing this code here would prevent Undo/Redo logic to work
                var element = itemToUIElementMap[sender];
                dynamic item = (dynamic)sender;
                Canvas.SetLeft(element, item.Coordinates.Left);
                Canvas.SetTop(element, item.Coordinates.Top);
            }
        }

        private void RemoveItem(object item)
        {
            var itemContent = itemToUIElementMap[item];
            RemoveElement(itemContent);
            itemToUIElementMap.Remove(item);
            uiElementToItemMap.Remove(itemContent);


            var changingItem = item as INotifyPropertyChanged;
            changingItem.PropertyChanged += Item_PropertyChanged;
        }
        private void AddElement(UIElement element)
        {           
            this.Children.Add(element);
            element.MouseDown += Circle_MouseDown;
        }
        private void RemoveElement(UIElement element)
        {
            element.MouseDown -= Circle_MouseDown;
            this.Children.Remove(element);
        }
        private void Circle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var circle = sender as UIElement;
            startPoint = e.GetPosition(this);
            initialLocation = new Point(Canvas.GetLeft(circle), Canvas.GetTop(circle));
            draggingElement = circle;
            circle.CaptureMouse();            
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (startPoint != null)
            {
                var currentPosition = e.GetPosition(this);
                var offset = currentPosition - startPoint;
                Canvas.SetLeft(draggingElement, Math.Clamp(initialLocation.Value.X + offset.Value.X, 0, this.RenderSize.Width - draggingElement.RenderSize.Width));
                Canvas.SetTop(draggingElement, Math.Clamp(initialLocation.Value.Y + offset.Value.Y,0, this.RenderSize.Height - draggingElement.RenderSize.Height));
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (startPoint != null)
            {
                var oldLocation = startPoint;
                var currentPosition = e.GetPosition(this);
                startPoint = null;
                draggingElement.ReleaseMouseCapture();
                RaiseEvent(new ItemMovedEventArgs(OnItemMovedEvent) { Item = uiElementToItemMap[draggingElement], OldLocation = oldLocation.Value, NewLocation = currentPosition });
            }
        }

        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent OnItemMovedEvent = EventManager.RegisterRoutedEvent(
            name: "OnItemMoved",
            routingStrategy: RoutingStrategy.Direct,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(ShapeEditor));

        // Provide CLR accessors for assigning an event handler.
        public event RoutedEventHandler OnItemMoved
        {
            add { AddHandler(OnItemMovedEvent, value); }
            remove { RemoveHandler(OnItemMovedEvent, value); }
        }
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape && startPoint != null)
            {
                startPoint = null;
                Canvas.SetLeft(draggingElement, initialLocation.Value.X);
                Canvas.SetTop(draggingElement, initialLocation.Value.Y);
                draggingElement.ReleaseMouseCapture();
            }
        }
    }

    public class ItemMovedEventArgs : RoutedEventArgs
    {
        public ItemMovedEventArgs() : base()
        {

        }

        public ItemMovedEventArgs(RoutedEvent routedEvent) : base(routedEvent)
        {

        }

        public object Item { get; set; }
        public Point OldLocation { get; set; }
        public Point NewLocation { get; set; }
    }
}
