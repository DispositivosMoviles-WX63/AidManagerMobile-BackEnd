﻿using AidManager.API.Authentication.Domain.Model.Aggregates;
using AidManager.API.Authentication.Interfaces.ACL;
using AidManager.API.IAM.Interfaces.ACL;

namespace AidManager.API.UserManagement.UserProfile.Application.Internal.OutboundServices.ACL;

public class ExternalUserAuthService(IIamContextFacade iamContextFacade, IAuthenticationFacade authenticationFacade)
{
    public async Task CreateUsername(string username, string password, int role)
    {
        try
        {
            await iamContextFacade.CreateUser(username, password, role);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task UpdateUser(string username, string password, int role)
    {
        await iamContextFacade.UpdateUserData(username, password, role);
    }
    
    public async Task<int> FetchUserIdByUsername(string username)
    {
        return await iamContextFacade.FetchUserIdByUsername(username);
    }
    
    public async Task<string> FetchUsernameByUserId(int userId)
    {
        return await iamContextFacade.FetchUsernameByUserId(userId);
    }
    
    public async Task<Company> AuthenticateCode(string registerCode)
    {
        var company = await authenticationFacade.ValidateRegisterCode(registerCode);
        if (company == null)
        {
            throw new Exception("AUTH ERROR: Register Code NOT Valid");
        }

        return company;
    }
    
    public async Task<Company> FetchCompanyByUserId(int managerId)
    {
        var company = await authenticationFacade.GetCompanyByManagerId(managerId);
        if (company == null)
        {
            throw new Exception("ERROR Company not found by Manager ID");
        }

        return company;
    }
    
    public async Task<bool> CreateCompany(string companyName, string country, string email, int userId)
    {
        try
        {
            return await authenticationFacade.CreateCompany(companyName, country, email, userId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    
    
}