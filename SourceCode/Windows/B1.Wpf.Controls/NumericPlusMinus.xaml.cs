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

        public NumericPlusMinus()
        {
            InitializeComponent();
            gbUpDown.Header = string.Format("numericPlusMinus{0}", ++_instance);
        }

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

        public string Label { get { return gbUpDown.Uid; } set { gbUpDown.Uid = value; } }
        public int Min { get { return _min; } set { _min = value; } }
        public int Max { get { return _max; } set { _max = value; } }
        public int Plus { get { return _plus; } set { _plus = value; } }
        public int Minus { get { return _minus; } set { _minus = value; } }
        public int? Value { get { return _value; } set { _value = value; tbNumber.Text = _value.ToString();  } }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (_value.HasValue)
            {
                HandleClick(sender, e, PlusOrMinus.Minus);
            }
            tbNumber.Text = _value.ToString();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
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
            if (_value.HasValue)
            {
                if (plusOrMinus == PlusOrMinus.Minus)
                {
                    if ((_value.Value - _minus) < _min)
                    {
                        _delta = _value.Value - _min;
                        _value = _min;
                    }
                    else
                    {
                        _value -= _minus;
                        changed = true;
                        _delta = _minus;
                    }
                    _delta *= -1;
                }
                else
                {
                    if ((_value.Value + _plus) > _max)
                    {
                        _delta = _max - _value.Value;
                        _value = _max;
                    }
                    else
                    {
                        _value += _plus;
                        changed = true;
                        _delta = _plus;
                    }
                    _delta *= -1;
                }
            }
            tbNumber.Text = _value.ToString();
            if (_clickHdlr != null && changed && _value.HasValue)
                _clickHdlr(_parentContext, _delta);
        }
    }
}
