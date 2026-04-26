using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CommonModels.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : GenericController
{
  public async Task<ActionResult<List<EmployeeDTO>>> GetEmployeesByTeamId(int teamId)
  {
    
    return this.BadRequest();
  }
  
  public EmployeeController(IRepository repository, IMapper mapper) :  base(repository, mapper)
  {
    this.Repository = repository;
    this.Mapper = mapper;
  }
}