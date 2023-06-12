using System;
using System.Collections.Generic;
using CMI.Contract.Common;
using System.Linq;

namespace CMI.Access.Harvest.CMIAIS.Mapping
{
    public class ArchiveRecordMapperBuilder
    {
        private readonly ArchiveRecord archiveRecord;
        private readonly Verzeichnungseinheit cmiRecord;
        internal readonly LanguageSettings languageSettings;
        internal readonly IAISSpecificRecordAccess<Verzeichnungseinheit> cmiSpecificRecordAccess;
        private readonly Tektonik.Verzeichnungseinheit cmicRecordTectonic;

        public ArchiveRecordMapperBuilder(Verzeichnungseinheit cmiRecord, Tektonik.Verzeichnungseinheit cmicRecordTectonic, LanguageSettings languageSettings, IAISSpecificRecordAccess<Verzeichnungseinheit> cmiSpecificRecordAccess)
        {
            archiveRecord = new ArchiveRecord
            {
                ArchiveRecordId = cmiRecord.OBJ_GUID,
            };
            this.cmiRecord = cmiRecord;
            this.cmicRecordTectonic = cmicRecordTectonic;
            this.languageSettings = languageSettings;
            this.cmiSpecificRecordAccess = cmiSpecificRecordAccess;
        }

        public ArchiveRecord Build()
        {
            return archiveRecord;
        }

        public MetaDataBuilder AddMedataData()
        {
            archiveRecord.Metadata = new ArchiveRecordMetadata
            {
                AccessionDate = cmiRecord.Akzession?.FirstOrDefault()?.Datum?.Start?.Year ?? 0,
                References = GetReferences(cmiRecord),
                Descriptors = GetDescriptors(cmiRecord),
                Containers = GetContainers(cmiRecord),
            };

            return new MetaDataBuilder(cmiRecord, cmicRecordTectonic, archiveRecord, this);
        }

        private ArchiveRecordMetadataContainers GetContainers(Verzeichnungseinheit cmiRecord)
        {
            if (cmiRecord.Standort == null || cmiRecord.Standort.Length == 0)
                return null;

            var containers = new List<ArchiveRecordMetadataContainersContainer>();
            containers.AddRange(cmiRecord.Standort.OfType<MagazinObjekt>().Select(m => new ArchiveRecordMetadataContainersContainer
            {
                ContainerLocation = m.ParentMagazinobjekt?.Item?.Kuerzel ?? m.ParentGebauede?.Item?.Kuerzel,
                ContainerType = m.Art?.Item?.Bezeichnung,
                IdName = m.Bezeichnung,
                ContainerCode = m.Kuerzel
            }));
            
            return new ArchiveRecordMetadataContainers
            {
                Container = containers,
                NumberOfContainers = containers.Count
            };
        }

        private List<Descriptor> GetDescriptors(Verzeichnungseinheit cmiRecord)
        {
            return cmiRecord.Registerzuordnung?.Select(r =>
            {
                var register = r.Registereintrag?.Item;
                if (register == null)
                    return null;
                return new Descriptor
                {
                    Name = register.Bezeichnung,
                    Description = register.Bemerkung,
                    Thesaurus = register.Registertyp?.Item?.Bezeichnung,
                    Function = register.Rolle,
                    DateOfBirth = DateTime.TryParse(register.Geburtsdatum, out var dateValue) ? dateValue : null,
                    DateOfDeath = DateTime.TryParse(register.Sterbedatum, out dateValue) ? dateValue : null,
                    Source =  register.GNDID?.ToString() 
                };
            })
            .Where(r => r != null)
            .ToList();
        }

        private List<ArchiveRecordMetadataReference> GetReferences(Verzeichnungseinheit cmiRecord)
        {
            var verweiseVon = cmiRecord.VerwiesenVon?.Select(v => CreateVerweis(cmiRecord, v, "Verweis von")) ?? new List<ArchiveRecordMetadataReference>();
            var verweiseZu = cmiRecord.VerwiesenZu?.Select(v => CreateVerweis(cmiRecord, v, "Verweis zu")) ?? new List<ArchiveRecordMetadataReference>();

            return verweiseVon.Union(verweiseZu).Where(v => v != null).ToList();
        }

        private static ArchiveRecordMetadataReference CreateVerweis(Verzeichnungseinheit cmiRecord, Verweis v, string verweisRole)
        {
            var referencedVe = v.VE1.Item.OBJ_GUID == cmiRecord.OBJ_GUID ? v.VE2?.Item : v.VE1?.Item;
            if (referencedVe == null)
                return null;

            return new ArchiveRecordMetadataReference
            {
                ArchiveRecordId = referencedVe.OBJ_GUID,
                ReferenceName = referencedVe.DisplayName,
                Role = verweisRole
            };
        }
    }
}
