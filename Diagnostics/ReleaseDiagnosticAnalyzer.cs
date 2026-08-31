//using AkengMauiCrashReporter.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AkengMauiCrashReporter.Diagnostics
//{
//    internal sealed class ReleaseDiagnosticAnalyzer
//    {
//        public ReleaseDiagnostic Analyze(CrashClassification classification, EnvironmentSnapshot environment)
//        {
//            if (!environment.IsRelease)
//            {
//                return new ReleaseDiagnostic
//                {
//                    IsSuspected = false
//                };
//            }

//            var score = 0;

//            var signals = new List<string>();

//            if (classification.IsReleaseSensitive)
//            {
//                score += 4;
//                signals.Add("Crash category is known to be sensitive to Release builds.");
//            }

//            if (environment.IsTrimmingEnabled == true)
//            {
//                score += 3;

//                signals.Add(
//                    "Trimming is enabled.");
//            }

//            if (environment.IsAotEnabled == true)
//            {
//                score += 2;

//                signals.Add(
//                    "AOT is enabled.");
//            }

//            if (!environment.IsDebuggerAttached)
//            {
//                score += 1;

//                signals.Add(
//                    "Debugger is not attached.");
//            }

//            if (score == 0)
//            {
//                return new ReleaseDiagnostic
//                {
//                    IsSuspected = false
//                };
//            }

//            return new ReleaseDiagnostic
//            {
//                IsSuspected = score >= 4,

//                Confidence =
//                    Math.Round(
//                        Math.Min(
//                            score / 10.0,
//                            1.0),
//                        2),

//                Reason =
//                    "The crash contains signals commonly associated with Release-only failures.",

//                Signals =
//                    signals
//            };
//        }
//    }
//}
