using JWT_Calisma.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JWT_Calisma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppSettings appSettings;

        UserManager<Kullanici> userManager;
        RoleManager<IdentityRole> roleManager;
        SignInManager<Kullanici> signInManager;

        IHttpContextAccessor httpContextAccessor;

        public UserController(IOptions<AppSettings> appSettings, UserManager<Kullanici> userManager, RoleManager<IdentityRole> roleManager, SignInManager<Kullanici> signInManager, IHttpContextAccessor contextAccessor)
        {
            this.appSettings = appSettings.Value;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.httpContextAccessor = contextAccessor;
        }

        [HttpGet]
        [Route("getLoginStatus")]
        public async Task<IActionResult> GetLoginStatus(string userId)
        {
            var userResult = await userManager.FindByIdAsync(userId);
            if (userResult != null)
            {
                var result = signInManager.IsSignedIn(User);
                if (result)
                {
                    return Ok(new string[]
                    {
                        "Login state : true"
                    });
                }
                else
                {
                    return Ok(new string[]
                    {
                        "Login state : false"
                    });
                }
            }
            else
            {
                return BadRequest(new string[]
                {
                    "User not found!"
                });
            }
        }

        [HttpPost]
        [Route("updatePassword")]
        public async Task<IActionResult> UpdatePassword(string userId, string password)
        {
            if (userId != null && password != null && password.Length >= 3)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, token, password);
                    if (result.Succeeded)
                    {
                        return Ok(new string[]
                        {
                            "Password updated!"
                        });
                    }
                    else
                    {
                        return BadRequest(new string[]
                        {
                            "The password could not be updated."
                        });
                    }
                }
                else
                {
                    return BadRequest(new string[]
                    {
                        "User not found!"
                    });
                }
            }
            else
            {
                return BadRequest(new string[]
                {
                    "The password could not be updated."
                });
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginUser([FromBody] UserLogin model)
        {
            if (model.Username != null && model.Password != null)
            {
                await SignOut();

                var resultUser = await userManager.FindByNameAsync(model.Username);
                if (resultUser != null)
                {
                    var resultUserRole = await userManager.GetRolesAsync(resultUser);
                    var resultPassword = await userManager.CheckPasswordAsync(resultUser, model.Password);
                    if (resultPassword != false)
                    {
                        var signInResult = await signInManager.PasswordSignInAsync(resultUser, model.Password, false, false);
                        if (signInResult.Succeeded)
                        {
                            var tokenHandler = new JwtSecurityTokenHandler();
                            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
                            var tokenDescriptor = new SecurityTokenDescriptor
                            {
                                Subject = new ClaimsIdentity(new Claim[]
                                {
                                new Claim(ClaimTypes.Name,resultUser.Id.ToString()),
                                new Claim(ClaimTypes.Role,resultUserRole[0])
                                }),
                                Expires = DateTime.UtcNow.AddHours(1),
                                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Secret)), SecurityAlgorithms.HmacSha256Signature)
                            };

                            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                            var token = tokenHandler.WriteToken(securityToken);

                            TokenProvider.Token = token;

                            CookieOptions option = new CookieOptions();
                            option.Expires = DateTime.UtcNow.AddHours(1);
                            Response.Cookies.Append("info", token.ToString(), option);


                            return Ok(new string[]
                            {
                                "Login success!",token.ToString()
                            });
                        }
                        else
                        {
                            return BadRequest(new string[]
                            {
                                "Login failed! Try again!"
                            });
                        }
                    }
                    else
                    {
                        return BadRequest(new string[]
                        {
                            "Incorrect password!"
                        });
                    }
                }
                else
                {
                    return BadRequest(new string[]
                    {
                        "User not found!"
                    });
                }
            }
            else
            {
                return BadRequest(new string[]
                {
                    "Incorrect model!"
                });
            }
        }

        [HttpPost]
        [Route("deleteUser")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (userId != null)
            {
                var userResult = await userManager.FindByIdAsync(userId);
                if (userResult != null)
                {
                    var result = await userManager.DeleteAsync(userResult);
                    if (result.Succeeded)
                    {
                        return Ok(new string[]
                        {
                            "This user has been deleted!"
                        });
                    }
                    else
                    {
                        return BadRequest(new string[]
                        {
                            "This user could not be delele! Try again!"
                        });
                    }
                }
                else
                {
                    return BadRequest(new string[]
                    {
                        "This user already deleted!"
                    });
                }
            }
            else
            {
                return BadRequest(new string[]
                {
                    "User not found!"
                });
            }
        }

        [HttpPost]
        [Route("getUserInformation")]
        public async Task<IActionResult> GetUserInformation()
        {/*IDX10000 error Code information*/
            try
            {
                var value = httpContextAccessor.HttpContext.Request.Cookies["info"];
                var decoded = (new JwtSecurityTokenHandler()).ReadJwtToken(value).ToString();
                string[] splited = decoded.Split('\"');
                string userId = splited[11];
                return Ok(new string[] { userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new string[] { "Error!", ex.Message });
            }
        }

        [HttpPost]
        [Route("signOut")]
        public async Task<IActionResult> SignOut()
        {
            await signInManager.SignOutAsync();
            Response.Cookies.Delete("info");
            return Ok(new string[]
            {
                "SignOut successfull!"
            });
        }

        [HttpPost]
        [Route("createRole")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (roleName != null)
            {
                var isExist = await roleManager.RoleExistsAsync(roleName);

                if (isExist == true)
                {
                    return BadRequest(new string[]
                    {
                        "This role already added!"
                    });
                }
                else
                {
                    var role = new IdentityRole();
                    role.Name = roleName;

                    var result = await roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        return Ok(new string[]
                        {
                            "Role added!"
                        });
                    }
                    else
                    {
                        return BadRequest(new string[]
                        {
                            "Error while role adding!"
                        });
                    }
                }
            }
            else
            {
                return BadRequest(new string[]
                {
                    "Role name not be null!"
                });
            }
        }

        [HttpPost]
        [Route("addToRole")]
        public async Task<IActionResult> AddToRole(string userId, string roleName)
        {
            if (userId != null && roleName != null)
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        var result = await userManager.AddToRoleAsync(user, roleName);
                        if (result.Succeeded)
                        {
                            return Ok(new string[]
                            {
                                "Assignment of the user to the role was successful."
                            });
                        }
                        else
                        {
                            return BadRequest(new string[]
                            {
                                "An error occurred while assigning the user to the role!"
                            });
                        }
                    }
                    else
                    {
                        return BadRequest(new string[]
                        {
                            "Role not found!"
                        });
                    }
                }
                else
                {
                    return BadRequest(new string[]
                    {
                        "User not found!"
                    });
                }
            }
            else
            {
                return BadRequest(new string[]
                {
                    "An error eccourred while adding!"
                });
            }
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserRegister registerModel)
        {
            if (ModelState.IsValid)
            {
                var kullanici = new Kullanici
                {
                    PhoneNumber = registerModel.Phone,
                    Email = registerModel.Email,
                    UserName = registerModel.Username
                };

                var result = await userManager.CreateAsync(kullanici, registerModel.Password);

                if (result.Succeeded)
                {
                    return Ok(new string[]
                    {
                        "User successfully added!"
                    });
                }
                else
                {
                    ModelState.AddModelError("error:", result.Errors.ToString());
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest(new string[] { "Invalid model state!" });
            }
        }
    }
}
