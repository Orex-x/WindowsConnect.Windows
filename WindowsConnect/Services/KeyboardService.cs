using System;
using System.Windows;
using System.Windows.Input;
using WindowsInput;

namespace WindowsConnect.Services
{

    public class KeyboardService
    {
        private InputSimulator _inputSimulator;

        public KeyboardService()
        {
            _inputSimulator = new InputSimulator();
        }


        public void press(char ch)
        {
            try
            {
                _inputSimulator.Keyboard.TextEntry(ch);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception {ex.Message}");
            }
        }

        public void down(int code)
        {
            try
            {
                _inputSimulator.Keyboard.KeyDown((WindowsInput.Native.VirtualKeyCode) code);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception {ex.Message}");
            }
        }

        public void up(int code)
        {
            try
            {
                _inputSimulator.Keyboard.KeyUp((WindowsInput.Native.VirtualKeyCode) code);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception {ex.Message}");
            }
        }
    }
}
