using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace ExcelUploadPortal.Services;
public class LdapAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LdapAuthenticationService> _logger;

    public LdapAuthenticationService(IConfiguration configuration, ILogger<LdapAuthenticationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool ValidateUser(string username, string password)
    {
        try
        {
            string domain = _configuration["LDAP:Domain"];

            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                return context.ValidateCredentials(username, password);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"LDAP Authentication Error: {ex.Message}");
            return false;
        }
    }
}
