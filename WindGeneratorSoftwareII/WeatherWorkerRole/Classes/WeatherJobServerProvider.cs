///////////////////////////////////////////////////////////
//  WeatherJobServerProvider.cs
//  Implementation of the Class WeatherJobServerProvider
//  Generated by Enterprise Architect
//  Created on:      16-maj-2018 10.31.24
//  Original author: Stefan
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WeatherCommon.Classes;
using System.Diagnostics;
using WeatherWorkerRoleData.Classes;
using System.Threading.Tasks;

namespace WeatherWorkerRole.Classes
{
    public class WeatherJobServerProvider : IWeather
    {
      

        public WeatherJobServerProvider()
        {

        }
        public void SendWeatherState(Weather weather)
        {
            if (Repositories.weatherRepository.GetAllWeathersByCity(weather.City).Count != 0)
            {
                if (!Repositories.weatherRepository.GetOneWeather(weather.City).WindSpeed.Equals(weather.WindSpeed))
                    Repositories.weatherRepository.AddOrReplaceWeather(new WeatherBase(weather));
            }
            else
            {
                Repositories.weatherRepository.AddWeather(new WeatherBase(weather));
            }
        }

        public WindGenerator GetWindGenerator(string city)
        {
           
            return GetWindGeneratorFromDataBase(city);
        }

        private WindGenerator GetWindGeneratorFromDataBase(string city)
        {
            WeatherBase weatherBase = null;
            WindGeneratorBase windGeneratorBase = Task<WindGeneratorBase>.Factory.StartNew(()=>  Repositories.windGeneratorRepository.GetOneWindGenerator(city)).Result;

            WindMillBase windMillBase = Task<WindMillBase>.Factory.StartNew(() => Repositories.windMillRepository.GetOneWindMill(windGeneratorBase.WindMill)).Result;
            AggregateBase aggregateBase = Task<AggregateBase>.Factory.StartNew(() => Repositories.aggregateRepository.GetOneAggregate(windGeneratorBase.Aggregate)).Result;
            weatherBase = Task<WeatherBase>.Factory.StartNew(() => Repositories.weatherRepository.GetLastWeather(windGeneratorBase.Weather)).Result;


            if ((windGeneratorBase.Power = windGeneratorBase.CalculatePower()) < windMillBase.MinPower)
            {
                windGeneratorBase.AggregateONCnt++;
                aggregateBase.State = true;
               
            }
            else
            {
                aggregateBase.State = false;
            }

            // racunanje cene dosadasnjeg rada agregata
            windGeneratorBase.TotalAggregateCost = windGeneratorBase.CalculateTotalAggregateCost(120);

            Repositories.windGeneratorRepository.AddOrReplaceWindGenerator(windGeneratorBase);
            Repositories.aggregateRepository.AddOrReplaceAggregate(aggregateBase);


            Aggregate aggregate = new Aggregate(int.Parse(aggregateBase.RowKey), aggregateBase.CostPerHour, aggregateBase.Power, aggregateBase.State);
            WindMill windMill = new WindMill(windMillBase.Coefficient, windMillBase.MinPower, windMillBase.TurbineDiameter, windMillBase.MaxSpeed, windMillBase.MaxSpeedTime,windMillBase.WorkingTime);

            if (weatherBase == null)
                weatherBase = Task<WeatherBase>.Factory.StartNew(() => Repositories.weatherRepository.GetOneWeather(windGeneratorBase.Weather)).Result;

            Weather weather = new Weather(weatherBase.City, weatherBase.Description, weatherBase.MaxTemp, weatherBase.MinTemp, weatherBase.Pressure, weatherBase.WindSpeed);
            
            return new WindGenerator(weather, windMill, windGeneratorBase.WindMillCnt, aggregate,windGeneratorBase.AggregateONCnt,windGeneratorBase.Power);
        }

    }//end WeatherJobServerProvider

}//end namespace WeatherWorkerRole