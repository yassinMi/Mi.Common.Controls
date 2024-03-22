using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Mi.Common.Controls
{
    /// <summary>
    /// similar to ContentPresenter, but caches and re-uses the visual trees generated for Content instances.
    /// this serves as a simplified TabControl, where ViewModel switching is managed programmatically elsewhere.
    /// </summary>
    public class PersistentContentPresenter: FrameworkElement
    {

        public int MaxCachedViews
        {
            get { return (int)GetValue(MaxCachedViewsProperty); }
            set { SetValue(MaxCachedViewsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxCachedViews.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxCachedViewsProperty =
            DependencyProperty.Register("MaxCachedViews", typeof(int), typeof(PersistentContentPresenter), new PropertyMetadata(8));

        protected override int VisualChildrenCount
        {
            get
            {
                return visuals.Count;
            }
        }
        protected override Visual GetVisualChild(int index)
        {
            return visuals.ElementAt(index);
        }

        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(PersistentContentPresenter), new PropertyMetadata(null));


        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(PersistentContentPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange, h_Content_ch));
        ContentPresenter CurrentVisibleCp;
        Queue<ContentPresenter> visuals = new Queue<ContentPresenter>();
        private static void h_Content_ch(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PersistentContentPresenter p = (PersistentContentPresenter)d;
            if (e.NewValue == null)
            {
                //# case null value, hide current visible cp is any
                if (p.CurrentVisibleCp != null) p.CurrentVisibleCp.Visibility = Visibility.Collapsed;
                return;
            }
            var existingCp = p.visuals.FirstOrDefault(v => v.Content == e.NewValue);
            if (existingCp != null)
            {
                //# case cp exists : set the cp as the visible one
                if (p.CurrentVisibleCp != null) p.CurrentVisibleCp.Visibility = Visibility.Collapsed;
                p.CurrentVisibleCp = existingCp;
                existingCp.Visibility = Visibility.Visible;
            }

            else
            {
                //# case new view - dequeue one view if necessary and create new cp and set it as the visible one
                if (p.visuals.Count >= p.MaxCachedViews)
                {
                    var toberemoved = p.visuals.Dequeue();
                    p.RemoveLogicalChild(toberemoved);
                    p.RemoveVisualChild(toberemoved);
                    if (p.CurrentVisibleCp == toberemoved)
                    {
                        toberemoved.Visibility = Visibility.Collapsed;
                        p.CurrentVisibleCp = null;
                    }
                }
                if (p.CurrentVisibleCp != null) p.CurrentVisibleCp.Visibility = Visibility.Collapsed;
                ContentPresenter cp = new ContentPresenter();
                cp.ContentTemplate = p.ContentTemplate;
                cp.Content = e.NewValue;
                p.AddVisualChild(cp);
                p.AddLogicalChild(cp);
                p.visuals.Enqueue(cp);
                p.CurrentVisibleCp = cp;
            }

            p.InvalidateVisual();
        }
         
        protected override Size MeasureOverride(Size availableSize)
        {
            Size maxChildSize = new Size();
            if (VisualChildrenCount > 0)
            {
                for (int i = 0; i < VisualChildrenCount; i++)
                {
                    UIElement u = GetVisualChild(i) as UIElement;
                    if (u != null)
                    {
                        u.Measure(availableSize);
                        maxChildSize.Height = Math.Max(maxChildSize.Height, u.DesiredSize.Height);
                        maxChildSize.Width = Math.Max(maxChildSize.Width, u.DesiredSize.Width);
                    }
                }
                
            }
            return maxChildSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (VisualChildrenCount > 0)
            {
                for (int i = 0; i < VisualChildrenCount; i++)
                {
                    UIElement u = GetVisualChild(i) as UIElement;
                    if (u != null)
                    {
                        u.Arrange(new Rect(finalSize));
                    }
                }
            }
            return finalSize;
        }

    }
}
