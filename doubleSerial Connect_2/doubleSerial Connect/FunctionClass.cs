using System;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;

namespace doubleSerial_Connect
{
    class FunctionClass
    {
        public static void GetSerialPort(ComboBox combobox)
        {
            try
            {
                combobox.Items.Clear();
                string[] portname = SerialPort.GetPortNames();
                foreach (var port in portname)
                {
                    combobox.Items.Add(port);
                }
                if (combobox.Items.Count <= 0) MessageBox.Show("연결 장치가 없습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public static void VisonChange(GroupBox groupBox, bool v)
        {
            groupBox.Visibility = (v) ? groupBox.Visibility = Visibility.Visible : groupBox.Visibility = Visibility.Hidden;
        }
        public static void Check_CheckBox(CheckBox checkbox)
        {
            if (checkbox.IsChecked == true) checkbox.IsChecked = false;
        }
        public static void calculateTime(int time_int,Label label)
        {
            decimal long_min = time_int / 60;
            decimal min = Math.Truncate(long_min);
            decimal sec = time_int - min * 60;
            label.Content = min.ToString() + "분" + sec.ToString() + "초";
        }
        public static void SliderValuecon(Slider slider, bool v)
        {
            slider.Value = (v) ? slider.Maximum : slider.Minimum;
        }
        public static void sliderValuech(Slider time_Value_slider, TextBox time_value_num)
        {
            time_Value_slider.SelectionEnd = time_Value_slider.Value;
            time_value_num.Text = Convert.ToString(Math.Truncate(time_Value_slider.Value));
        }
        public static void sliderValuech1(Slider time_Value_slider, TextBox time_value_num)
        {
            time_Value_slider.SelectionEnd = time_Value_slider.Value;
            time_value_num.Text = Convert.ToString((Math.Truncate(time_Value_slider.Value)+49)*10);
        }
    }
}