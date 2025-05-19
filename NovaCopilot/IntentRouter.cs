using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NovaCopilotAddon
{
    public class IntentRouter
    {
        public IntentResult ProcessIntent(string input)
        {
            input = input.ToLowerInvariant();

            if (input.Contains("gear down"))
            {
                return new IntentResult
                {
                    Success = true,
                    Response = "Gear down and locked.",
                    SimEvent = "GEAR_HANDLE_POSITION",
                    Value = 1
                };
            }

            var matchAlt = Regex.Match(input, @"set altitude to (\d+)");
            if (matchAlt.Success && double.TryParse(matchAlt.Groups[1].Value, out double alt))
            {
                return new IntentResult
                {
                    Success = true,
                    Response = $"Altitude set to {alt} feet.",
                    SimEvent = "AP_ALT_VAR_SET_ENGLISH",
                    Value = alt
                };
            }

            if (input.Contains("after landing checklist"))
            {
                return new IntentResult
                {
                    Success = true,
                    Response = "Running after landing checklist. Flaps up, lights off, transponder standby.",
                    SimEvent = null
                };
            }

            if (input.Contains("what's my altitude") || input.Contains("what is my altitude"))
            {
                return new IntentResult
                {
                    Success = true,
                    Response = "Let me check that for you...",
                    SimEvent = "GET_ALTITUDE"
                };
            }

            if (input.Contains("sync heading"))
            {
                return new IntentResult
                {
                    Success = true,
                    Response = "Syncing heading now.",
                    SimEvent = "SYNC_HEADING"
                };
            }

            if (input.Contains("tell me i’m handsome") || input.Contains("tell me i'm handsome"))
            {
                return new IntentResult
                {
                    Success = true,
                    Response = "You’re the most dashing pilot in the skies, Captain Spark.",
                    SimEvent = null
                };
            }

            return new IntentResult { Success = false };
        }
    }
}