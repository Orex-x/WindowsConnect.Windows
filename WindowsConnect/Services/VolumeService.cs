using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsConnect.Services
{
    public class VolumeService
    {
        CoreAudioDevice _defaultPlaybackDevice;
        public VolumeService()
        {
            _defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
        }

        public void setVolume(int volume)
        {
            if(volume >= 0 && volume <= 100)
            {
                _defaultPlaybackDevice.Volume = volume;
            }
        }
    }
}
