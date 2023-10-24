using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTOs;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using System.Net;

namespace MagicVilla_CouponAPI.EndPoints
{
	public static class AuthEndPoints
	{
		public static void ConfigureAuthEndPoints(this WebApplication app)
		{



			app.MapPost("/api/login", Login)
				.WithName("Login")
				.Accepts<LoginRequestDTO>("application/json")
				.Produces<APIResponse>(200)
			.Produces(400);

			app.MapPost("/api/register", Register)
				.WithName("Register")
				.Accepts<APIResponse>("application/json")
				.Produces<LoginResponseDTO>(200)
				.Produces(400);

		}


		private async static Task<IResult> Login(IAuthRepository _authRepo, [FromBody] LoginRequestDTO model)
		{
			APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
			var loginResponse = await _authRepo.Login(model);

			if( loginResponse == null)
			{
				response.ErrorMessages.Add("Username or Password is incorrect");
				return Results.BadRequest(response);
			}

			response.Result = loginResponse;
			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.OK;
			return Results.Ok(response);
		}

		private async static Task<IResult> Register(IAuthRepository _authRepo, [FromBody] RegisterationRequestDTO model)
		{
			APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
			
			bool isUserNameIsUnique = _authRepo.IsUniqueUser(model.UserName);
			if(!isUserNameIsUnique)
			{
				response.ErrorMessages.Add("Username is Already Exists");
				return Results.BadRequest(response);
			}

			var registertionResponse = await _authRepo.Register(model);
			if (registertionResponse is null || string.IsNullOrEmpty(registertionResponse.Name))
				return Results.BadRequest(response);

			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.OK;
			return Results.Ok(response);
		}

	}
}
