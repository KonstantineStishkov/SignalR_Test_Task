using Entities;
using Npgsql;
using SignalR_Server_TestCase.Models;
using System.Text;

namespace SignalR_Server_TestCase
{
    public class NpgSqlAdapter
    {
        const string connString = "Host=127.0.0.1;Username=ClientObserver;Password=1989;Database=SignalR_TestServer";
        public void AddClient(ClientModel client)
        {
            const string query = "INSERT INTO public.\"Clients\"(ipaddress, connectionid, isactive) VALUES(($1),($2),($3));";

            using (var connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection)
                {
                    Parameters =
                    {
                        new() { Value = client.IpAddress },
                        new() { Value = client.ConnectionId },
                        new() { Value = client.IsActive }
                    }
                })

                {
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateClient(ClientModel client)
        {
            const string query = "UPDATE public.\"Clients\" SET isactive = ($2) WHERE ipaddress = ($1);";
            using (var connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection)
                {
                    Parameters =
                    {
                        new() { Value = client.IpAddress },
                        new() { Value = client.IsActive }
                    }
                })

                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<ClientInfo> GetInfo()
        {
            string query = GetQueryString();

            using (var connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    NpgsqlDataReader? reader = command.ExecuteReader();
                    ClientInfo info = null;
                    List<Disk> disks = null;
                    while (reader.Read())
                    {
                        string ip = reader.GetString(0);

                        if (info != null && info.IpAddress != ip)
                        {
                            info.Disks = disks;
                            yield return info;
                            info = null;
                        }

                        if (info == null)
                        {
                            info = new ClientInfo();
                            info.IpAddress = ip;
                            info.MemoryUsage = (long)reader.GetFloat(1);
                            info.MemoryTotal = (long)reader.GetFloat(2);
                            info.CPUUsagePercentage = reader.GetFloat(3);
                            disks = new List<Disk>();
                        }

                        disks.Add(new Disk()
                        {
                            Literal = reader.GetString(4).First(),
                            DiskSpaceAvailable = reader.GetFloat(5),
                            DiskSpaceTotal = reader.GetFloat(6)
                        });
                    }
                }
            }
        }

        private string GetQueryString()
        {
            var stb = new StringBuilder();
            stb.Append("SELECT \"ClientInfo\".ipaddress, ");
            stb.Append("memoryusage, ");
            stb.Append("memorytotal, ");
            stb.Append("cpuusagepercentage, ");
            stb.Append("\"Disks\".literal, ");
            stb.Append("\"Disks\".diskspaceavailable, ");
            stb.Append("\"Disks\".diskspacetotal ");
            stb.Append("FROM ( SELECT ipaddress, MAX(datetime) as dat ");
            stb.Append("FROM public.\"ClientInfo\" GROUP BY ipaddress) as maximum ");
            stb.Append("INNER JOIN public.\"ClientInfo\" ON maximum.ipaddress = \"ClientInfo\".ipaddress ");
            stb.Append("AND maximum.dat = \"ClientInfo\".datetime ");
            stb.Append(" LEFT JOIN \"Disks\" ON \"ClientInfo\".ipaddress = \"Disks\".ipaddress; ");
            return stb.ToString();
        }
    }
}