using AudioSwitcher.AudioApi.CoreAudio;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WindowsConnect.Services
{
    public class VolumeService
    {
        CoreAudioDevice _defaultPlaybackDevice;
        public VolumeService()
        {
            try
            {
                _defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception {e.Message}");
            }
        }

        public void SetVolume(int volume)
        {
            if(_defaultPlaybackDevice != null)
            {
                if (volume >= 0 && volume <= 100) _defaultPlaybackDevice.Volume = volume;
            }
        }

        public void Pause()
        {
            if (_defaultPlaybackDevice != null)
            {
                
            }
        }
    }
}
