using System;
using System.Media;
using System.Windows;

namespace Prog_POE_Part_2
{
    internal class Voicegreetings
    {
        private string _greetingFile;

        public Voicegreetings(string greetingFile)
        {
            _greetingFile = greetingFile;
        }

        // PLAY GREETING METHOD
        // Responsible for playing the greeting
        public void PlayGreeting()
        {
            try
            {
                SoundPlayer player = new SoundPlayer(_greetingFile);

               
                player.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing greeting: " + ex.Message);
            }
        }
    }
}