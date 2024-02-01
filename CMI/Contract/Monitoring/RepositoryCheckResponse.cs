namespace CMI.Contract.Monitoring
{
    public class RepositoryCheckResponse
    {
        public bool Ok { get; set; }


        public string RepositoryName { get; set; }

        public string ProductName { get; set; }

        public string ProductVersion { get; set; }
    }
}