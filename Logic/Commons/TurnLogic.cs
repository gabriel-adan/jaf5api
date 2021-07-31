using System;
using System.Linq;
using System.Collections.Generic;
using Domain;
using Domain.RepositoryInterfaces;
using Logic.Dtos;
using Logic.Contracts;
using SharpArch.Domain.PersistenceSupport;
using Logic.BusinessRules;

namespace Logic.Commons
{
    public class TurnLogic : ITurnLogic
    {
        private readonly IRepository<Hour> hourRepository;
        private readonly IRepository<Team> teamRepository;
        private readonly IRepository<Perfil> perfilRepository;
        private readonly IFieldRepository fieldRepository;
        private readonly ITurnRepository turnRepository;
        private readonly IRepository<Player> playerRepository;

        public TurnLogic(IRepository<Hour> hourRepository, IRepository<Team> teamRepository, IRepository<Perfil> perfilRepository, IFieldRepository fieldRepository, ITurnRepository turnRepository, IRepository<Player> playerRepository)
        {
            this.hourRepository = hourRepository;
            this.teamRepository = teamRepository;
            this.perfilRepository = perfilRepository;
            this.fieldRepository = fieldRepository;
            this.turnRepository = turnRepository;
            this.playerRepository = playerRepository;
        }

        public TurnResultDto Request(DateTime date, int hourId, int teamId, int perfilId)
        {
            try
            {
                DateTime now = Helper.GetDateTimeZone();
                Helper.ThrowIf(date < now, "No se puede solicitar un turno para una fecha pasada.");
                Hour hour = hourRepository.Get(hourId);
                Helper.ThrowIfNull(hour, "Horario inválido.");
                Team team = teamRepository.Get(teamId);
                Helper.ThrowIfNull(team, "Grupo no encontrado.");
                Player requestPlayer = team.Players.Where(p => p.Perfil.Id == perfilId).FirstOrDefault();
                Helper.ThrowIfNull(requestPlayer, "No eres miembro de este grupo.");
                int amoutPlayers = (int)EPlayersOnTeam.MIN_NUMBER;
                Helper.ThrowIf(team.Players.Count < amoutPlayers, string.Format("El grupo {0} no tiene la cantidad de participantes suficientes para invitar, debe tener al menos {1} participantes.", team.Name, amoutPlayers));
                Field field = fieldRepository.FindAvailable(date, hour);
                Helper.ThrowIfNull(field, "No hay canchas disponibles.");

                turnRepository.TransactionManager.BeginTransaction();

                Turn turn = new Turn();
                turn.Date = date;
                turn.FullName = team.Name;
                turn.State = EState.REQUESTED;
                turn.Field = field;
                turn.Hour = hour;
                turn.Team = team;
                turnRepository.Save(turn);
                turnRepository.SaveOrUpdate(turn);
                Camp camp = hour.Camp;
                requestPlayer.ConfirmDate = now;
                playerRepository.SaveOrUpdate(requestPlayer);

                IList<Player> players = team.Players.Where(p => p.Perfil.Id != perfilId).ToList();
                //Notificar a los jugadores
                foreach (Player player in players)
                {
                    if (player.ConfirmDate.HasValue)
                    {
                        player.ConfirmDate = null;
                        playerRepository.SaveOrUpdate(player);
                    }

                    Perfil perfil = player.Perfil;
                    string nofity = string.Format("Hola {0}! El grupo {1} quiere jugar el día {2} a Hs {3} en la cancha {4}, {5} {6}. ¿Te sumas?", perfil.Name, team.Name, turn.Date.ToString("dd/MM/yy"), hour.Time.ToString("HH:mm"), camp.Name, camp.Street, camp.Number);

                }

                turnRepository.TransactionManager.CommitTransaction();

                DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, hour.Time.Hours, hour.Time.Minutes, hour.Time.Seconds);
                return new TurnResultDto(turn.Id, dateTime, turn.FullName, turn.Field.Name);
            }
            catch
            {
                turnRepository.TransactionManager.RollbackTransaction();
                throw;
            }
        }

        public TurnResultDto Reserve(DateTime date, int hourId, string name)
        {
            try
            {
                Helper.ThrowIfIsNullOrEmpty(name, "Debe ingresar el nombre de quien reserva.");
                Helper.ThrowIf(date < Helper.GetDateTimeZone(), "No se puede solicitar un turno para una fecha pasada.");
                Hour hour = hourRepository.Get(hourId);
                Helper.ThrowIfNull(hour, "Horario inválido.");
                Field field = fieldRepository.FindAvailableReserve(date, hour);
                Helper.ThrowIfNull(field, "No hay canchas disponibles.");

                turnRepository.TransactionManager.BeginTransaction();

                Turn turn = new Turn();
                turn.Date = date;
                turn.FullName = name;
                turn.State = EState.RESERVED;
                turn.Field = field;
                turn.Hour = hour;
                turnRepository.Save(turn);

                IList<Turn> requestedTurns = turnRepository.GetRequests(date, hour, field);
                foreach (Turn requestedTurn in requestedTurns)
                {
                    Field requestedField = fieldRepository.FindAvailable(date, hour);
                    if (requestedField != null)
                    {
                        requestedTurn.Field = requestedField;
                        turnRepository.SaveOrUpdate(requestedTurn);
                    }
                    else
                    {
                        //Notificar los turnos que quedan sin canchas
                    }
                }
                turnRepository.TransactionManager.CommitTransaction();

                DateTime dateTime = new DateTime(turn.Date.Year, turn.Date.Month, turn.Date.Day, turn.Hour.Time.Hours, turn.Hour.Time.Minutes, turn.Hour.Time.Seconds);
                return new TurnResultDto(turn.Id, dateTime, turn.FullName, turn.Field.Name);
            }
            catch
            {
                turnRepository.TransactionManager.RollbackTransaction();
                throw;
            }
        }

