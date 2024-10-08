﻿using AidManager.API.Authentication.Domain.Model.Commands;
using AidManager.API.Authentication.Domain.Model.Entities;
using AidManager.API.Authentication.Domain.Repositories;
using AidManager.API.Authentication.Domain.Services;
using AidManager.API.Shared.Domain.Repositories;
using AidManager.API.UserManagement.UserProfile.Application.Internal.OutboundServices.ACL;
using AidManager.API.UserProfile.Domain.Model.Commands;
using Microsoft.EntityFrameworkCore.Storage;

namespace AidManager.API.Authentication.Application.Internal.CommandServices;

public class UserCommandService(IUserRepository userRepository, IUnitOfWork unitOfWork, ExternalUserAuthService externalUserAuthService) : IUserCommandService
{
    public async Task<User?> Handle(CreateUserCommand command)
    {
        try
        {
            var validate = await userRepository.FindUserByEmail(command.Email);
                //se valida q no esta el email en la base de datos.
            if ( validate != null) //si se encuentra error
            {
                Console.WriteLine("EMAIL ALREADY USED"); 
                throw new Exception("Error: User EMAIL already exists");
            }

            var user = new User(command);
            
            await externalUserAuthService.CreateUsername(user.Email, user.Password, user.Role);

            var userid = await externalUserAuthService.FetchUserIdByUsername(user.Email);
           
            
            
            
            
            switch (command.Role)
            {
                //Manager
                case 0:
                    //externalCompanyAuthService.CreateCompany
                    await externalUserAuthService.CreateCompany(command.CompanyName, command.CompanyCountry, command.CompanyEmail, userid);
                    var company = await externalUserAuthService.FetchCompanyByUserId(userid);
                    user.CompanyId = company.Id;
                    break;
                //TeamMember
                case 1:
                    //externalCompanyAuthService.GetCompanyInfoByCompanyRegisterCode
                    var companyData = await externalUserAuthService.AuthenticateCode(command.TeamRegisterCode);
                    user.CompanyId = companyData.Id;
                    user.CompanyName = companyData.CompanyName;
                    break;
            } 
            
            Console.WriteLine("USER: " + user);
            
            await userRepository.AddAsync(user);
            await unitOfWork.CompleteAsync();
            return user;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error in creation: " + e.Message);
            throw;
        }
        
    }
    
    public async Task<User> Handle(UpdateUserCommand command, string email)
    {
        var user = await userRepository.FindUserByEmail(email);
        if (user != null)
        {
            user.updateProfile(command);
            await externalUserAuthService.UpdateUser(user.Email, user.Password, user.Role);
                    await userRepository.Update(user);
                    await unitOfWork.CompleteAsync();
                    return user;
        }
        throw new Exception("User not found");
    }

    public async Task<bool> AuthenticateUser(ValidateUserCredentialsCommand command)
    {
        var user = await userRepository.FindUserByEmail(command.Email);
        if (user == null)
        {
            return false;
        }
        
        Console.WriteLine("User found in UserRepository");
        Console.WriteLine(user.Password + " == " + command.Password);
        return user.Password == command.Password;
    }
    
    public async Task<bool> Handle(EditCompanyIdCommand command, int companyId)
    {
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user != null)
        {
            user.CompanyId = companyId;
            await userRepository.Update(user);
            await unitOfWork.CompleteAsync();
            return true;
        }

        return false;
        
    }
    
    public async Task<bool> Handle(KickUserByCompanyIdCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user != null)
        {
            await userRepository.Remove(user);
            await unitOfWork.CompleteAsync();
            return true;
        }

        return false;
    }
    
    public async Task<User?> Handle(UpdateUserCompanyNameCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user != null)
        {
            user.CompanyName = command.CompanyName;
            await userRepository.Update(user);
            await unitOfWork.CompleteAsync();
            return user;
        }

        return null;
    }
}