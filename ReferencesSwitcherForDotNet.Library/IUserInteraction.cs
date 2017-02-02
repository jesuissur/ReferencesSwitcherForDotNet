namespace ReferencesSwitcherForDotNet.Library
{
    public interface IUserInteraction
    {
        bool AskYesNoQuestion(string question);
        void DisplayMessage(string message);
        string AskQuestion(string question);
    }
}