        public TurnResultDto CreateTeamTurn(DateTime date, int hourId, string name, bool isPrivate, int perfilId)
        {
            try
            {
                Helper.ThrowIfIsNullOrEmpty(name, "Debe ingresar un nombre para el grupo.");
                Perfil perfil = perfilRepository.Get(perfilId);
                Helper.ThrowIfNull(perfil, "Perfil inválido.");
                Hour hour = hourRepository.Get(hourId);
                Helper.ThrowIfNull(hour, "Horario inválido.");
                Field field = fieldRepository.FindAvailable(date, hour);
                Helper.ThrowIfNull(field, "No hay canchas disponibles.");

                teamRepository.TransactionManager.BeginTransaction();

                Team team = new Team();
                team.IsPrivate = isPrivate;
                team.Name = name;
                team.Perfil = perfil;

                teamRepository.Save(team);

                DateTime now = Helper.GetDateTimeZone();
                Player player = new Player();
                player.Perfil = perfil;
                player.Team = team;
                player.RequestDate = now;
                player.ConfirmDate = now;
                team.Players.Add(player);

                teamRepository.SaveOrUpdate(team);

                Turn turn = new Turn();
                turn.Date = date;
                turn.FullName = team.Name;
                turn.State = EState.REQUESTED;
                turn.Field = field;
                turn.Hour = hour;
                turn.Team = team;

                turnRepository.Save(turn);

                turnRepository.SaveOrUpdate(turn);

                teamRepository.TransactionManager.CommitTransaction();

                DateTime dateTime = new DateTime(turn.Date.Year, turn.Date.Month, turn.Date.Day, turn.Hour.Time.Hours, turn.Hour.Time.Minutes, turn.Hour.Time.Seconds);
                return new TurnResultDto(turn.Id, dateTime, turn.FullName, turn.Field.Name);
            }
            catch
            {
                teamRepository.TransactionManager.RollbackTransaction();
                throw;
            }
        }

        public IList<TurnTeamDto> ListByBufferZone(double longitude, double latitude, float radius)
        {
            try
            {
                IList<TurnTeamDto> turnTeams = new List<TurnTeamDto>();
                DateTime dateTime = Helper.GetDateTimeZone();
                dateTime = dateTime.AddHours((int)ETurnsExpiration.HOURS);
                var turns = turnRepository.ListByBufferZone(longitude, latitude, radius, dateTime);
                foreach (Turn turn in turns)
                {
                    Hour hour = turn.Hour;
                    Camp camp = hour.Camp;
                    Team team = turn.Team;
                    DateTime timestamp = new DateTime(turn.Date.Year, turn.Date.Month, turn.Date.Day, hour.Time.Hours, hour.Time.Minutes, hour.Time.Seconds);
                    int playersAmount = team.Players.Where(p => p.ConfirmDate.HasValue).Count();
                    TurnTeamDto turnTeamDto = new TurnTeamDto(turn.Id, team.Name, camp.Name, camp.Street + " " + camp.Number, timestamp, playersAmount);
                    turnTeams.Add(turnTeamDto);
                }
                return turnTeams;
            }
            catch
            {
                throw;
            }
        }

        public IList<HourTurn> List(int campId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                return turnRepository.List(campId, fromDate, toDate);
            }
            catch
            {
                throw;
            }
        }

        public IList<TurnDto> GetReserveList(DateTime date, int hourId, int campId)
        {
            try
            {
                IList<TurnDto> results = new List<TurnDto>();
                var list = turnRepository.ReserveList(date, hourId, campId);
                foreach (Turn turn in list)
                {
                    int? teamId = null;
                    if (turn.Team != null)
                        teamId = turn.Team.Id;
                    TurnDto turnDto = new TurnDto(turn.Id, turn.Date, turn.FullName, turn.Field.Name, teamId);
                    results.Add(turnDto);
                }
                return results;
            }
            catch
            {
                throw;
            }
        }

        public void Confirm(int id)
        {
            try
            {
                Turn turn = turnRepository.Get(id);
                Helper.ThrowIfNull(turn, "El turno no existe");
                DateTime now = Helper.GetDateTimeZone();
                //Helper.ThrowIf(turn.Date < now, "El turno está vencido");
                if (!turn.Success)
                {
                    turnRepository.TransactionManager.BeginTransaction();
                    turn.Success = true;
                    turnRepository.SaveOrUpdate(turn);
                    turnRepository.TransactionManager.CommitTransaction();
                }
            }
            catch
            {
                throw;
            }
        }

        public IList<TurnDto> GetRequestList(DateTime date, int hourId, int campId)
        {
            try
            {
                IList<TurnDto> results = new List<TurnDto>();
                var list = turnRepository.RequestList(date, hourId, campId);
                foreach (Turn turn in list)
                {
                    int? teamId = null;
                    if (turn.Team != null)
                        teamId = turn.Team.Id;
                    TurnDto turnDto = new TurnDto(turn.Id, turn.Date, turn.FullName, turn.Field.Name, teamId);
                    results.Add(turnDto);
                }
                return results;
            }
            catch
            {
                throw;
            }
        }
    }
}
