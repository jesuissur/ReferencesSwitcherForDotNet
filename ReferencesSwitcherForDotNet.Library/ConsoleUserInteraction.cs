using System;

namespace ReferencesSwitcherForDotNet.Library
{
    public class ConsoleUserInteraction : IUserInteraction
    {
        public string AskQuestion(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine();
        }

        public bool AskYesNoQuestion(string question)
        {
            return AskQuestion(question + " [Y or N + Enter]")?.ToLower() == "y";
        }

        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}