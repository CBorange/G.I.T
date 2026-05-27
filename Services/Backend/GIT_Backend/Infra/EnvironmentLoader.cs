using DotNetEnv;
using Npgsql;
using StackExchange.Redis;

namespace GIT_Backend.Infra
{
    public class EnvironmentLoader
    {
        private bool _initialized = false;

        public string LoadMainDBConnectionString()
        {
            EnsureInitialized();

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

        public string LoadRedisConnectionString()
        {
            EnsureInitialized();

            var connectionStringBuilder = new ConfigurationOptions
            {
                Password = GetRequiredValue("Redis_Password"),
                AbortOnConnectFail = false,
            };

            connectionStringBuilder.EndPoints.Add(
                GetRequiredValue("Redis_Host"),
                int.Parse(GetRequiredValue("Redis_Port")));

            return connectionStringBuilder.ToString(includePassword: true);
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }
            var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
            if (File.Exists(envPath))
            {
                Env.NoClobber().Load(envPath);
            }

            _initialized = true;
        }

        private string GetRequiredValue(string key)
        {
            var value = Env.GetString(key, string.Empty);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{key} is required in environment variables or .env.");
            }

            return value;
        }
    }
}
