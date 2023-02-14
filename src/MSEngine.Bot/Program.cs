using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MSEngine.Core;
using MSEngine.Solver;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

namespace MSEngine.Bot
{
    class Program
    {
        const int nodeCount = 480;
        const int mineCount = 99;
        const int columns = 30;

        static void Main(string[] args)
        {
            var options = new ChromeOptions();
            //options.AddArgument("-headless");
            using var driver = new ChromeDriver(options);

            // some key aspects have been omitted from this bot
            // also, performance is purposefully degraded
            driver.Navigate().GoToUrl("");

            var squares = driver
                .FindElement(By.Id("game"))
                .FindElements(By.ClassName("square"))
                .Where(x => x.Displayed)
                .ToList();

            Debug.Assert(squares.Count == nodeCount);

            LeftClickNode(squares, 93);

            var buffs = new BufferKeeper
            {
                Turns = stackalloc Turn[nodeCount],
                EdgeIndexes = stackalloc int[Engine.MaxNodeEdges],
                Mines = stackalloc int[mineCount],
                RevealedMineCountNodeIndexes = stackalloc int[nodeCount - mineCount],
                AdjacentHiddenNodeIndexes = stackalloc int[nodeCount],
                Grid = stackalloc float[nodeCount * nodeCount]
            };

            while (IsGamePending(driver))
            {
                var nodes = squares
                    .Select(x => x.GetAttribute("class"))
                    .Select((x, i) => new Node(i, false, GetMineCount(x), GetNodeState(x)))
                    .ToArray()
                    .AsSpan();

                var matrix = new Matrix<Node>(nodes, columns);
                var turnCount = MatrixSolver.CalculateTurns(matrix, buffs, true);

                if (turnCount == 0) { break; }

                // ignore the other turns for now
                var turn = buffs.Turns[0];
                
                if (turn.Operation == NodeOperation.Reveal)
                {
                    LeftClickNode(squares, turn.NodeIndex);
                }
                else
                {
                    RightClickNode(squares, turn.NodeIndex, driver);
                }

                Console.WriteLine(turn);
            }

            Console.WriteLine("finished");
            Console.ReadLine();
        }

        public static bool IsGamePending(IWebDriver driver)
            => !IsAlertPresent(driver)
                && driver.FindElements(By.ClassName("facesmile")).Count == 1;

        public static byte GetMineCount(string foo)
            => (byte)(foo.Contains("open0") ? 0
                : foo.Contains("open1") ? 1
                : foo.Contains("open2") ? 2
                : foo.Contains("open3") ? 3
                : foo.Contains("open4") ? 4
                : foo.Contains("open5") ? 5
                : foo.Contains("open6") ? 6
                : foo.Contains("open7") ? 7
                : foo.Contains("open8") ? 8
                : 0);

        public static NodeState GetNodeState(string foo)
            => foo.Contains("blank") ? NodeState.Hidden
                : foo.Contains("bombflagged") ? NodeState.Flagged
                : NodeState.Revealed;

        public static void LeftClickNode(IEnumerable<IWebElement> nodes, int index)
            => nodes
                .ElementAt(index)
                .Click();

        public static void RightClickNode(IEnumerable<IWebElement> nodes, int index, IWebDriver driver)
        {
            var node = nodes.ElementAt(index);

            new Actions(driver)
                .ContextClick(node)
                .Build()
                .Perform();
        }

        public static bool IsAlertPresent(IWebDriver driver)
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }
    }
}
