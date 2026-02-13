using System.Threading.Tasks;
using MySqlConnector;

namespace mini_pos.Services;

public interface IMySqlConnectionFactory
{
    Task<MySqlConnection> OpenConnectionAsync();
}
