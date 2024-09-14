using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace CircularProgress
{
    /// <summary>
    /// Interaction logic for RadialProgressBar.xaml
    /// </summary>
    public partial class RadialProgressBar : UserControl
    {

        /// <summary>
        /// Define our BackgroundBrushProperty
        /// </summary>
        public static readonly DependencyProperty OuterBrushProperty = DependencyProperty.Register("OuterBrush", typeof(Brush), typeof(RadialProgressBar));
        public Brush OuterBrush
        {
            get { return (Brush)this.GetValue(OuterBrushProperty); }
            set { this.SetValue(OuterBrushProperty, value); }
        }

        /// <summary>
        /// Define our InnerBrushProperty
        /// </summary>
        public static readonly DependencyProperty InnerBrushProperty = DependencyProperty.Register("InnerBrush", typeof(Brush), typeof(RadialProgressBar));
        public Brush InnerBrush
        {
            get { return (Brush)this.GetValue(InnerBrushProperty); }
            set { this.SetValue(InnerBrushProperty, value); }
        }

        /// <summary>
        /// Define our RingBrushProperty
        /// </summary>
        public static readonly DependencyProperty RingBrushProperty = DependencyProperty.Register("RingBrush", typeof(Brush), typeof(RadialProgressBar));
        public Brush RingBrush
        {
            get { return (Brush)this.GetValue(RingBrushProperty); }
            set { this.SetValue(RingBrushProperty, value); }
        }

        /// <summary>
        /// Define our percentage ValueProperty
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(RadialProgressBar));
        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Define our ring ThicknessProperty
        /// </summary>
        public static readonly DependencyProperty RingThicknessProperty = DependencyProperty.Register("RingThickness", typeof(int), typeof(RadialProgressBar));
        public int RingThickness
        {
            get { return (int)this.GetValue(RingThicknessProperty); }
            set { this.SetValue(RingThicknessProperty, value); }
        }

        public RadialProgressBar()
        {
            InitializeComponent();
        }
    }

    [ValueConversion(typeof(int), typeof(double))]
    public class ValueToAngle : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convert the 0 to 360 degrees of the circle into 0% to 100%.
            return (double)((int)value * 0.01 * 360);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convert the 0% to 100% into 0 to 360 degrees of the circle. 
            return (int)((double)value / 360 * 100);
        }
    }
}
