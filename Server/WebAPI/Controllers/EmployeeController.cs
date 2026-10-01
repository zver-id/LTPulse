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

  /// <summary>
  /// Получить всех сотрудников (для админского выбора при включении в команду).
  /// </summary>
  /// <returns>Список всех сотрудников.</returns>
  [HttpGet("all")]
  public async Task<ActionResult<List<EmployeeDTO>>> GetAllEmployees()
  {
    try
    {
      var employees = await this.employeeService.GetAllEmployees();
      return this.Ok(this.Mapper.Map<List<EmployeeDTO>>(employees));
    }
    catch (Exception ex)
    {
      this.logger.LogError(ex, "Error retrieving all employees");
      return this.BadRequest("При получении списка сотрудников произошла ошибка.");
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

  /// <summary>
  /// Исключить сотрудника из команды.
  /// </summary>
  /// <param name="employeeId">Идентификатор сотрудника.</param>
  /// <param name="teamId">Идентификатор команды.</param>
  /// <returns>Ok, если сотрудник исключен.</returns>
  [HttpPost("remove/{teamId:int}")]
  public async Task<ActionResult> RemoveEmployeeFromTeam(int employeeId, int teamId)
  {
    try
    {
      await this.employeeService.RemoveEmployeeFromTeam(employeeId, teamId);
      return this.Ok();
    }
    catch (ArgumentException ex)
    {
      this.logger.LogError(ex, "Error removing employee from team");
      return this.BadRequest("Сотрудника нет в указанной команде или команда не найдена.");
    }
    catch (Exception ex)
    {
      this.logger.LogError(ex, "Error removing employee from team");
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