using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> GetTickets(int teamId, DateTime date, string ticketType)
  {
    var a = teamId;
    return this.Ok("Not implemented");
    
  }
  
}