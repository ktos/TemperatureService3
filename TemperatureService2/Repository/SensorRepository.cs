﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TemperatureService2.Data;
using TemperatureService2.Models;

namespace TemperatureService2.Repository
{
    public class SensorRepository : ISensorRepository
    {
        private readonly SensorsDbContext _context;

        public SensorRepository(SensorsDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Sensor> GetAllSensorsWithValues()
        {
            return _context.Sensors.Include(x => x.Values).ToList();
        }

        public IEnumerable<Sensor> GetAllSensors()
        {
            return _context.Sensors.ToList();
        }

        public Sensor GetSensor(string name)
        {
            return _context.Sensors.Include(x => x.Values).FirstOrDefault(x => x.Name == name);
        }

        public bool UpdateSensor(SensorDto sensorDto)
        {
            var sensor = GetSensor(sensorDto.Name);
            if (sensorDto.Description != sensor.Description) sensor.Description = sensorDto.Description;
            if (sensorDto.Id != sensor.InternalId) sensor.InternalId = sensorDto.Id;
            if (sensorDto.Type != sensor.Type) sensor.Type = sensorDto.Type;
            if (sensorDto.Data != -127 && !AddSensorReading(sensorDto))
            {
                return false;
            }

            _context.SaveChanges();
            return true;
        }

        public bool AddSensor(SensorDto sensorDto)
        {
            var sensor = new Sensor
            {
                Description = sensorDto.Description,
                InternalId = sensorDto.Id,
                Name = sensorDto.Name,
                Type = sensorDto.Type
            };

            var created = _context.Sensors.Add(sensor);
            if (sensorDto.Data != -127 && !AddSensorReading(sensorDto))
            {
                return false;
            }

            return true;
        }

        public bool AddSensorReading(SensorDto sensorDto)
        {
            _context.SensorValues.Add(new SensorValue
            {
                Data = sensorDto.Data,
                Sensor = GetSensor(sensorDto.Name),
                Timestamp = DateTime.UtcNow
            });

            _context.SaveChanges();
            return true;
        }
    }
}