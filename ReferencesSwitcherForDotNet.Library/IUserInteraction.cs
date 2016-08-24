namespace ReferencesSwitcherForDotNet.Library
{
    public interface IUserInteraction
    {
        bool AskQuestion(string question);
        void DisplayMessage(string message);
    }
}