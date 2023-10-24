using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTOs;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.EndPoints
{
	public static class CouponEndPoints
	{
		public static void ConfigureCouponEndPoints(this WebApplication app)
		{

			app.MapGet("/api/coupon", GetAllCoupon)
				.WithName("GetCoupons")
				.Produces<APIResponse>(200);

			app.MapGet("/api/coupon/{id}", GetCouponById)
				.WithName("GetCoupon")
				.Produces<APIResponse>(200)
				.RequireAuthorization("AdminOnly")
				.AddEndpointFilter(async (context, next) =>
				{
					var id = context.GetArgument<int>(2);
					if(id == 0)
					{
						return Results.BadRequest("Cannot have 0 in id");
					}

					return await next(context);
				});

			app.MapPost("/api/coupon", CreateCoupon)
				.WithName("CreateCoupon")
				.Accepts<CouponCreateDTO>("application/json")
				.Produces<APIResponse>(201)
				.Produces(400);

			app.MapPut("/api/coupon", UpdateCoupon)
				.WithName("UpdateCoupon")
				.Accepts<CouponUpdateDTO>("application/json")
				.Produces<APIResponse>(200)
				.Produces(400);

			app.MapDelete("/api/coupon/{id}", DeleteCoupon).WithName("DeleteCoupon");
		}

		private async static Task<IResult> GetAllCoupon(ICouponRepository _couponRepositoy, ILogger<Program> _logger)
		{
			_logger.Log(LogLevel.Information, "Getting All Coupons");
			APIResponse response = new()
			{
				Result = await _couponRepositoy.GetAllAsync(),
				IsSuccess = true,
				StatusCode = HttpStatusCode.OK
			};
			return Results.Ok(response);
		}
		private async static Task<IResult> GetCouponById(ICouponRepository _couponRepo, int id)
		{
			APIResponse response = new()
			{
				Result = await _couponRepo.GetAsync(id),
				IsSuccess = true,
				StatusCode = HttpStatusCode.OK
			};

			return Results.Ok(response);
		}
		[Authorize]
		private async static Task<IResult> CreateCoupon(IMapper _mapper, ICouponRepository _couponRepo,
				IValidator<CouponCreateDTO> _validator, [FromBody] CouponCreateDTO couponCreateDTO)
		{
			APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
			var validationResult = await _validator.ValidateAsync(couponCreateDTO);
			if (!validationResult.IsValid)
			{
				response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
				return Results.BadRequest(response);
			}
			if (await _couponRepo.GetAsync(couponCreateDTO.Name) is not null)
			{
				response.ErrorMessages.Add("Name Already Exists");
				return Results.BadRequest(response);
			}

			Coupon coupon = _mapper.Map<Coupon>(couponCreateDTO);
			await _couponRepo.CreateAsync(coupon);
			await _couponRepo.SaveAsync();

			response.Result = _mapper.Map<CouponDTO>(coupon);
			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.Created;
			return Results.Ok(response);
		}
		[Authorize]
		private async static Task<IResult> UpdateCoupon(IMapper _mapper, ICouponRepository _couponRepo,
				IValidator<CouponUpdateDTO> _validator, [FromBody] CouponUpdateDTO couponUpdateDTO)
		{
			APIResponse response = new()
			{ IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };


			var validationResult = await _validator.ValidateAsync(couponUpdateDTO);
			if (!validationResult.IsValid)
			{
				response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
				return Results.BadRequest(response);
			}

			await _couponRepo.UpdateAsync(_mapper.Map<Coupon>(couponUpdateDTO));
			await _couponRepo.SaveAsync();

			response.Result = _mapper.Map<CouponDTO>(await _couponRepo.GetAsync(couponUpdateDTO.Id));
			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.OK;
			return Results.Ok(response);

		}
		[Authorize]
		private async static Task<IResult> DeleteCoupon(ICouponRepository _couponRepo, int id)
		{
			APIResponse response = new()
			{ IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

			var couponFromDb = await _couponRepo.GetAsync(id);

			if (couponFromDb is null)
			{
				response.ErrorMessages.Add("Not Item Exists");
				return Results.BadRequest(response);
			}

			await _couponRepo.RemoveAsync(couponFromDb);
			await _couponRepo.SaveAsync();

			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.NoContent;
			return Results.Ok(response);

		}
	}
}





#region Comments

//couponFromDb.IsActive = couponUpdateDTO.IsActive;
//couponFromDb.Name = couponUpdateDTO.Name;
//couponFromDb.Percent = couponUpdateDTO.Percent;
//couponFromDb.LastUpdated = DateTime.Now;
//return Results.CreatedAtRoute($"GetCoupon", new { id = couponDTO.Id} , couponDTO);
//return Results.Created($"/api/coupon/{coupon.Id}" , coupon);
//app.MapGet("/helloworld/{id:int}", (int id) =>
//{
//	return Results.Ok($"id= {id}");
//});
//var summaries = new[]
//{
//	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//	var forecast = Enumerable.Range(1, 5).Select(index =>
//		new WeatherForecast
//		(
//			DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//			Random.Shared.Next(-20, 55),
//			summaries[Random.Shared.Next(summaries.Length)]
//		))
//		.ToArray();
//	return forecast;
//})
//.WithName("GetWeatherForecast")
//.WithOpenApi();

//internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}

#endregion