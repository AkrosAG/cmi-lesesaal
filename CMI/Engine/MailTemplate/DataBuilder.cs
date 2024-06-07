using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Contract.Order;
using MassTransit;
using Serilog;

namespace CMI.Engine.MailTemplate
{
    public class DataBuilder : IDataBuilder
    {
        private readonly IBus bus;
        private dynamic expando;

        // Required constructor for dependency injection
        public DataBuilder(IBus bus) : this(bus, new ExpandoObject())
        {
        }

        public DataBuilder(IBus bus, dynamic expando)
        {
            this.bus = bus;
            Stammdaten.Bus = bus;
            this.expando = expando;
            this.expando.Global = new Global();
        }

        public IDataBuilder AddUser(string userId)
        {
            expando.User = GetPerson(userId);
            return this;
        }

        public IDataBuilder AddBesteller(string bestellerId)
        {
            expando.Besteller = GetPerson(bestellerId);
            return this;
        }

        public IDataBuilder AddBestellung(Ordering ordering)
        {
            expando.Bestellung = new Bestellung(ordering);
            return this;
        }

        public IDataBuilder AddVe(string archiveRecordId)
        {
            expando.Ve = GetVe(archiveRecordId);
            return this;
        }

        public IDataBuilder AddVeList(IEnumerable<string> archiveRecordIdList)
        {
            var veList = new List<InElasticIndexierteVe>();
            foreach (var archiveRecordId in archiveRecordIdList)
            {
                veList.Add(GetVe(archiveRecordId));
            }

            AddVeList(veList);
            return this;
        }

        public IDataBuilder AddVeList(List<InElasticIndexierteVe> veList)
        {
            expando.VeList = veList;
            expando.HatMehrereVe = veList.Count > 1;
            return this;
        }

        /// <param name="sprachCode">Z.B. de</param>
        public IDataBuilder AddSprache(string sprachCode)
        {
            expando.Sprachen = new[] { new Sprache(sprachCode) };
            return this;
        }

        public IDataBuilder AddAuftraege(IEnumerable<int> orderItemIds)
        {
            var neueAuftraege = GetAuftraege(orderItemIds);

            if (!((IDictionary<string, object>)expando).ContainsKey("Aufträge"))
            {
                expando.Aufträge = new List<Auftrag>();
            }

            var auftragsliste = (List<Auftrag>)expando.Aufträge;
            auftragsliste.AddRange(neueAuftraege);


            return this;
        }

        public IDataBuilder AddAuftraege(Ordering ordering, IEnumerable<OrderItem> items, string propertyName)
        {
            var auftraege = new List<Auftrag>();
            foreach (var orderItem in items)
            {
                auftraege.Add(GetAuftrag(ordering, orderItem));
            }

            AddValue(propertyName, auftraege);
            return this;
        }

        public IDataBuilder AddAuftrag(Ordering ordering, OrderItem item)
        {
            AddValue("Auftrag", GetAuftrag(ordering, item));
            return this;
        }

        public IDataBuilder AddValue(string propertyName, object value)
        {
            ((IDictionary<string, object>)(ExpandoObject)expando)[propertyName] = value;
            return this;
        }

        public IDataBuilder AddBestellerMitAuftraegen(int[] orderItemIds)
        {
            var auftraege = GetAuftraege(orderItemIds);
            var gruppenNachBestellerId = auftraege.GroupBy(auftrag => auftrag.Bestellung.Besteller.Id, auftrag => auftrag);

            var alleBestellerMitAuftraegen = new ListWithFlags<BestellerMitAuftraegen>();

            foreach (var gruppe in gruppenNachBestellerId)
            {
                var besteller = auftraege.First(a => a.Bestellung.Besteller.Id == gruppe.Key).Bestellung.Besteller;
                alleBestellerMitAuftraegen.Add(new BestellerMitAuftraegen(besteller, gruppe.GetEnumerator()));
            }

            expando.BestellerMitAufträgen = alleBestellerMitAuftraegen;

            return this;
        }


        public List<Auftrag> GetAuftraege(IEnumerable<int> orderItemIds)
        {
            var requestClient =
                CreateRequestClient<FindOrderItemsRequest>(bus, BusConstants.OrderManagerFindOrderItemsRequestQueue);
            var task = requestClient.GetResponse<FindOrderItemsResponse>(new FindOrderItemsRequest { OrderItemIds = orderItemIds.ToArray() });
            task.Wait();
            var response = task.Result.Message;
            var auftraege = new List<Auftrag>();

            foreach (var orderItem in response.OrderItems)
            {
                var ordering = GetOrdering(orderItem.OrderId);

                auftraege.Add(string.IsNullOrWhiteSpace(orderItem.VeId)
                    ? GetAuftragFormularbestellung(ordering, orderItem)
                    : GetAuftragForOrderItemWithVeId(ordering, orderItem));
            }

            return auftraege;
        }

        public void Reset()
        {
            expando = new ExpandoObject();
        }

        public dynamic Create()
        {
            return expando;
        }

        private Person GetPerson(string userId)
        {
            var requestClient =
                CreateRequestClient<ReadUserInformationRequest>(bus, BusConstants.ReadUserInformationQueue);
            var task = requestClient.GetResponse<ReadUserInformationResponse>(new ReadUserInformationRequest { UserId = userId });
            task.Wait();
            var response = task.Result.Message;
            return Person.FromUser(response?.User);
        }


