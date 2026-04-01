using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using CommonModels.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
  private readonly IRepository repository;
  private readonly IMapper mapper;
  
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

  public TicketsController(IRepository repository, IMapper mapper)
  {
    this.repository = repository;
    this.mapper = mapper;
  }
}