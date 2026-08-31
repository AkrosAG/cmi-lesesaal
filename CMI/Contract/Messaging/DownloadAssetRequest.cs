using System;
using System.Collections.Generic;
using CMI.Contract.Asset;
using CMI.Contract.Common;

namespace CMI.Contract.Messaging
{
    public class DownloadAssetRequest
    {
        public string AssetId { get; set; }
        public string ArchiveRecordId { get; set; }
        public int OrderItemId { get; set; }
        public AssetType AssetType { get; set; }
        public CacheRetentionCategory RetentionCategory { get; set; }
        public string Recipient { get; set; }
        public bool ForceSendPasswordMail { get; set; }
    }

    public class DownloadAssetResult
    {
        public string AssetDownloadLink { get; set; }
    }

    public enum AssetDownloadStatus
    {
        InCache,
        InPreparationQueue,
        RequiresPreparation
    }


    public class PrepareAssetRequest
    {
        public string AssetId { get; set; }
        public string ArchiveRecordId { get; set; }
        public string CallerId { get; set; }
        public AssetType AssetType { get; set; }
        public CacheRetentionCategory RetentionCategory { get; set; }
        public string Recipient { get; set; }
        public string DeepLinkToVe { get; set; }
        public string Language { get; set; }
    }

    public class PrepareAssetResult
    {
        public AssetDownloadStatus Status { get; set; }
        public DateTime InQueueSince { get; set; }
        public DateTime EstimatedPreparationEnd { get; set; }
        public TimeSpan EstimatedPreparationDuration { get; set; }
    }

    public class GetAssetStatusRequest
    {
        public string AssetId { get; set; }
        public AssetType AssetType { get; set; }
        public string ArchiveRecordId { get; set; }
        public string CallerId { get; set; }
        public CacheRetentionCategory RetentionCategory { get; set; }
    }

    public class GetAssetStatusResult
    {
        public AssetDownloadStatus Status { get; set; }
        public DateTime InQueueSince { get; set; }
        public DateTime EstimatedPreparationEnd { get; set; }
        public TimeSpan EstimatedPreparationDuration { get; set; }
        public long FileSizeInBytes { get; set; }
    }

    public class GetTitlePageForAISFilesRequest
    {
        /// <summary>
        /// Metadaten als Key-Value-Paare. Die Schlüssel entsprechen direkt den {{placeholder}}-Namen
        /// im Titelblatt-Template (z.B. "titel", "signatur", "permanente_url").
        /// Neue Felder können hinzugefügt werden, ohne den Request anzupassen.
        /// </summary>
        public Dictionary<string, string> Metadaten { get; set; }
    }

    public class GetTitlePageForAISFilesResult
    {
        /// <summary>
        /// Die generierte Titelseite als PDF-Bytes.
        /// Ist null wenn kein Template konfiguriert ist oder das Template fehlerhaft ist.
        /// </summary>
        public byte[] TitlePagePdfBytes { get; set; }
    }
}