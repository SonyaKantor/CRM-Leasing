
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace LeasingJobs
{
    class VehicleList
    {

        IOrganizationService service;
        public OrganizationServiceContext svcContext = null;
        EntityCollection ec = new EntityCollection();
        public int indx = 0, updated = 0, failed = 0;
        public VehicleList(IOrganizationService m_service)
        {
            service = m_service;
            svcContext = new OrganizationServiceContext(service);
        }

        public async Task Run()
        {
            try
            {
                VehicleDataModel vehicle = new VehicleDataModel();
                List<Entity> newVehicles = new List<Entity>();
                Dictionary<int, Guid> manufacturers = GetAllManufacturers();
                HttpClient httpClient = new HttpClient();
                UriBuilder uriBuilder = new UriBuilder("https://data.gov.il/api/3/action/datastore_search");
                uriBuilder.Query = "resource_id=142afde2-6228-49f9-8a29-9b6c3a0cbe40&q={\"shnat_yitzur\":%20\"2023\"}&limit=10000";;
                // _id, tozar, tozeret_eretz_nm , tozeret_nm, tozeret_cd, degem_nm, degem_cd,
                HttpResponseMessage response = await httpClient.GetAsync(uriBuilder.Uri);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var matches = Regex.Matches(responseBody, "2023");
                    int startOfRecords = responseBody.IndexOf("\"records\": [") + "\"records\": [".Length+1;
                    int endOfRecords =  responseBody.IndexOf("], \"fields\":");
                    string listOfRecords = responseBody.Substring(startOfRecords,endOfRecords-startOfRecords );
                    int maxModelAdded = GetLastModel();
                    Array res = listOfRecords.Split(new char[] { '{', });
                    foreach (string arr in res)
                    {
                        Console.WriteLine("String: " + arr + "\n");
                        var listOfAttributes = arr.Split(new char[] { ',', });

                        foreach (string attr in listOfAttributes)
                        {
                            Tuple<string, string> splitted = GetTagValue(attr);
                            switch (splitted.Item1)
                            {
                                case "_id":
                                    if(int.Parse(splitted.Item2)>maxModelAdded)
                                        vehicle.TableId = int.Parse(splitted.Item2);
                                    break;
                                case "tozeret_nm":
                                    vehicle.ProductName = splitted.Item2;
                                    break;
                                case "tozeret_cd":
                                    vehicle.ProductCode = int.Parse(splitted.Item2);
                                    break;
                                case "degem_nm":
                                    vehicle.ModelName = splitted.Item2;
                                    break;
                                case "degem_cd":
                                    vehicle.ModelCode = int.Parse(splitted.Item2);
                                    break;
                                case "tozeret_eretz_nm":
                                    vehicle.Country = splitted.Item2;
                                    break;


                            }
                            
                        }
                        if (vehicle.TableId == 0)
                        {
                            vehicle.ClearData();
                            continue;
                        }
                            
                        manufacturers= AddNewModel(vehicle, manufacturers);
                        Console.WriteLine(vehicle.ToString() + " added");
                        vehicle.ClearData();
                    }
                }
                else
                {
                    Console.WriteLine("Error: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

          
        }
        private Dictionary<int, Guid> AddNewModel(VehicleDataModel vehicle, Dictionary<int,Guid> manufacturer)
        {
            Entity model = new Entity("son_model");
            Guid manufacturerId = manufacturer.ContainsKey(vehicle.ProductCode) ? manufacturer[vehicle.ProductCode] : Guid.Empty;
            if(manufacturerId == Guid.Empty)
            {
                manufacturerId = CreateNewManudacturer(vehicle);
                manufacturer[vehicle.ProductCode] = manufacturerId;
            }
            model["son__id"] = vehicle.TableId;
            model["son_modelcode"] = vehicle.ModelCode.ToString();
            model["son_modelname"] = vehicle.ModelName;
            model["son_manufacturer"] = new EntityReference("son_manufacturer", manufacturerId);
            model["son_name"] = vehicle.ModelName + " " + vehicle.ModelCode + " " + vehicle.ProductName;
            service.Create(model);
            return manufacturer;
        }
        private Guid CreateNewManudacturer(VehicleDataModel vehicle)
        {
            Entity manufacturer = new Entity("son_manufacturer");
            manufacturer["son_manufacturercode"] = vehicle.ProductCode.ToString();
            manufacturer["son_manufacturername"] = vehicle.ProductName;
            manufacturer["son_country"] = vehicle.Country;
            manufacturer["son_name"] = vehicle.ProductName + " " + vehicle.ProductCode + " " + vehicle.Country;
            return service.Create(manufacturer);
        }
        private Tuple<string,string> GetTagValue(string line)
        {
            var splittedTag = line.Split(new char[] {':'});
            if(splittedTag.Length == 2)
            {
                string left = splittedTag[0].Replace("\"", "");
                string right = splittedTag[1].Replace("\"", "");
                return Tuple.Create(splittedTag[0].Replace("\"", ""), splittedTag[1].Replace("\"", ""));
                
            }
            return Tuple.Create("", "");
        }
        // get table id with the biggest number
        private int GetLastModel()
        {
            QueryExpression query = new QueryExpression("son_model");
            query.AddOrder("son__id", OrderType.Descending);
            query.PageInfo = new PagingInfo() { Count = 1, PageNumber = 1 };
            query.ColumnSet = new ColumnSet("son__id");
            var result = service.RetrieveMultiple(query);
            if (result.Entities.Count > 0)
                return (int)result.Entities[0].Attributes["son__id"];
            return 0;
        }
        // get all the manufacturers in the CRM table
        private Dictionary<int, Guid> GetAllManufacturers ()
        {
            Dictionary<int,Guid> Models = new Dictionary<int, Guid>();
            QueryExpression q = new QueryExpression("son_manufacturer");
            q.ColumnSet = new ColumnSet("son_manufacturerid", "son_manufacturercode");
            var result = service.RetrieveMultiple (q);
            if(result.Entities.Count == 0)
                return Models;
            foreach (var model in result.Entities)
            {
                Models[int.Parse((string)model["son_manufacturercode"])] = (Guid)model["son_manufacturerid"];
            }
            return Models;

        }
         
    }
}
