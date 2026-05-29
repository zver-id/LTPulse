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
public class EmployeeController : GenericController
{
  private readonly EmployeeService employeeService;
  private readonly ILogger<EmployeeController> logger;

  [HttpGet]
  public async Task<ActionResult<List<EmployeeDTO>>> GetEmployeesByTeamId(int teamId)
  {
    try
    {
      var employees = await this.employeeService.GetEmployeesByTeamId(teamId);
      return this.Ok(this.Mapper.Map<List<EmployeeDTO>>(employees));
    }
    catch (Exception ex)
    {
      this.logger.LogError(ex, "Error retrieving employees");
      return this.BadRequest("При получении сотрудников по команде произошла ошибка.");
    }
  }

  [HttpPost]
  public async Task<ActionResult> AddEmployeeToTeam(EmployeeDTO employeeDTO, int teamId)
  {
    try
    {
      var employee = this.Mapper.Map<EmployeeDTO, Employee>(employeeDTO);
      await this.employeeService.AddEmployeeToTeam(employee, teamId);
      return this.Ok();
    }
    catch (Exception ex)
    {
      this.logger.LogError(ex, "Error updating employee");
      return this.BadRequest();
    }
  }
  
  public EmployeeController(IRepository repository, IMapper mapper, EmployeeService employeeService, ILogger<EmployeeController> logger) :  base(repository, mapper)
  {
    this.Repository = repository;
    this.Mapper = mapper;
    this.employeeService = employeeService;
    this.logger = logger;
  }
}