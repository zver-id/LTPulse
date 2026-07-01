using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using CommonModels.Interfaces;
using CommonModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.DTO;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController(IRepository repository, IMapper mapper, ILogger<TicketsController> logger)
  : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<List<TicketDTO>>> GetTickets(int teamId, DateTime date, string ticketType)
  {
    try
    {
      var service = new TicketService(repository);
      var ticketList = await service.GetTickets(teamId, date, ticketType);
      var result = mapper.Map<List<TicketDTO>>(ticketList);
      return this.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return this.NoContent();
    }
  }
  
    [HttpGet("byMetric")]
    public async Task<ActionResult<List<TicketDTO>>> GetTicketsByMetric(DateTime date, string metricType, int teamId)
    {
      try
      {
        var service = new TicketService(repository);
        List<Ticket> ticketList = await service.GetTicketsByMetric(teamId, date, metricType);
        var result = mapper.Map<List<TicketDTO>>(ticketList);
        return this.Ok(result);
      }
      catch (InvalidOperationException ex)
      {
          return this.NoContent();
      }
    }

  [HttpPost]
  public async Task<ActionResult<TicketDTO>> UpdateTicket(TicketDTO ticketDTO)
  {
    try
    {
      var service = new TicketService(repository);
      var ticket = await repository.GetById<Ticket>(ticketDTO.Key);
      if  (ticket == null)
        return this.BadRequest("Этого обращения нет на сервере");
      
      // маппим только простые поля так как по сути нужно обновить только комментарий.
      mapper.Map(ticketDTO, ticket);
      await service.AddOrUpdateTicket(ticket);
      return this.Ok(ticketDTO);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, ex.Message);
      return this.BadRequest("Ошибка при обработке запроса");
    }
    
  }
}