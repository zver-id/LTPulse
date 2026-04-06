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
public class TicketsController : ControllerBase
{
  private readonly IRepository repository;
  private readonly IMapper mapper;
  private readonly ILogger<TicketsController> logger;
  
  [HttpGet]
  public async Task<ActionResult<List<TicketDTO>>> GetTickets(int teamId, DateTime date, string ticketType)
  {
    try
    {
      var service = new TicketService(this.repository);
      var ticketList = await service.GetTickets(teamId, date, ticketType);
      var result = this.mapper.Map<List<TicketDTO>>(ticketList);
      return this.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return this.NoContent();
    }
  }

  [HttpPost]
  public async Task<ActionResult<TicketDTO>> UpdateTicket(TicketDTO ticketDTO)
  {
    try
    {
      var service = new TicketService(this.repository);
      var ticket = this.repository.GetById<Ticket>(ticketDTO.Key);
      if  (ticket == null)
        return this.BadRequest("Этого обращения нет на сервере");
      
      // маппим только простые поля так как по сути нужно обновить только комментарий.
      this.mapper.Map(ticketDTO, ticket);
      await service.UpdateTicket(ticket);
      return this.Ok(ticketDTO);
    }
    catch (Exception ex)
    {
      this.logger.LogError(ex, ex.Message);
      return this.BadRequest("Ошибка при обработке запроса");
    }
    
  }

  public TicketsController(IRepository repository, IMapper mapper, ILogger<TicketsController> logger)
  {
    this.repository = repository;
    this.mapper = mapper;
    this.logger = logger;
  }
}