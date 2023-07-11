namespace CMI.Contract.Messaging
{
    public interface IArchiveRecordRemoved
    {
        long MutationId { get; set; }
        bool ActionSuccessful { get; set; }
        string ErrorMessage { get; set; }
        string StackTrace { get; set; }
    }
}