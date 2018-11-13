﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using TemperatureService2.Models;

namespace TemperatureService2.Test
{
    public class SensorViewModel
    {
        [Fact]
        public void SensorViewModel_StatusFalseWhenEmptyValues()
        {
            var sensor = new Sensor {
                Name = "outdoor",
                Description = "zewnątrz",
                InternalId = "1",
                Type = SensorType.Temperature,
                Values = null
            };

            var svm = new ViewModels.SensorViewModel(sensor);
            Assert.False(svm.Status);
        }

        [Fact]
        public void SensorViewModel_StatusFalseWhenNoValues()
        {
            var sensor = new Sensor
            {
                Name = "outdoor",
                Description = "zewnątrz",
                InternalId = "1",
                Type = SensorType.Temperature,
                Values = new List<SensorValue> {
                    
                }
            };

            var svm = new ViewModels.SensorViewModel(sensor);
            Assert.False(svm.Status);
        }

        [Fact]
        public void SensorViewModel_PositiveData()
        {
            var now = DateTime.UtcNow;

            var sensor = new Sensor
            {
                Name = "outdoor",
                Description = "zewnątrz",
                InternalId = "1",
                Type = SensorType.Temperature,
                Values = new List<SensorValue>
                {
                    new SensorValue {  Data = 1, Id = 1, Timestamp = now },
                    new SensorValue {  Data = 2, Id = 2, Timestamp = now - TimeSpan.FromMinutes(15) },
                    new SensorValue {  Data = 3, Id = 3, Timestamp = now - TimeSpan.FromMinutes(30) }
                }
            };

            var svm = new ViewModels.SensorViewModel(sensor);
            Assert.True(svm.Status);
            Assert.Equal("outdoor", svm.Name);
            Assert.Equal("zewnątrz", svm.Description);
            Assert.Equal("1", svm.Id);
            Assert.Equal(SensorType.Temperature, svm.Type);
            Assert.Equal(1, svm.Data);
            Assert.Equal(now, svm.LastUpdated);
        }

        [Fact]
        public void SensorViewModel_StatusFalseWhenTooOld()
        {
            var now = DateTime.UtcNow;

            var sensor = new Sensor
            {
                Name = "outdoor",
                Description = "zewnątrz",
                InternalId = "1",
                Type = SensorType.Temperature,
                Values = new List<SensorValue>
                {
                    new SensorValue {  Data = 1, Id = 1, Timestamp = now - TimeSpan.FromMinutes(60) },
                    new SensorValue {  Data = 2, Id = 2, Timestamp = now - TimeSpan.FromMinutes(75) },
                    new SensorValue {  Data = 3, Id = 3, Timestamp = now - TimeSpan.FromMinutes(90) }
                }
            };

            var svm = new ViewModels.SensorViewModel(sensor);
            Assert.False(svm.Status);
            Assert.Equal("outdoor", svm.Name);
            Assert.Equal("zewnątrz", svm.Description);
            Assert.Equal("1", svm.Id);
            Assert.Equal(SensorType.Temperature, svm.Type);
            Assert.Equal(1, svm.Data);
            Assert.Equal(now - TimeSpan.FromMinutes(60), svm.LastUpdated);
        }
    }
}
