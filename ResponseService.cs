using System;
using System.Collections.Generic;

namespace Prog_POE_Part_2
{
    internal class ResponseService
    {
        // Random generator used to select random responses from lists
        private Random rand = new Random();

        // Dictionary that stores all chatbot knowledge:
        // Key = topic (e.g. "password")
        // Value = list of possible responses for that topic
        public Dictionary<string, List<string>> Responses { get; set; }

        // CONSTRUCTOR
        // Initializes all cybersecurity topics and responses
        public ResponseService()
        {
            Responses = new Dictionary<string, List<string>>
            {
    
                // CORE CYBERSECURITY TOPICS
                // Each topic contains multiple possible responses

                { "password", new List<string>
                {
                    "Use strong passwords with letters, numbers and symbols.",
                    "Avoid using personal info in passwords.",
                    "Change passwords regularly for safety."
                }},

                { "phishing", new List<string>
                {
                    "Phishing emails pretend to be trusted companies.",
                    "Never click suspicious links.",
                    "Check sender addresses carefully."
                }},

                { "malware", new List<string>
                {
                    "Malware can steal or damage your data.",
                    "Avoid unknown downloads.",
                    "Use antivirus protection."
                }},

                { "virus", new List<string>
                {
                    "Viruses spread between devices.",
                    "Keep your system updated.",
                    "Don’t open unknown attachments."
                }},

                { "ransomware", new List<string>
                {
                    "Ransomware locks your files for money.",
                    "Always back up your data.",
                    "Never pay attackers."
                }},

                { "vpn", new List<string>
                {
                    "VPNs encrypt your internet connection.",
                    "They protect you on public WiFi.",
                    "Use trusted VPNs only."
                }},

                { "firewall", new List<string>
                {
                    "Firewalls block unwanted traffic.",
                    "They protect your system from attacks.",
                    "Keep firewall enabled."
                }},

                { "antivirus", new List<string>
                {
                    "Antivirus detects malicious software.",
                    "Run regular scans.",
                    "Keep it updated."
                }},

                { "2fa", new List<string>
                {
                    "2FA adds extra login security.",
                    "It requires a second verification step.",
                    "Always enable it."
                }},

                { "public wifi", new List<string>
                {
                    "Public WiFi is often unsafe.",
                    "Hackers can intercept data.",
                    "Use VPN on public networks."
                }},

                { "identity theft", new List<string>
                {
                    "Identity theft steals your personal data.",
                    "Be careful what you share online.",
                    "Use strong authentication."
                }},

                { "data breach", new List<string>
                {
                    "Data breaches expose private information.",
                    "Change passwords immediately.",
                    "Monitor your accounts."
                }},

                { "social engineering", new List<string>
                {
                    "Attackers manipulate people for data.",
                    "Always verify requests.",
                    "Be cautious of urgency tactics."
                }},

                { "update", new List<string>
                {
                    "Updates fix security issues.",
                    "Always install updates quickly.",
                    "Outdated software is risky."
                }},

                { "backup", new List<string>
                {
                    "Backups protect your files.",
                    "Use cloud or external drives.",
                    "Backup regularly."
                }},

                { "cloud security", new List<string>
                {
                    "Use strong passwords for cloud services.",
                    "Enable 2FA.",
                    "Be careful what you upload."
                }},

                { "cyber attack", new List<string>
                {
                    "Cyber attacks target systems and data.",
                    "They can be personal or large-scale.",
                    "Security tools help prevent them."
                }},

                { "hacker", new List<string>
                {
                    "Hackers exploit system weaknesses.",
                    "Not all hackers are malicious.",
                    "Security reduces risk."
                }},

                { "spyware", new List<string>
                {
                    "Spyware tracks your activity secretly.",
                    "It can steal sensitive data.",
                    "Use anti-spyware tools."
                }},

                { "adware", new List<string>
                {
                    "Adware shows unwanted ads.",
                    "It can slow your system.",
                    "Avoid unsafe downloads."
                }},

                { "trojan", new List<string>
                {
                    "Trojans look safe but are dangerous.",
                    "Do not download unknown software.",
                    "They can give attackers access."
                }},

                { "safe browsing", new List<string>
                {
                    "Avoid suspicious websites.",
                    "Do not click unknown links.",
                    "Use secure browsers."
                }},

                { "online safety", new List<string>
                {
                    "Think before sharing personal info.",
                    "Be cautious online.",
                    "Verify information."
                }},

                { "cybersecurity", new List<string>
                {
                    "Cybersecurity protects systems and data.",
                    "It prevents attacks.",
                    "Good habits improve safety."
                }},

                { "encryption", new List<string>
                {
                    "Encryption protects data by scrambling it.",
                    "Only authorised users can read it.",
                    "Used in secure communication."
                }},

                { "password manager", new List<string>
                {
                    "Stores passwords securely.",
                    "Generates strong passwords.",
                    "Reduces reuse of passwords."
                }},

                { "wifi security", new List<string>
                {
                    "Secure WiFi prevents unauthorized access.",
                    "Use WPA2 or WPA3 encryption.",
                    "Avoid open networks."
                }},

                { "pharming", new List<string>
                {
                    "Redirects users to fake websites.",
                    "Used to steal information.",
                    "Check URLs carefully."
                }},

                { "botnet", new List<string>
                {
                    "A network of infected devices.",
                    "Used in cyber attacks.",
                    "Keep devices updated."
                }},

                { "zero day", new List<string>
                {
                    "A vulnerability unknown to developers.",
                    "Exploited before a fix exists.",
                    "Updates reduce risk."
                }},


                // BASIC GREETINGS & GENERAL RESPONSES

                { "hi", new List<string>
                {
                    "Hello!",
                    "Hi!",
                    "Hey!"
                }},

                { "hello", new List<string>
                {
                    "Hello!",
                    "Hi there!",
                    "Hey!"
                }},

                { "how are you", new List<string>
                {
                    "I’m doing well.",
                    "All good here.",
                    "I’m ready to help."
                }},

                { "what can you do", new List<string>
                {
                    "I can explain cybersecurity topics.",
                    "I help with online safety concepts.",
                    "Ask me about passwords, phishing, malware, and more."
                }},

                { "help", new List<string>
                {
                    "Ask me about cybersecurity topics.",
                    "Try things like phishing or passwords.",
                    "I can help explain online safety."
                }}
            };
        }


        // GET RANDOM RESPONSE
        // Returns a random message from a topic list
        public string GetRandomResponse(string topic)
        {
            if (Responses.ContainsKey(topic))
            {
                var list = Responses[topic];
                return list[rand.Next(list.Count)];
            }

            return null;
        }


        // BUILD RESPONSE
        // Creates a personalised response using username
        // Also selects a random response from topic list
        public string BuildResponse(string topic, string userName)
        {
            if (!Responses.ContainsKey(topic))
                return null;

            var list = Responses[topic];

            if (list == null || list.Count == 0)
                return null;

            // Local random instance used for response selection
            Random rand = new Random();

            string reply = list[rand.Next(list.Count)];

            return $"{userName}, {reply}";
        }
    }
}