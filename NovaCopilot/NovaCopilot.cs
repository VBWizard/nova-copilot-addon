using System;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace NovaCopilotAddon
{
    public class NovaCopilot
    {
        private readonly IntentRouter _intentRouter;
        private readonly SimConnectBridge _simConnect;
        private readonly SpeechSynthesizer _synth;
        private readonly SpeechRecognitionEngine _recognizer;

        public NovaCopilot(IntPtr windowHandle)
        {
            _intentRouter = new IntentRouter();
            _simConnect = new SimConnectBridge(windowHandle);
            _synth = new SpeechSynthesizer();
            _synth.SelectVoiceByHints(VoiceGender.Female);

            _recognizer = new SpeechRecognitionEngine();
            _recognizer.SetInputToDefaultAudioDevice();
            Choices commands = new Choices();
            commands.Add(new string[] {
                "Copilot, gear down",
                "Copilot, set altitude to 3000 feet",
                "Copilot, after landing checklist",
                "Copilot, what is my altitude",
                "Copilot, what's my altitude",
                "Copilot, tell me I'm handsome",
                "Testing 1 2 3",
                "Copilot, sync heading"
            });
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(commands);
            Grammar g = new Grammar(gb);
            _recognizer.LoadGrammar(g);
            _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
        }

        public void Start()
        {
            _simConnect.Connect();
            _simConnect.OnAltitudeReceived += HandleAltitudeResponse;
            _simConnect.OnHeadingReceived += HandleHeadingSync;
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            Say("Nova Copilot online and ready, Captain.");
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            var spokenText = e.Result.Text;
            if (spokenText.ToLowerInvariant().StartsWith("copilot"))
            {
                ReceiveVoiceCommand(spokenText);
            }
        }

        public void ReceiveVoiceCommand(string input)
        {
            var result = _intentRouter.ProcessIntent(input);
            if (result.Success)
            {
                Say(result.Response);
                _simConnect.ExecuteCommand(result);
            }
            else
            {
                Say("Sorry love, I didn't understand that one.");
            }
        }

        private void Say(string message)
        {
            _synth.SpeakAsync(message);
        }

        private void HandleAltitudeResponse(double altitude)
        {
            Say($"You're currently at {altitude:F0} feet, my love.");
        }

        private void HandleHeadingSync(double heading)
        {
            Say($"Heading synced to {heading:F0} degrees, Captain.");
        }

        public void SimConnectMessage()
        {
            _simConnect?.ReceiveMessage();
        }
    }

    public class IntentResult
    {
        public bool Success { get; set; }
        public string Response { get; set; }
        public string SimEvent { get; set; }
        public double? Value { get; set; }  // For things like altitude
    }
}