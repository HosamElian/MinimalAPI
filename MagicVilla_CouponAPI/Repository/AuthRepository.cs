using AutoMapper;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTOs;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_CouponAPI.Repository
{
	public class AuthRepository : IAuthRepository
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly IConfiguration _configuration;
		private string sectetKey;
		public AuthRepository(ApplicationDbContext db,
			IMapper mapper,
			IConfiguration configuration)
		{
			_db = db;
			_mapper = mapper;
			_configuration = configuration;
			sectetKey = _configuration.GetValue<string>("ApiSettings:Secret");
		}
		public bool IsUniqueUser(string username)
		{
			return _db.LocalUsers.FirstOrDefault(x => x.UserName == username) == null ? true : false; 
		}

		public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
		{
			var userFromDb = _db.LocalUsers.FirstOrDefault(x => x.UserName == loginRequestDTO.UserName
															&& x.Password == loginRequestDTO.Password);

			if (userFromDb == null)
				return null;

			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(sectetKey);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				 {
					 new Claim(ClaimTypes.Name, userFromDb.UserName),
					 new Claim(ClaimTypes.Role, userFromDb.Role),

				 }),
				Expires = DateTime.Now.AddDays(7),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			LoginResponseDTO loginResponseDTO = new()
			{
				User = _mapper.Map<UserDTO>(userFromDb),
				Token = tokenHandler.WriteToken(token)
			};

			return loginResponseDTO;
		}

		public async Task<UserDTO> Register(RegisterationRequestDTO requestDTO)
		{
			LocalUser userObj = new()
			{
				UserName = requestDTO.UserName,
				Name = requestDTO.Name,
				Password = requestDTO.Password,
				Role = "admin"
			};
			_db.LocalUsers.Add(userObj);
			await _db.SaveChangesAsync();

			userObj.Password = "";
			return _mapper.Map<UserDTO>(userObj);
		}
	}
}
