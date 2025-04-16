
using Saga.Domain.Entities.Systems;
using Saga.DomainShared.Constants;
using Saga.DomainShared;
using Saga.DomainShared.Helpers;
using Microsoft.AspNetCore.Identity;
using Saga.Persistence.Context;
using Saga.Persistence.Models;
using Saga.DomainShared.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Saga.Infrastructure.Interfaces;
using Saga.DomainShared.Enums;
using MediatR;
using Saga.Mediator.Systems.UserManagementMediator;
using System.Linq.Expressions;

namespace Saga.Infrastructure.Services;

public class UserService(
    IDataContext _context,
    SignInManager<ApplicationUser> _signInManager,
    UserManager<ApplicationUser> _userManager,
    ICurrentUser _currentUser,
    IHttpContextAccessor _httpContext,
    Lazy<IAuditLogger> _auditLogger,
    IMediator _mediator
) : IUserService
{
    public async Task<Result> SaveUserProfile(UserProfile userProfile, CancellationToken cancellationToken)
    {
        try
        {
            var defaultPass = "Asdsa.0909";
            // find existing User Profile
            var findProfile = _context.UserProfile.FirstOrDefault(x => x.Key == userProfile.Key);
            
            if(findProfile == null)
            {
                userProfile.TokenIdentity = UserTokenGenerator.GenerateToken(userProfile);
                userProfile.IdentityPass = Crypt.Encrypt(defaultPass);
                _context.UserProfile.Add(userProfile);

                // send email
                var token = StringHelper.EnFilter(userProfile.TokenIdentity);
            }
            else
            {
                _context.UserProfile.Entry(userProfile).CurrentValues.SetValues(userProfile);
            }

            // check existing in Identity
            var findIdentity = await _userManager.FindByEmailAsync(userProfile.Email);
            if (findIdentity == null)
            {
                ApplicationUser user = new()
                {
                    Email = userProfile.Email,
                    UserName = userProfile.Email
                };

                var saveIdentity = await _userManager.CreateAsync(user, defaultPass);
                if (!saveIdentity.Succeeded)
                    throw new Exception(saveIdentity.Errors.Select(x => x.Description).FirstOrDefault());

                userProfile.UserId = Guid.Parse(user.Id);

                // create claims
                List<Claim> claims = [
                    new (ClaimTypes.Email, user.Email)
                ];
                await _userManager.AddClaimsAsync(user, claims);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return await Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return await Task.FromResult(Result.Failure([ex.Message]));
        }
    }

    public async Task<Result> RemoveUserProfile(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var findProfile = await FindUserProfile(x => x.UserId == userId)
                ?? throw new Exception(AppMessageError.UserNotFound);

            // remove User Identity
            var findIdentity = await _userManager.FindByIdAsync(findProfile.UserId.ToString());
            if (findIdentity != null)
                await _userManager.DeleteAsync(findIdentity);

            // remove User Profile
            _context.UserProfile.Remove(findProfile);
            await _context.SaveChangesAsync(cancellationToken);

            await _auditLogger.Value.WriteLoggerAsync(LogMode.Delete, ContentOf.ManagementUser, new { Code = $"{findProfile.Email} {findProfile.UserCategory}"});

            return await Task.FromResult(Result.Success());
        }catch (Exception ex)
        {
            return await Task.FromResult(Result.Failure([ex.Message]));
        }

    }

    public async Task<UserProfile?> FindUserProfile(Expression<Func<UserProfile, bool>> where)
        => await _mediator.Send(new GetUserManagementQuery(where));

    public async Task<Result> SignInAttemp(string Email, string Password)
    {
        CancellationToken cancellationToken = default;

        var password = Crypt.Encrypt(Password);
        try
        {
            //var pass = Crypt.Encrypt("Zxcxz@123");
            var findUserProfile = _context.UserProfile.FirstOrDefault(x => x.Email == Email && x.IdentityPass == password && x.IsConfirmed && x.IsActive)
                ?? throw new Exception(AppMessageError.UserNotFound);

            var user = await _userManager.FindByEmailAsync(findUserProfile.Email)
                ?? throw new Exception(AppMessageError.UserNotFound);

            findUserProfile.Employee = _context.Employees.FirstOrDefault(x => x.Key == findUserProfile.EmployeeKey);

            // ATTEMP
            findUserProfile.LastLogin = DateTime.UtcNow;
            _context.UserProfile.Update(findUserProfile);
            await _context.SaveChangesAsync(cancellationToken);

            await _signInManager.SignInAsync(user, true);

            _httpContext.HttpContext.Session.SetString(SessionLogin.UserId, user.UserId.ToString());
            _httpContext.HttpContext.Session.SetString(SessionLogin.Email, user.Email);
            _httpContext.HttpContext.Session.SetString(SessionLogin.Language, findUserProfile.Language.ToString());

            var claims = await _userManager.GetClaimsAsync(user);
            var claimIdentity = new ClaimsIdentity(claims, "Custom");
            _httpContext.HttpContext.User = new ClaimsPrincipal(claimIdentity);

            var isAuth = _httpContext.HttpContext.User.Identity.IsAuthenticated;

            await _auditLogger.Value.WriteLoggerAsync(LogMode.Read, ContentOf.Login, user.Email );

            return await Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return await Task.FromResult(Result.Failure([ex.Message]));
        }
    }

    public async Task SignOut()
    {
        var user = await _userManager.FindByIdAsync(_currentUser.UserId);
        await _auditLogger.Value.WriteLoggerAsync(LogMode.Read, ContentOf.Logout, user?.Email ?? string.Empty);

        await _httpContext.HttpContext.SignOutAsync();
        await _signInManager.SignOutAsync();
        await Task.CompletedTask;
    }

    public async Task<Result> SavePassword(UserProfile userProfile, string password, CancellationToken cancellationToken)
    {
        try
        {
            var newPassword = Crypt.Encrypt(password);

            var findProfile = await FindUserProfile(x => x.Key == userProfile.Key)
                ?? throw new Exception(AppMessageError.UserNotFound);

            findProfile.IdentityPass = newPassword;
            findProfile.IsConfirmed = true;

            _context.UserProfile.Update(findProfile);

            var findIdentity = await _userManager.FindByIdAsync(findProfile.UserId.ToString());
            if(findIdentity != null)
            {
                var oldPassword = Crypt.Decrypt(userProfile.IdentityPass);
                var result = await _userManager.ChangePasswordAsync(findIdentity, oldPassword, password);
                if (!result.Succeeded)
                    throw new Exception(result.Errors.Select(x => x.Description).FirstOrDefault());
            }

            await _context.SaveChangesAsync(cancellationToken);
            return await Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return await Task.FromResult(Result.Failure([ex.Message]));
        }
    }

    public async Task<Result> ResetPassword(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var findProfile = await FindUserProfile(x => x.Key == userId)
                ?? throw new Exception(AppMessageError.UserNotFound);

            findProfile.TokenResetPassword = UserTokenGenerator.GenerateToken(findProfile);
            _context.UserProfile.Update(findProfile);
            await _context.SaveChangesAsync(cancellationToken);

            // send email
            var token = StringHelper.EnFilter(findProfile.TokenResetPassword);

            return await Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return await Task.FromResult(Result.Failure([ex.Message]));
        }
    }  
}
