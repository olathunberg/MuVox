using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TTech.Muvox.UI_Features.LoadingPanel
{
    /// <summary>
    /// Interaction logic for LoadingPanel.xaml
    /// </summary>
    public partial class LoadingPanel : UserControl
    {
        public LoadingPanel()
        {
            InitializeComponent();
        }

        public int ShowDelay { get; set; }

        public bool DeferredShow
        {
            get { return (bool)GetValue(DeferredShowProperty); }
            set { SetValue(DeferredShowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DeferredShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeferredShowProperty =
            DependencyProperty.Register("DeferredShow", typeof(bool), typeof(LoadingPanel), new PropertyMetadata(false, async (s, e) =>
            {
                var view = (s as LoadingPanel);
                if (view == null) return;

                if ((bool)e.NewValue)
                {
                    await Task.Delay(view.ShowDelay);

                    if (view.Resources["FadeIn"] is Storyboard storyboard)
                        storyboard.Begin(view.border);
                }
                else
                {
                    if (view.Resources["FadeOut"] is Storyboard storyboard)
                        storyboard.Begin(view.border);
                }
            }));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(LoadingPanel), new PropertyMetadata("Loading..."));
    }
}
