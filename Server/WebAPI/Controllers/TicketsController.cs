using System;
using System.Threading.Tasks;
using Application;
using CommonModels.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
  private readonly IRepository repository;
  
  [HttpGet]
  public async Task<IActionResult> GetTickets(int teamId, DateTime date, string ticketType)
  {
    try
    {
      var service = new TicketService(this.repository);
      var result = await service.GetTickets(teamId, date, ticketType);
      return this.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return this.NoContent();
    }
  }

  public TicketsController(IRepository repository)
  {
    this.repository = repository;
  }
}