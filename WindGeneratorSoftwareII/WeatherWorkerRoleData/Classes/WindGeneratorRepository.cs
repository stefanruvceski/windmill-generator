﻿///////////////////////////////////////////////////////////
//  WeatherRepository.cs
//  Implementation of the Class WeatherRepository
//  Generated by Enterprise Architect
//  Created on:      16-maj-2018 10.31.48
//  Original author: Stefan
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;
using System.Linq;
using WeatherCommon.Classes;

namespace WeatherWorkerRoleData.Classes
{
    public class WindGeneratorRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public WindGeneratorRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("WindGeneratorDataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri),
                                                                _storageAccount.Credentials);
            _table = tableClient.GetTableReference("WindGeneratorTable");

            if (_table.CreateIfNotExists())
            {
                InitWindGenerators();
            }
        }

        private void InitWindGenerators()
        {
            TableBatchOperation batchOperation = new TableBatchOperation();

            WindGeneratorBase w1 = new WindGeneratorBase("Novi Sad", "3", 20, "3");
            WindGeneratorBase w2 = new WindGeneratorBase("Subotica", "4", 17, "4");
            WindGeneratorBase w3 = new WindGeneratorBase("Sombor", "8", 15, "3");
            WindGeneratorBase w4 = new WindGeneratorBase("Kikinda", "5", 14, "5");
            WindGeneratorBase w5 = new WindGeneratorBase("Zrenjanin", "2", 12, "2");
            WindGeneratorBase w6 = new WindGeneratorBase("Vrsac", "7", 20, "5");
            WindGeneratorBase w7 = new WindGeneratorBase("Sremska Mitrovica", "1", 8, "1");
            WindGeneratorBase w8 = new WindGeneratorBase("Pancevo", "6", 9, "1");

            batchOperation.InsertOrReplace(w1);
            batchOperation.InsertOrReplace(w2);
            batchOperation.InsertOrReplace(w3);
            batchOperation.InsertOrReplace(w4);
            batchOperation.InsertOrReplace(w5);
            batchOperation.InsertOrReplace(w6);
            batchOperation.InsertOrReplace(w7);
            batchOperation.InsertOrReplace(w8);

            _table.ExecuteBatch(batchOperation);
        }


        public void AddOrReplaceWindGenerator(WindGeneratorBase windGenerator)
        {
            TableOperation add = TableOperation.InsertOrReplace(windGenerator);
            _table.Execute(add);

        }

        public List<WindGeneratorBase> GetAllWindGenerators()
        {
            IQueryable<WindGeneratorBase> requests = from g in _table.CreateQuery<WindGeneratorBase>()
                                                     where g.PartitionKey == "WindGenerator"
                                                     select g;
            return requests.ToList();
        }

        public WindGeneratorBase GetOneWindGenerator(string city)
        {
            IQueryable<WindGeneratorBase> requests = from g in _table.CreateQuery<WindGeneratorBase>()
                                                     where g.PartitionKey == "WindGenerator" && g.RowKey == city
                                                     select g;

            return requests.ToList()[0];
        }

    }//end WeatherRepository

}//end namespace WeatherWorkerRoleData