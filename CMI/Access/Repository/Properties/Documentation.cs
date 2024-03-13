using CMI.Utilities.Common;

namespace CMI.Access.Repository.Properties
{
    public class Documentation : AbstractDocumentation
    {
        public override void LoadDescriptions()
        {
            AddDescription<Settings>(x => x.ConnectionMode, "Connection Mode zum DIR");
            AddDescription<Settings>(x => x.FixityAlgorithmRefElementName, "Algorithmus zum DIR");
            AddDescription<Settings>(x => x.FixityValueElementName, "Fixity Value fürs DIR");
            AddDescription<Settings>(x => x.RepositoryPassword, "Passwort für die Schnittstelle zum DIR/Rosetta");
            AddDescription<Settings>(x => x.RepositoryExportIEUrl, "URL des DIR mit Rest aufruf zum export des IEs");
            AddDescription<Settings>(x => x.RepositoryServiceUrl, "URL des DIR");
            AddDescription<Settings>(x => x.RepositoryUser, "Benutzer für die Schnittstelle zum DIR/Rosetta");
            AddDescription<Settings>(x => x.RepositoryDirectoryUser, "User vom NAS");
            AddDescription<Settings>(x => x.RepositoryDirectoryPassword, "Passwort vom NAS");
            AddDescription<Settings>(x => x.RepositoryDirectory, "Name des Netzlaufwerks");
            AddDescription<Settings>(x => x.RepositoryDomain, "Domaine in der das Netzlaufwerks ist");
        }
    }
}