using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace InspectionAI.Classes.Managers
{
    /// <summary>
    /// Manager untuk handle sound alerts
    /// </summary>
    public class SoundAlertManager
    {
        private static SoundPlayer ngSoundPlayer;
        private static SoundPlayer warningSoundPlayer;
        private static bool isEnabled = true;

        static SoundAlertManager()
        {
            // Initialize sound players (akan load dari resources atau file)
            try
            {
                // Path ke sound files
                string ngSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sounds", "ng_alert.wav");
                string warnSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sounds", "warning_alert.wav");

                if (File.Exists(ngSoundPath))
                    ngSoundPlayer = new SoundPlayer(ngSoundPath);
                
                if (File.Exists(warnSoundPath))
                    warningSoundPlayer = new SoundPlayer(warnSoundPath);
            }
            catch
            {
                // Jika file tidak ada, akan pakai SystemSound
            }
        }

        /// <summary>
        /// Play NG alert sound
        /// </summary>
        public static void PlayNGAlert()
        {
            if (!isEnabled) return;

            Task.Run(() =>
            {
                try
                {
                    if (ngSoundPlayer != null)
                    {
                        ngSoundPlayer.Play();
                    }
                    else
                    {
                        // Fallback ke system beep
                        SystemSounds.Hand.Play(); // Error sound
                    }
                }
                catch
                {
                    // Ignore sound errors
                }
            });
        }

        /// <summary>
        /// Play Warning alert sound
        /// </summary>
        public static void PlayWarningAlert()
        {
            if (!isEnabled) return;

            Task.Run(() =>
            {
                try
                {
                    if (warningSoundPlayer != null)
                    {
                        warningSoundPlayer.Play();
                    }
                    else
                    {
                        SystemSounds.Exclamation.Play();
                    }
                }
                catch
                {
                    // Ignore sound errors
                }
            });
        }

        /// <summary>
        /// Enable/disable sound alerts
        /// </summary>
        public static void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }

        /// <summary>
        /// Generate simple beep sound untuk testing
        /// Frequency: 1000Hz (NG) atau 500Hz (Warning)
        /// Duration: 200ms
        /// </summary>
        public static void PlayBeep(int frequency = 1000, int duration = 200)
        {
            if (!isEnabled) return;

            Task.Run(() =>
            {
                try
                {
                    Console.Beep(frequency, duration);
                }
                catch
                {
                    // Some systems don't support Console.Beep
                    SystemSounds.Beep.Play();
                }
            });
        }
    }
}
