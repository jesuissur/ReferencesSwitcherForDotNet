using System;

namespace ReferencesSwitcherForDotNet.Library
{
    public class ConsoleUserInteraction : IUserInteraction
    {
        public bool AskQuestion(string question)
        {
            Console.WriteLine(question + " [Y or N + Enter]");
            return Console.ReadLine()?.ToLower() == "y";
        }

        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}