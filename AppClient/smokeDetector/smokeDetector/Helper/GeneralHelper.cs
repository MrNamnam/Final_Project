using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace smokeDetector.Helper
{
    class GeneralHelper
    {
        public static void PlayMusic()
        {
            var service = DependencyService.Get<IPlatformPlay>();
            var file = service.PlayMusicFile();

        }
        public static void PauseMusic()
        {
            var service = DependencyService.Get<IPlatformPlay>();
            var file = service.PauseMusicFile();
        }

    }
}
