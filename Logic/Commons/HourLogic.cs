using System;
using System.Collections.Generic;
using SharpArch.Domain.PersistenceSupport;
using Logic.Dtos;
using Logic.Contracts;
using Domain;
using Domain.RepositoryInterfaces;

namespace Logic.Commons
{
    public class HourLogic : IHourLogic
    {
        private readonly IHourRepository hourRepository;
        private readonly IRepository<Camp> campRepository;

        public HourLogic(IHourRepository hourRepository, IRepository<Camp> campRepository)
        {
            this.hourRepository = hourRepository;
            this.campRepository = campRepository;
        }

        public HourDto Create(TimeSpan time, int dayOfWeek, bool isEnabled, int campId)
        {
            try
            {
                Helper.TryParseEnum<DayOfWeek>(dayOfWeek - 1, "Día de la semana inválido");
                Camp camp = campRepository.Get(campId);
                Helper.ThrowIfNull(camp, "Datos de cancha inválida.");
                Helper.ThrowIf(!camp.IsEnabled, "Cancha no disponible por el momento.");
                Helper.ThrowIf(hourRepository.Exists(time, dayOfWeek, campId), "Ya existe el horario para el día de la semana indicado.");

                Hour hour = new Hour();
                hour.Time = time;
                hour.DayOfWeek = dayOfWeek;
                hour.IsEnabled = isEnabled;
                hour.Camp = camp;

                hourRepository.Save(hour);

                return new HourDto(hour.Id, hour.Time, hour.DayOfWeek, hour.IsEnabled, campId);
            }
            catch
            {
                throw;
            }
        }

        public void EnableDisable(int hourId, bool isEnabled)
        {
            try
            {
                hourRepository.TransactionManager.BeginTransaction();
                Hour hour = hourRepository.Get(hourId);
                Helper.ThrowIfNull(hour, "El horario no existe.");
                Helper.ThrowIf(hour.IsEnabled == isEnabled, "El horario ya se encuentra " + (isEnabled ? "habilitado" : "deshabilitado"));
                hour.IsEnabled = isEnabled;
                hourRepository.SaveOrUpdate(hour);
                hourRepository.TransactionManager.CommitTransaction();
            }
            catch
            {
                hourRepository.TransactionManager.RollbackTransaction();
                throw;
            }
        }

        public IList<HourDto> List(int campId)
        {
            IList<HourDto> results = new List<HourDto>();
            try
            {
                var hours = hourRepository.ToList(campId);
                foreach (Hour hour in hours)
                {
                    results.Add(new HourDto(hour.Id, hour.Time, hour.DayOfWeek, hour.IsEnabled, campId));
                }
            }
            catch
            {
                throw;
            }
            return results;
        }
    }
}
