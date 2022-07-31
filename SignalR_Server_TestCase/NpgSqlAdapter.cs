using Entities;
using Npgsql;
using SignalR_Server_TestCase.Models;
using System.Text;

namespace SignalR_Server_TestCase
{
    public class NpgSqlAdapter
    {
        const string connString = "Host=127.0.0.1;Username=ClientObserver;Password=1989;Database=SignalR_TestServer";
        public void AddClient(ClientInfoModel client)
        {
            const string query = "INSERT INTO public.\"Clients\"(ipaddress, connectionid, isactive) VALUES(($1),($2),($3));";

            using (var connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection)
                {
                    Parameters =
                    {
                        new() { Value = client.Info.IpAddress },
                        new() { Value = client.ConnectionId },
                        new() { Value = client.IsActive }
                    }
                })

                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        UpdateClient(client);
                    }
                }
            }
        }
        public void UpdateClient(ClientInfoModel client)
        {
            const string query = "UPDATE public.\"Clients\" SET isactive = ($2) WHERE ipaddress = ($1);";
            using (var connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection)
                {
                    Parameters =
                    {
                        new() { Value = client.Info.IpAddress },
                        new() { Value = client.IsActive }
                    }
                })

                {
                    command.ExecuteNonQuery();
                }
            }
        }
        public void AddInfo(ClientInfo info)
        {
            const string query = "INSERT INTO public.\"ClientInfo\" VALUES(($1),($2),($3),($4),NOW());";
            
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection)
                {
                    Parameters =
                    {
                        new() { Value = info.IpAddress },
                        new() { Value = info.MemoryUsage },
                        new() { Value = info.MemoryTotal },
                        new() { Value = info.CPUUsagePercentage }
                    }

                })

                    command.ExecuteNonQuery();

                foreach (Disk disk in info.Disks)
                {
                    AddDiskInfo(disk, connection, info.IpAddress);
                }
            }
        }
        public void AddDiskInfo(Disk disk, NpgsqlConnection connection, string ip)
        {
            const string query = "INSERT INTO public.\"Disks\" VALUES(($1),($2),($3),($4),(SELECT MAX(datetime) FROM public.\"ClientInfo\" WHERE ipaddress = ($1)));";

            using (var command = new NpgsqlCommand(query, connection)
            {
                Parameters =
                    {
                        new() { Value = ip },
                        new() { Value = disk.Literal },
                        new() { Value = disk.DiskSpaceAvailable },
                        new() { Value = disk.DiskSpaceTotal },
                }
            })
                command.ExecuteNonQuery();
        }

        public IEnumerable<ClientInfoModel> GetInfo()
        {
            string query = "SELECT * FROM public.\"allinfo\";";

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
                            yield return new ClientInfoModel() { Info = info };
                            info = null;
                        }

                        if (info == null)
                        {
                            info = new ClientInfo();
                            info.IpAddress = ip;
                            info.MemoryUsage = reader.GetString(1);
                            info.MemoryTotal = reader.GetString(2);
                            info.CPUUsagePercentage = reader.GetString(3);
                            disks = new List<Disk>();
                        }

                        disks.Add(new Disk()
                        {
                            Literal = reader.GetString(4),
                            DiskSpaceAvailable = reader.GetString(5),
                            DiskSpaceTotal = reader.GetString(6)
                        });
                    }
                    info.Disks = disks;
                    yield return new ClientInfoModel() { Info = info };
                }
            }
        }
    }
}