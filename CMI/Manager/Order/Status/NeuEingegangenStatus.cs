using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;
using CMI.Contract.Order;
using CMI.Engine.MailTemplate;
using CMI.Manager.Order.Mails;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMI.Manager.Order.Status
{
    public class NeuEingegangenStatus : AuftragStatus
    {
        private static readonly Lazy<NeuEingegangenStatus> lazy =
            new Lazy<NeuEingegangenStatus>(() => new NeuEingegangenStatus());

        private NeuEingegangenStatus()
        {
        }

        public static NeuEingegangenStatus Instance => lazy.Value;

        public override OrderStatesInternal OrderStateInternal => OrderStatesInternal.NeuEingegangen;

        public override void OnStateEnter()
        {
            switch (Context.Ordering.Type)
            {
                case OrderType.Einsichtsgesuch:
                    Context.SetNewStatus(AuftragStatusRepo.EinsichtsgesuchPruefen, Users.System);
                    Context.SetApproveStatus(ApproveStatus.NichtGeprueft, Users.System);
                    break;

                case OrderType.Digitalisierungsauftrag:
                    InitializeDigitalisierungsKategorie();
                    AutomatischOderManuellPruefenSetzen(AuftragStatusRepo.FuerDigitalisierungBereit);
                    break;

                case OrderType.Lesesaalausleihen:
                    AutomatischOderManuellPruefenSetzen(AuftragStatusRepo.FuerAushebungBereit);
                    break;

                case OrderType.Verwaltungsausleihe:
                    AutomatischOderManuellPruefungSetzenVerwaltungsausleihe();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AutomatischOderManuellPruefungSetzenVerwaltungsausleihe()
        {
            // Bei Verwaltungsausleihen soll das System nur dann eine automatische Freigabe erteilen, wenn der 
            // Benutzer ein passendes AS_XXX Token hat. Alle anderen Token werden nicht beachtet!
            if (Context.Besteller.Access.RolePublicClient == AccessRoles.RoleAS && !string.IsNullOrWhiteSpace(Context.OrderItem.VeId))
            {
                var veRecord = Context.IndexAccess.FindDocument(Context.OrderItem.VeId, false);
                if (veRecord != null)
                {
                    if (Context.Besteller.Access.HasAsTokenFor(veRecord.PrimaryDataDownloadAccessTokens)) // nur AS_XXX Tokens sind hier gültig
                    {
                        Context.SetNewStatus(AuftragStatusRepo.FuerAushebungBereit, Users.System);
                        Context.SetApproveStatus(ApproveStatus.FreigegebenDurchSystem, Users.System);
                        return;
                    }
                }
            }

            // Verwaltungsausleihen von AMA-Benutzer müssen immer den Status "Freigabe Prüfen" haben 
            Context.SetNewStatus(AuftragStatusRepo.FreigabePruefen, Users.System);
            Context.SetApproveStatus(ApproveStatus.NichtGeprueft, Users.System);
            AddOrderItemToNeuerAuftragEMail();
        }

        private void AutomatischOderManuellPruefenSetzen(AuftragStatus zielStatusWennAutomatischeFreigabeMoeglich)
        {
            // https://devblogs.microsoft.com/pfxteam/should-i-expose-synchronous-wrappers-for-asynchronous-methods/
            // ToDo: await correctly, when state-machine is async
            
            var kannAutomatischFreigeben = KannAutomatischFreigeben(Context.OrderItem, Context.Besteller)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            if (kannAutomatischFreigeben)
            {
                Context.SetNewStatus(zielStatusWennAutomatischeFreigabeMoeglich, Users.System);
                Context.SetApproveStatus(ApproveStatus.FreigegebenDurchSystem, Users.System);
            }
            else
            {
                Context.SetNewStatus(AuftragStatusRepo.FreigabePruefen, Users.System);
                Context.SetApproveStatus(ApproveStatus.NichtGeprueft, Users.System);
            }

            AddOrderItemToNeuerAuftragEMail();
        }

        /// <summary>
        /// Gleiches Verhalten wie bei AddOrderItemToNeueEinsichtsgesucheEMail
        /// </summary>
        private void AddOrderItemToNeuerAuftragEMail()
        {
            dynamic emailExpando = Context.MailPortfolio.GetUnfinishedMailData<NeuerAuftrag>("NeuerAuftrag");

            if (emailExpando == null)
            {
                // Das EMail mit seine Grunddaten erstellen:
                emailExpando = new DataBuilder(Context.Bus)
                    .AddUser(Context.Ordering.UserId)
                    .AddBestellung(Context.Ordering)
                    .AddVeList(new List<string>())
                    .AddValue("ArtDerArbeit", null)
                    .AddValue("Anzahl", 0)
                    .AddValue("HasOrganization", !string.IsNullOrEmpty(Context.CurrentUser.Organization))
                    .Create();

                Context.MailPortfolio.BeginUnfinishedMail<NeuerAuftrag>("NeuerAuftrag", emailExpando);
            }

            int count = emailExpando.Anzahl;
            emailExpando.Anzahl = count + 1;
            if (Context.Ordering.ArtDerArbeit != null)
            {
                List<int> list = new List<int> { (int)Context.Ordering.ArtDerArbeit };
                emailExpando.ArtDerArbeit = new Stammdaten(list, "ArtDerArbeit");
            }

            // die Ve zum EMail hinzufügen:
            if (Context.OrderItem.VeId == null)
            {
                // Was, wenn es sich um eine Formularbestellung handelt?
            }
            else
            {
                var veRecord = Context.IndexAccess.FindDocument(Context.OrderItem.VeId, false);
                ((List<InElasticIndexierteVe>)emailExpando.VeList).Add(InElasticIndexierteVe.FromElasticArchiveRecord(veRecord));
            }
        }

        /// <summary>
        ///     Setzt das Feld DigitalisierungsKategorie falls es noch leer ist
        /// </summary>
        private void InitializeDigitalisierungsKategorie()
        {
            var orderItem = Context.OrderItem;

            if (orderItem.DigitalisierungsKategorie != DigitalisierungsKategorie.Keine)
            {
                return;
            }

            switch (Context.Besteller.Access.RolePublicClient.GetRolePublicClientEnum())
            {
                case AccessRolesEnum.Ö2:
                    orderItem.DigitalisierungsKategorie = DigitalisierungsKategorie.Oeffentlichkeit;
                    break;

                case AccessRolesEnum.Ö3:
                    orderItem.DigitalisierungsKategorie =
                        Context.Besteller.ResearcherGroup
                            ? DigitalisierungsKategorie.Forschungsgruppe
                            : DigitalisierungsKategorie.Oeffentlichkeit;
                    break;

                case AccessRolesEnum.EMA:
                case AccessRolesEnum.AS:
                    orderItem.DigitalisierungsKategorie = DigitalisierungsKategorie.Amt;
                    break;

                case AccessRolesEnum.AMA:
                    orderItem.DigitalisierungsKategorie = DigitalisierungsKategorie.Intern;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(null,
                        "Angegeben Rolle wurde für die 'InitializeDigitalisierungsKategorie' nicht definiert");
            }
        }

        private async Task<bool> KannAutomatischFreigeben(OrderItem currentOrderItem, User besteller)
        {
            // Nur Bestellungen die mit einer VE in der Datenbank verknüpft sind, könn(t)en automatsich 
            // freigegeben werden.
            if (string.IsNullOrWhiteSpace(currentOrderItem.VeId))
            {
                return false;
            }

            // Prüfen ob gültiger Record von Elasic geliefert wurde.
            var veRecord = Context.IndexAccess.FindDocument(currentOrderItem.VeId, false);
            if (veRecord == null || veRecord.ArchiveRecordId != currentOrderItem.VeId)
            {
                return false;
            }

            if (besteller.Access.HasNonIndividualTokenFor(veRecord.PrimaryDataDownloadAccessTokens))
            {
                return true;
            }

            var indivTokens = await Context.OrderDataAccess.GetIndividualAccessTokens(currentOrderItem.VeId, currentOrderItem.Id);
            return besteller.Access.HasAnyTokenFor(indivTokens.PrimaryDataDownloadAccessTokens);
        }
    }
}