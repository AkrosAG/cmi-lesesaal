namespace CMI.Contract.Messaging
{
    public interface IArchiveRecordUpdated
    {
        long MutationId { get; set; }
        bool ActionSuccessful { get; set; }
        string ErrorMessage { get; set; }
        string StackTrace { get; set; }
        int PrimaerdatenAuftragId { get; set; }
        string ArchiveRecordId { get; set; }
    }
}