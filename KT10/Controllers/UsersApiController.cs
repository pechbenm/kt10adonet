using KT10.Models;
using KT10.Services;
using Microsoft.AspNetCore.Mvc;

namespace KT10.Controllers;

[ApiController]
[Route("api/users")]
public class UsersApiController : ControllerBase
{
    private readonly IUserService _users;

    public UsersApiController(IUserService users) => _users = users;

    // GET api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        var users = await _users.GetAllAsync();
        return Ok(users.Select(UserResponse.From));
    }

    // GET api/users/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await _users.GetByIdAsync(id);
        if (user is null) return Fail(UserOpStatus.NotFound, id);

        return Ok(UserResponse.From(user));
    }

    // POST api/users
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var result = await _users.CreateAsync(request.Username, request.Email, request.Password);
        if (result.Status != UserOpStatus.Ok) return Fail(result.Status);

        var body = UserResponse.From(result.User!);
        return CreatedAtAction(nameof(GetById), new { id = body.Id }, body);   // 201 + заголовок location
    }

    // PUT api/users/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> Update(int id, UpdateUserRequest request)
    {
        var result = await _users.UpdateAsync(id, request.Username, request.Email, request.Password);
        if (result.Status != UserOpStatus.Ok) return Fail(result.Status, id);

        return Ok(UserResponse.From(result.User!));
    }

    // DELETE api/users/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _users.DeleteAsync(id);
        if (result.Status != UserOpStatus.Ok) return Fail(result.Status, id);

        return NoContent();
    }

    //единое место, где статус операции превращается в HTTP-ответ
    private ObjectResult Fail(UserOpStatus status, int id = 0) => status switch
    {
        UserOpStatus.NotFound => Problem(
            title: "Пользователь не найден",
            detail: $"Пользователя с Id={id} не существует.",
            statusCode: StatusCodes.Status404NotFound),

        UserOpStatus.UsernameTaken => Problem(
            title: "Конфликт данных",
            detail: "Имя пользователя уже занято.",
            statusCode: StatusCodes.Status409Conflict),

        UserOpStatus.EmailTaken => Problem(
            title: "Конфликт данных",
            detail: "Пользователь с таким email уже существует.",
            statusCode: StatusCodes.Status409Conflict),

        _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
    };
}