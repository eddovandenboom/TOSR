﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Common;
using static Solver.DealHands;

namespace Solver
{
    public class SingleDummySolver
    {
        public static List<int> SolveSingleDummy(int trumpSuit, int declarer, string northHand, string southHand)
        {
            var handsForSolver = GetHandsForSolver(northHand, southHand, 10).ToArray();
            return Api.SolveAllBoards(handsForSolver, trumpSuit, declarer).ToList();
        }

        private static IEnumerable<string> GetHandsForSolver(string northHandStr, string southHandStr, int nrOfHands)
        {
            var northHand = northHandStr.Split(',');
            var southHand = southHandStr.Split(',');
            var northHandCards = GetCardDtosFromString(northHand);

            for (int i = 0; i < nrOfHands; i++)
            {
                // Also randomize partners hand
                var southHandCards = GetCardDtosFromStringWithx(southHand, northHand);
                // Shuffle
                var deal = Shuffling.FisherYates(northHandCards, southHandCards).ToList();
                var handStrs = GetDealAsString(deal);
                yield return handStrs.Aggregate("W:", (current, hand) => current + hand.handStr.Replace(',', '.') + " ");
            }
        }

        public static List<int> SolveSingleDummy(int trumpSuit, int declarer, string northHand, string southHandShape, int minControls, int maxControls)
        {
            var handsForSolver = GetHandsForSolver(northHand, southHandShape, minControls, maxControls, 10).ToArray();
            return Api.SolveAllBoards(handsForSolver, trumpSuit, declarer).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="northHandStr">Whole northhand. Cannot contain x's</param>
        /// <param name="southHandShape">For example 5431</param>
        /// <param name="minControls">Number of controls in the southhand</param>
        /// <param name="nrOfHands"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetHandsForSolver(string northHandStr, string southHandShape, int minControls, int maxControls, int nrOfHands)
        {
            var shuffleRestrictions = new ShuffleRestrictions
            {
                shape = southHandShape,
                restrictShape = true,
            };
            shuffleRestrictions.SetControls(minControls, maxControls);

            var northHandCards = GetCardDtosFromString(northHandStr.Split(','));

            for (int i = 0; i < nrOfHands; i++)
            {
                string southHand;
                List<CardDto> cards;
                do
                {
                    cards = Shuffling.FisherYates(northHandCards, new List<CardDto>{ }).ToList();
                    var orderedCardsSouth = cards.Skip(13).Take(13).OrderByDescending(x => x.Suit).ThenByDescending(c => c.Face, new FaceComparer());
                    southHand = Util.GetDeckAsString(orderedCardsSouth);
                }
                while
                    (!shuffleRestrictions.Match(southHand));

                var handStrs = GetDealAsString(cards);
                yield return handStrs.Aggregate("W:", (current, hand) => current + hand.handStr.Replace(',', '.') + " ");
            }
        }

        public static List<int> SolveSingleDummy2(int trumpSuit, int declarer, string northHandStr, SouthHandInfo southHandInfo)
        {
            var handsForSolver = GetHandsForSolver2(northHandStr, southHandInfo, 10);
            var scores = Api.SolveAllBoards(handsForSolver, trumpSuit, declarer).ToList();
            for (int i = 0; i < scores.Count; i++)
            {
                Console.WriteLine($"Deal: {handsForSolver[i]} Nr of tricks:{scores[i]}");
            }
            return scores;
        }

        private static string[] GetHandsForSolver2(string northHandStr, SouthHandInfo southHandInfo, int nrOfHands)
        {
            var directory = @".\..\..\..\..\Dealer";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = directory;
            startInfo.FileName = Path.Combine(directory, "Dealer.exe");
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            File.WriteAllText(Path.Combine(directory, "custom.tcl"), getTCLInput(northHandStr, southHandInfo));
            startInfo.Arguments = $"-i format/pbn -i custom.tcl {nrOfHands.ToString()}";
            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception($"Dealer has incorrect exit code: {process.ExitCode}");
                }
                return process.StandardOutput.ReadToEnd().Split("\n").Where(x => x.StartsWith("[")).Select(x => x.Substring(7, 69)).ToArray();
            }
        }

        private static IEnumerable<(Player player, string handStr)> GetDealAsString(IEnumerable<CardDto> deal)
        {
            foreach (Player player in Enum.GetValues(typeof(Player)))
            {
                if (player == Player.UnKnown)
                    continue;
                var cardsPlayer = player switch
                {
                    Player.West => deal.Skip(26).Take(13),
                    Player.North => deal.Take(13),
                    Player.East => deal.Skip(39).Take(13),
                    Player.South => deal.Skip(13).Take(13),
                    _ => throw new ArgumentException(nameof(player)),
                };
                var orderedCards = cardsPlayer.OrderByDescending(x => x.Suit).ThenByDescending(c => c.Face, new FaceComparer());
                var dealStr = Util.GetDeckAsString(orderedCards);
                yield return (player, dealStr);
            }
        }

        private static IEnumerable<CardDto> GetCardDtosFromString(string[] hand)
        {
            for (var suit = 0; suit < hand.Count(); ++suit)
                foreach (var card in hand[suit])
                {
                    yield return new CardDto() { Suit = (Suit)(3 - suit), Face = Util.GetFaceFromDescription(card) };
                }
        }

        private static IEnumerable<CardDto> GetCardDtosFromStringWithx(string[] hand, string[] partnersHand)
        {
            var random = new Random();

            for (var suit = 0; suit < hand.Count(); ++suit)
            {
                var remainingCards = Enumerable.Range(1, 10).Where(x => !partnersHand[suit].Contains(Util.GetFaceDescription((Face)x))).Select(x => (Face)x).ToList();
                foreach (var card in hand[suit])
                {
                    Face face;
                    if (card != 'x')
                    {
                        face = Util.GetFaceFromDescription(card);
                    }
                    else
                    {
                        var c = random.Next(0, remainingCards.Count());
                        face = remainingCards.ElementAt(c);
                        remainingCards.RemoveAt(c);
                    }
                    yield return new CardDto() { Suit = (Suit)(3 - suit), Face = face };
                }
            }
        }
    }
}