        private InElasticIndexierteVe GetVe(string archiveRecordId)
        {
            return InElasticIndexierteVe.FromElasticArchiveRecord(GetElasticArchiveRecord(archiveRecordId));
        }

        private ElasticArchiveRecord GetElasticArchiveRecord(string archiveRecordId)
        {
            ElasticArchiveRecord retVal;
            var retryCount = 0;
            var success = false;

            try
            {
                // Bei Fehlerfall warten wir ab retryCount > 0
                // RetryCount = 0   -->    0 ms
                // RetryCount = 1   --> 2000 ms
                // RetryCount = 2   --> 8000 ms
                Thread.Sleep(1000 * ((3 ^ retryCount) - 1));

                var requestClient =
                    CreateRequestClient<FindArchiveRecordRequest>(bus, BusConstants.IndexManagerFindArchiveRecordMessageQueue);
                var task = requestClient.GetResponse<FindArchiveRecordResponse>(new FindArchiveRecordRequest { ArchiveRecordId = archiveRecordId });
                task.Wait();

                retVal = task.Result.Message.ElasticArchiveRecord ?? new ElasticArchiveRecord
                {
                    ArchiveRecordId = archiveRecordId,
                    Title = "Record not found in Elastic",
                    CreationPeriod = new ElasticTimePeriod
                    {
                        StartDate = DateTime.MaxValue,
                        EndDate = DateTime.MaxValue,
                        Text = DateTime.MaxValue.ToShortDateString(),
                        StartDateText = DateTime.MaxValue.ToShortDateString(),
                        EndDateText = DateTime.MaxValue.ToShortDateString(),
                        Years = new List<int> { 9999 }
                    }
                };
                if (retVal.ArchiveplanContext == null || retVal.ArchiveplanContext.Count == 0)
                {
                    retVal.ArchiveplanContext = new List<ElasticArchiveplanContextItem>
                    {
                        new()
                        {
                            ArchiveRecordId = "999999999",
                            Level = "Dossier"
                        }
                    };
                }
            }
            catch (Exception e)
            {
                Log.Error(e,
                    "Es gab ein Problem beim Zusammenbauen von einem Record mit der id {archiveRecordId},es wird ein default Record zurückgegeben. Fehler: {message}",
                    archiveRecordId, e.Message);
                retVal = new ElasticArchiveRecord
                {
                    ArchiveRecordId = archiveRecordId,
                    Title = "Error while fetching record from Elastic",
                    CreationPeriod = new ElasticTimePeriod
                    {
                        StartDate = DateTime.MaxValue,
                        EndDate = DateTime.MaxValue,
                        Text = DateTime.MaxValue.ToShortDateString(),
                        StartDateText = DateTime.MaxValue.ToShortDateString(),
                        EndDateText = DateTime.MaxValue.ToShortDateString(),
                        Years = new List<int> { 9999 }
                    }
                };
                if (retVal.ArchiveplanContext == null || retVal.ArchiveplanContext.Count == 0)
                {
                    retVal.ArchiveplanContext = new List<ElasticArchiveplanContextItem>
                    {
                        new()
                        {
                            ArchiveRecordId = "999999999",
                            Level = "Dossier"
                        }
                    };
                }

                retryCount++;
            }
            
            return retVal;
        }


        private Auftrag GetAuftrag(Ordering ordering, OrderItem orderItem)
        {
            return string.IsNullOrWhiteSpace(orderItem.VeId)
                ? GetAuftragFormularbestellung(ordering, orderItem)
                : GetAuftragForOrderItemWithVeId(ordering, orderItem);
        }

        private Auftrag GetAuftragFormularbestellung(Ordering ordering, OrderItem orderItem)
        {
            var x = new BestellformularVe(orderItem);
            var besteller = GetPerson(ordering.UserId);
            var auftrag = new Auftrag(orderItem,
                ordering,
                x,
                x,
                besteller);
            return auftrag;
        }

        private Auftrag GetAuftragForOrderItemWithVeId(Ordering ordering, OrderItem orderItem)
        {
            var bestellterRecord = GetElasticArchiveRecord(orderItem.VeId.ToString());
            ElasticArchiveRecord auszuhebenderRecord = null;
            var besteller = GetPerson(ordering.UserId);

            if (ordering.Type == OrderType.Digitalisierungsauftrag)
            {
                var dossierId = bestellterRecord.GetAuszuhebendeArchiveRecordId();
                if (dossierId != null)
                {
                    auszuhebenderRecord = GetElasticArchiveRecord(dossierId);
                }
            }
            else
            {
                auszuhebenderRecord = bestellterRecord;
            }

            var auftrag = new Auftrag(orderItem,
                ordering,
                InElasticIndexierteVe.FromElasticArchiveRecord(bestellterRecord),
                InElasticIndexierteVe.FromElasticArchiveRecord(auszuhebenderRecord),
                besteller);
            return auftrag;
        }


        public static IRequestClient<T1> CreateRequestClient<T1>(IBus busControl, string relativeUri) where T1 : class
        {
            var client = busControl.CreateRequestClient<T1>(new Uri(busControl.Address, relativeUri), TimeSpan.FromSeconds(10));
            return client;
        }

        private Ordering GetOrdering(int orderingId)
        {
            var client = CreateRequestClient<GetOrderingRequest>(bus, BusConstants.OrderManagerGetOrderingRequestQueue);
            var result = client.GetResponse<GetOrderingResponse>(new GetOrderingRequest { OrderingId = orderingId }).GetAwaiter().GetResult().Message;
            return result.Ordering;
        }
    }
}