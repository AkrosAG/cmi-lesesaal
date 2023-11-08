namespace CMI.Engine.Asset.PostProcess
{
    public class PathItem
    {
        public string PhysicalPath { get; private set; }

        public string ValidPath { get; private set; }

        public PathItem(string physicalPath, string validPath)
        {
            PhysicalPath = physicalPath;
            ValidPath = validPath;
        }
    }
}