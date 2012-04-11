using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace B1.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for NumericPlusMinus.xaml
    /// Provides a control to maintain a numeric value with functionality 
    /// to increase (plus) or decrease (minus) the value.
    /// </summary>
    public partial class NumericPlusMinus : UserControl, INumericPlusMinus
    {
        public delegate void ClickHandler(object context, int newValue);
        public enum PlusOrMinus { Plus, Minus };
        static int _instance = 0;
        ClickHandler _clickHdlr = null;
        int _min = int.MinValue;
        int _max = int.MaxValue;
        int _plus = 1;
        int _minus = 1;
        int? _value = 0;
        object _parentContext = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public NumericPlusMinus()
        {
            InitializeComponent();
            gbUpDown.Header = string.Format("numericPlusMinus{0}", ++_instance);
        }

        /// <summary>
        /// Sets the initial values and configuration for the control
        /// </summary>
        /// <param name="label">Text label for the control</param>
        /// <param name="parentContext">Object to be returned on click events</param>
        /// <param name="clickHdlr">Delegate handler for the click events</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="plus">Increment by value</param>
        /// <param name="minus">Decrement by value</param>
        /// <param name="value">Starting value</param>
        public void Initialize(string label = null
                , object parentContext = null
                , ClickHandler clickHdlr = null
                , int min = int.MinValue
                , int max = int.MaxValue
                , int plus = 1
                , int minus = 1
                , int value = 0)
        {
            gbUpDown.Header = label;
            _parentContext = parentContext;
            _clickHdlr = clickHdlr;
            _min = min;
            _max = max;
            _plus = plus;
            _minus = minus;
            _value = value;
            tbNumber.Text = _value.Value.ToString();
        }

        /// <summary>
        /// Get/Set Label
        /// </summary>
        public string Label { get { return gbUpDown.Uid; } set { gbUpDown.Uid = value; } }

        /// <summary>
        /// Get/Set Minimum Value
        /// </summary>
        public int Min { get { return _min; } set { _min = value; } }
        
        /// <summary>
        /// Get/Set Maximum Value
        /// </summary>
        public int Max { get { return _max; } set { _max = value; } }

        /// <summary>
        /// Get/Set Increment Value
        /// </summary>
        public int Plus { get { return _plus; } set { _plus = value; } }

        /// <summary>
        /// Get/Set Decrement Value
        /// </summary>
        public int Minus { get { return _minus; } set { _minus = value; } }

        /// <summary>
        /// Get/Set Current Value
        /// </summary>
        public int? Value { get { return _value; } set { _value = value; tbNumber.Text = _value.ToString();  } }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (_value.HasValue)
            {
                HandleClick(sender, e, PlusOrMinus.Minus);
            }
            tbNumber.Text = _value.ToString();
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            if (_value.HasValue)
            {
                HandleClick(sender, e, PlusOrMinus.Plus);
            }
            tbNumber.Text = _value.ToString();
        }

        private void HandleClick(object sender, RoutedEventArgs e, PlusOrMinus plusOrMinus)
        {
            bool changed = false;;
            int _delta = 0;
            // if current value is not null
            if (_value.HasValue)
            {
                if (plusOrMinus == PlusOrMinus.Minus)
                {
                    // if it was a decrement, make sure it is not below min
                    if ((_value.Value - _minus) < _min)
                    {
                        // if so, return the min
                        _delta = _value.Value - _min;
                        _value = _min;
                    }
                    else
                    {
                        // otherwise decrement by minus factor
                        _value -= _minus;
                        changed = true;
                        _delta = _minus;
                    }
                    _delta *= -1; // we will be adding to the value, so minus is just an add of a negative
                }
                else
                {
                    // it was an increment, make sure it is not greater than max
                    if ((_value.Value + _plus) > _max)
                    {
                        // if so, return the max
                        _delta = _max - _value.Value;
                        _value = _max;
                    }
                    else
                    {
                        // otherwise increment by plus factor
                        _value += _plus;
                        changed = true;
                        _delta = _plus;
                    }
                }
            }
            tbNumber.Text = _value.ToString();
            if (_clickHdlr != null && changed && _value.HasValue)
                _clickHdlr(_parentContext, _delta);
        }
    }
}
