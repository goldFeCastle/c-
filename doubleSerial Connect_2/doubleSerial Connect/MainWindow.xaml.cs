using System;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;
using System.Windows.Interop;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace doubleSerial_Connect
{
    public partial class MainWindow : Window
    {
        DispatcherTimer Shot_Timer = new DispatcherTimer();
        DispatcherTimer Operation_Timer = new DispatcherTimer();
        SerialPort TrigerSerial = new SerialPort();
        SerialPort PulseSerial = new SerialPort();
        private int Reapeat_Checker;
        private int time_int;
        string Enabled = "UnReady";
        delegate void SetTextCallBack(string text);
        SolidColorBrush mySolidColorBrush = new SolidColorBrush();
        List<string> setcondision = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(initCom);
            Operation_Timer.Interval = TimeSpan.FromSeconds(1);
            Operation_Timer.Start();
            Operation_Timer.Tick += Operation_Timer_Tick;
            Freq_Value_Slider.Value = 1;
            Freq_Value_Slider.Minimum = 1;
        }
        private void initCom(object sender, RoutedEventArgs e)
        {
            TrigerSerial.DataReceived += new SerialDataReceivedEventHandler(serialDatarecived);
            PulseSerial.DataReceived += new SerialDataReceivedEventHandler(serialDatarecived);
            string[] ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                port_combox.Items.Add(port);
                port_combox1.Items.Add(port);
            }
        }

        private void serialDatarecived(object sender, SerialDataReceivedEventArgs e)
        {
                GetSerialDatafunction(TrigerSerial);
                GetSerialDatafunction(PulseSerial);
        }

        private void GetSerialDatafunction(SerialPort Serial)
        {
            try
            {
                string getSerialData_t = TrigerSerial.ReadExisting();
                string getSerialData_p = PulseSerial.ReadExisting();
                if (getSerialData_t != string.Empty || getSerialData_p != string.Empty)
                {
                    if (getSerialData_t == "Check the ready")
                    {
                        Enabled = "Ready";
                        MessageBox.Show("Not enabled");

                    }
                    else
                    {
                        Enabled = "UnReady";
                        SetText(getSerialData_t);
                        SetText(getSerialData_p);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SetText(string getSerialData)
        {
            Connect_Button.IsEnabled = false;
            if (RecivedData_TextBox.Dispatcher.CheckAccess())
            {
                RecivedData_TextBox.AppendText(getSerialData);
            }
            else
            {
                SetTextCallBack d = new SetTextCallBack(SetText);
                RecivedData_TextBox.Dispatcher.Invoke(d, new object[] { getSerialData });
            }
            Connect_Button.IsEnabled = true;
        }

        private void Operation_Timer_Tick(object sender, EventArgs e)
        {
            time_int++;
            FunctionClass.calculateTime(time_int,label3);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource sorce = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            sorce.AddHook(new HwndSourceHook(Wndproc));
        }

        private IntPtr Wndproc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            uint WM_DEVICECHANGE = 0x0219;
            uint DBT_DEVICEARRIVAL = 0x8000;
            uint DBT_DEVICECOMPLETE = 0x8004;
            uint DBT_DEVTYP_PORT = 0x00000003;
            if ((msg == WM_DEVICECHANGE) && (wParam.ToInt32() != DBT_DEVICECOMPLETE))
            {
                if (wParam.ToInt32() == DBT_DEVICEARRIVAL)
                {
                    int devtype = System.Runtime.InteropServices.Marshal.ReadInt32(lParam, 4);
                    if (devtype == DBT_DEVTYP_PORT)
                    {
                        FunctionClass.GetSerialPort(port_combox);
                        FunctionClass.GetSerialPort(port_combox1);
                    }
                }
            }
            return IntPtr.Zero;
        }

        private void port_combox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TrigerSerial.IsOpen) TrigerSerial.Close();
                TrigerSerial.PortName = port_combox.SelectedItem.ToString();

        }//Combox 설정

        private void port_combox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PulseSerial.IsOpen) PulseSerial.Close();
                PulseSerial.PortName = port_combox1.SelectedItem.ToString();
        }

        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               if (Connect_Button.Content.ToString() == "Connect") COMBox_control(true);
               else COMBox_control(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void COMBox_control(bool v)
        {
            if (v)
            {
                TrigerSerial.Open();
                TrigerSerial.DataReceived += new SerialDataReceivedEventHandler(serialDatarecived);
                PulseSerial.Open();
                PulseSerial.DataReceived += new SerialDataReceivedEventHandler(serialDatarecived);
                Connect_Button.Content = "Disconnect";
                TrigerCOM_groupbox.IsEnabled = false;
                PulseCOM_groupbox.IsEnabled = false;
                Control_Grid.Visibility = Visibility.Visible;
            }
            else
            {

                Connect_Button.Content = "Connect";
                TrigerCOM_groupbox.IsEnabled = true;
                PulseCOM_groupbox.IsEnabled = true;
                Control_Grid.Visibility = Visibility.Hidden;
                TrigerSerial.DataReceived -= new SerialDataReceivedEventHandler(serialDatarecived);
                TrigerSerial.Close();
                PulseSerial.DataReceived -= new SerialDataReceivedEventHandler(serialDatarecived);
                PulseSerial.Close();
            }
        }
        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Shot_Timer.IsEnabled)
                {
                    Shot_Timer.Stop();
                }
                else
                {
                    PulseSerial.Write("5");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Triger_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FunctionClass.Check_CheckBox(Shot_CheckBox);
            FunctionClass.VisonChange(Set_Sellect_GroupBox, true);
            BurstCondition_GroupBox.IsEnabled = true;
        }
        private void Triger_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FunctionClass.VisonChange(Set_Sellect_GroupBox, false);
        }

        private void Shot_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FunctionClass.Check_CheckBox(Triger_CheckBox);
            if ((Times_Value_Slider.Value == 0) || (Duty_Value_Slider.Value == 0)||(Freq_Value_Slider.Value == 0))
            {
                if(setcondision.Count !=0)
                {
                    MessageBox.Show("이전 셋팅 값으로 셋팅 합니다.", "이전값 사용?");
                    Times_Value_Textbox.Text = setcondision[0];
                    Duty_Value_Textbox.Text = setcondision[1];
                    Freq_Value_Textbox.Text = setcondision[2];
                    Done_Button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    BurstCondition_GroupBox.Visibility = Visibility.Visible;

                }
               
            }
            else
            {
                FunctionClass.VisonChange(Shot_GroupBox, true);
            }
        }
        private void Shot_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FunctionClass.VisonChange(Shot_GroupBox, false);
            FunctionClass.Check_CheckBox(Costom_CheckBox);
            FunctionClass.Check_CheckBox(Preset_CheckBox);
            Enable_Button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void Costom_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FunctionClass.Check_CheckBox(Preset_CheckBox);
            FunctionClass.VisonChange(BurstCondition_GroupBox, true);
        }
        private void Costom_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FunctionClass.VisonChange(BurstCondition_GroupBox, false);
        }

        private void Preset_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FunctionClass.Check_CheckBox(Costom_CheckBox);
            FunctionClass.VisonChange(Preset_GroupBox, true);
        }
        private void Preset_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FunctionClass.VisonChange(Preset_GroupBox, false);
        }

        private void Quter_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Duty_Value_Slider.Value = 25;
            Preset_Check(100, Quter_CheckBox);
        }
        private void Half_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Duty_Value_Slider.Value = 50;
            Preset_Check(100, Half_CheckBox);
        }
        private void Whole_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Duty_Value_Slider.Value = 100;
            Preset_Check(100, Whole_CheckBox);
        }
        private void Preset_Check(int v, CheckBox CheckBox)
        {
            Times_Value_Slider.Value = 5;
            Freq_Value_Slider.Value = 200;
            FunctionClass.VisonChange(BurstCondition_GroupBox, true);
            FunctionClass.Check_CheckBox(CheckBox);
            Done_Button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void Times_Min_Button_Click(object sender, RoutedEventArgs e)
        {
            FunctionClass.SliderValuecon(Times_Value_Slider, false);
        }
        private void Times_Max_Button_Click(object sender, RoutedEventArgs e)
        {
            FunctionClass.SliderValuecon(Times_Value_Slider, true);
        }
        private void Times_Value_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FunctionClass.sliderValuech(Times_Value_Slider, Times_Value_Textbox);
        }
        private void Times_miner_Button_Click(object sender, RoutedEventArgs e)
        {
            Times_Value_Slider.Value--;
        }
        private void Times_plus_Button_Click(object sender, RoutedEventArgs e)
        {
            Times_Value_Slider.Value++;
        }

        private void Duty_Min_Button_Click(object sender, RoutedEventArgs e)
        {
            FunctionClass.SliderValuecon(Duty_Value_Slider, false);
        }
        private void Duty_Value_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FunctionClass.sliderValuech(Duty_Value_Slider, Duty_Value_Textbox);
        }
        private void Duty_Max_Button_Click(object sender, RoutedEventArgs e)
        {
            FunctionClass.SliderValuecon(Duty_Value_Slider, true);
        }
        private void Duty_miner_Button_Click(object sender, RoutedEventArgs e)
        {
            Duty_Value_Slider.Value--;
        }
        private void Duty_plus_Button_Click(object sender, RoutedEventArgs e)
        {
            Duty_Value_Slider.Value++;
        }

        private void Freq_Min_Button_Click(object sender, RoutedEventArgs e)
        {
            FunctionClass.SliderValuecon(Freq_Value_Slider, false);
        }
        private void Freq_Value_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FunctionClass.sliderValuech1(Freq_Value_Slider, Freq_Value_Textbox);
        }
        private void Freq_Max_Button_Click(object sender, RoutedEventArgs e)
        {
            FunctionClass.SliderValuecon(Freq_Value_Slider, true);
        }
        private void Freq_miner_Button_Click(object sender, RoutedEventArgs e)
        {
            Freq_Value_Slider.Value--;
        }
        private void Freq_plus_Button_Click(object sender, RoutedEventArgs e)
        {
            Freq_Value_Slider.Value++;
        }

        private void Done_Button_Click(object sender, RoutedEventArgs e)
        {
            int slidervalve = Convert.ToInt32(Duty_Value_Textbox.Text);
            Duty_Value_Slider.Value = slidervalve;
            int slidervalve1 = Convert.ToInt32(Times_Value_Textbox.Text);
            Times_Value_Slider.Value = slidervalve1;
            int slidervalve2 = ((Convert.ToInt16(Freq_Value_Textbox.Text)/10)-49);
            Freq_Value_Slider.Value = slidervalve2;
            if ((Times_Value_Slider.Value == 0) || (Times_Value_Slider.Value == 0)||(Freq_Value_Slider.Value == 0))
            {
                MessageBox.Show("듀티, 또는 타임값을 입력 하십시요.");
            }
            else
            {
                ShowMessegeBox(true);
            }
        }
        private void serial_TX()
        {
            if ((TrigerSerial.IsOpen)&& (PulseSerial.IsOpen))
            {
                TrigerSerial.Write("1");
                Task.Delay(50);
                TrigerSerial.Write("1");
                Task.Delay(50);
                TrigerSerial.Write(plus_zero(Times_Value_Textbox.Text));
                Task.Delay(50);
                TrigerSerial.Write(plus_zero(Duty_Value_Textbox.Text));
                Enable_Button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                System.Threading.Thread.Sleep(100);
                PulseSerial.Write("1");
                System.Threading.Thread.Sleep(50);
                PulseSerial.Write(plus_zero_p(Freq_Value_Textbox.Text));
                System.Threading.Thread.Sleep(100);
                PulseSerial.Write("0050");
                System.Threading.Thread.Sleep(100);
                PulseSerial.Write("2");
                System.Threading.Thread.Sleep(100);



            }
            else
            {
                MessageBox.Show("포트 연결을 재확인 하십시요");
                ShowMessegeBox(false);
            }
        }

        private string plus_zero_p(string text)
        {
            decimal d = Convert.ToDecimal(text);
            if(d<=999)
            {
                text = "0"+text;
            }
            return text;
        }

        private void ShowMessegeBox(bool v)
        {
            if (v)
            {
                if (MessageBox.Show("입력 하신 값은\r" + "트리거 \r" + "Times : " + Times_Value_Textbox.Text + "회\r"+ "Duty ratio :  "+Duty_Value_Textbox.Text + "%\r" + "펄스 \r"
                    +"freq : "+ Freq_Value_Textbox.Text + "kHz \r 입니다. 입력한 값과 일치합니까?", "입력값 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    BurstCondition_GroupBox.IsEnabled = false;
                    Triger_CheckBox.IsChecked = false;
                    Shot_CheckBox.IsChecked = true;
                    FunctionClass.VisonChange(Preset_GroupBox, false);
                    FunctionClass.VisonChange(Shot_GroupBox, true);
                    serial_TX();
                }
            }
            else
            {
                BurstCondition_GroupBox.IsEnabled = true;
                Triger_CheckBox.IsChecked = true;
            }
        }
        private string plus_zero(string text)
        {
            int k = Convert.ToInt32(text);
            if (k < 10)
            {
                text = "0" + text;
            }
            return text;
        }

        private void BurstCondition_GroupBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (BurstCondition_GroupBox.IsEnabled == true&& BurstCondition_GroupBox.Visibility == Visibility.Hidden)
            {

                setcondision.Add(Times_Value_Slider.Value.ToString());
                setcondision.Add(Duty_Value_Slider.Value.ToString());
                setcondision.Add(Freq_Value_Textbox.Text);
                Times_Value_Slider.Value = 0;
                Duty_Value_Slider.Value = 0;
                Freq_Value_Slider.Value = 0;
                Freq_Value_Textbox.Text = "0000";
            }
            if (BurstCondition_GroupBox.Visibility == Visibility.Visible && BurstCondition_GroupBox.IsEnabled == false)
            {
                setcondision.Clear();
            }
        }
        private void Shot_GroupBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Shot_GroupBox.Visibility == Visibility.Visible)
            {
                BurstCondition_GroupBox.Width = 325;
               FunctionClass.VisonChange(TotalTime_GroupBox, true);
            }
            else
            {
                BurstCondition_GroupBox.Width = 410;
                FunctionClass.VisonChange(TotalTime_GroupBox, false);
                FunctionClass.VisonChange(BurstCondition_GroupBox, false);
            }
        }
        private void Control_Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Control_Grid.Visibility == Visibility.Visible)
            {
                FunctionClass.VisonChange(First_sellect_GroupBox, true);
            }
            else
            {
                Preset_CheckBox.IsChecked = false;
                Costom_CheckBox.IsChecked = false;
                Triger_CheckBox.IsChecked = false;
                Shot_CheckBox.IsChecked = false;
                FunctionClass.VisonChange(First_sellect_GroupBox, false);
            }
        }

        private void BrustShot_Button_Click(object sender, RoutedEventArgs e)
        {
            TrigerSerial.Write("2");
            if (Shot_Timer.IsEnabled)
            {
                Shot_Timer.Tick -= new EventHandler(timer_Tick);
                Shot_Timer.Stop();
            }
            Shot_Timer.Interval = TimeSpan.FromSeconds(Convert.ToInt16(Times_Value_Textbox.Text) + Convert.ToInt16(Delay_TextBox.Text));
            TotalTime_GroupBox.IsEnabled = false;
            Shot_Timer.Start();
            Shot_Timer.Tick += new EventHandler(timer_Tick);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                int i = Convert.ToInt16(TotalTime_TextBox.Text) * 60;
                i = i / (Convert.ToInt16(Times_Value_Textbox.Text) + Convert.ToInt16(Delay_TextBox.Text));
                decimal repeat_num = Math.Truncate((decimal)i) - 1;
                if (i != 0)
                {
                    if (Reapeat_Checker == repeat_num)
                    {
                        Reapeat_Checker = 0;
                        Shot_Timer.Tick -= new EventHandler(timer_Tick);
                        Shot_Timer.Stop();
                        TotalTime_GroupBox.IsEnabled = true;

                    }
                    else
                    {
                        Reapeat_Checker++;
                        BrustShot_Button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                    }
                }
                else
                {
                    Shot_Timer.Tick -= new EventHandler(timer_Tick);
                    Shot_Timer.Stop();
                    TotalTime_GroupBox.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void RecivedData_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecivedData_TextBox.ScrollToEnd();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Enable_Button_Click(object sender, RoutedEventArgs e)
        {

            if (Enable_Button.Content.ToString() == "Ready")
            {
                if (Triger_CheckBox.IsChecked == true)
                {
                    MessageBox.Show("셋팅 값이 존재하지 않습니다");
                }
                else
                {
                    TrigerSerial.Write("3");
                    System.Threading.Thread.Sleep(300);
                    Enable_Button.Content = Enabled;
                    Enabled = "Ready";
                }
            }
            else
            {

                if (Shot_CheckBox.IsChecked == true)
                {
                    if (MessageBox.Show("Shot 모드를 끝내시겠습니까?", "입력값 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Triger_CheckBox.IsChecked = true;
                        Costom_CheckBox.IsChecked = true;
                    }
                }
                else
                {
                    TrigerSerial.Write("3");
                    System.Threading.Thread.Sleep(300);
                }
                Enabled = "Ready";
                Enable_Button.Content = Enabled;
                Enabled = "UnReady";

            }
            LED_color_Change(Enable_Button);
        }

        private void LED_color_Change(Button enable_Button)
        {
            if (Enable_Button.Content.ToString() == "Ready")
            {
                enable_Button.BorderBrush = mySolidColorBrush;
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 0, 0);

            }
            else
            {
                enable_Button.BorderBrush = mySolidColorBrush;
                mySolidColorBrush.Color = Color.FromArgb(255, 0, 255, 0);
            }
        }

        private void Freq_Value_Slider_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Freq_Value_Slider.Value == 1)
            {
                Freq_Value_Slider.LargeChange = 9;
            }
            else Freq_Value_Slider.LargeChange = 10;

        }

        private void Freq_Value_Slider_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Freq_Value_Slider.Value == 1)
            {
                Freq_Value_Slider.LargeChange = 9;
            }
            else Freq_Value_Slider.LargeChange = 10;
        }

        private void Freq_Value_Slider_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Freq_Value_Slider.Value == 1)
            {
                Freq_Value_Slider.LargeChange = 9;
            }
            else Freq_Value_Slider.LargeChange = 10;
        }

        private void RecivedData_TextBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RecivedData_TextBox.Text = "";
        }
    }
}
