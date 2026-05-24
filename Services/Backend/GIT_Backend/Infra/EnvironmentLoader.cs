using DotNetEnv;
using Npgsql;

namespace GIT_Backend.Infra
{
    public class EnvironmentLoader
    {
        public string LoadConnectionString()
        {
            var envPath = Path.Combine(AppContext.BaseDirectory, ".env");

            if (!File.Exists(envPath))
            {
                throw new FileNotFoundException($".env file was not found in executable path: {envPath}", envPath);
            }

            Env.Load(envPath);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = GetRequiredValue("DB_HOST"),
                Port = int.Parse(GetRequiredValue("DB_PORT")),
                Database = GetRequiredValue("DB_NAME"),
                Username = GetRequiredValue("DB_USER"),
                Password = GetRequiredValue("DB_PASSWORD"),
            };

            return connectionStringBuilder.ConnectionString;
        }

        private string GetRequiredValue(string key)
        {
            var value = Env.GetString(key, string.Empty);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{key} is required in .env.");
            }

            return value;
        }
    }
}
