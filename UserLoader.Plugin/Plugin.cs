using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using UserLoader.Plugin.Responses;

namespace UserLoader.Plugin
{
    [Author(Name = "Nikolai Marchenko")]
    public class Plugin : IPluggable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            logger.Info("Fetching employees from external API...");

            List<EmployeesDTO> newEmployees = new List<EmployeesDTO>();

            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync("https://dummyjson.com/users").GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        logger.Error($"API returned status code {response.StatusCode}");
                        return args;
                    }

                    var json = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<UserResponse>(json);

                    foreach (var user in result.Users)
                    {
                        var employee = new EmployeesDTO
                        {
                            Name = $"{user.FirstName} {user.LastName}"
                        };
                        try
                        {
                            employee.AddPhone(user.Phone);
                        }
                        catch
                        {
                            logger.Warn($"Invalid phone for {employee.Name}");
                        }

                        newEmployees.Add(employee);
                    }

                    logger.Info($"Loaded {newEmployees.Count} users from API.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while loading users");
            }

            return args.Concat(newEmployees).Cast<DataTransferObject>();
        }
    }
}
