using Npgsql;
using SignalR_Server_TestCase.Models;

namespace DataAccessLayer
{
    public class NpgSqlAdapter
    {
        const string connString = "Host=localhost;Username=ClientObserver;Password=1989;Database=SignalR_TestServer";

        public IEnumerable<ClientInfoModel> GetAll()
        {
            const string query = "SELECT \"Clients\".ipaddress, memoryusage, memorytotal, cpuusagepercentage, literal, \"Disks\".diskspaceavailable, \"Disks\".diskspacetotal FROM public.\"Clients\" LEFT JOIN \"Disks\" ON \"Clients\".ipaddress = \"Disks\".ipaddress;";
            using (var connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    ClientInfoModel model = null;
                    List<StorageDeviceModel> disks = null;
                    while (reader.Read())
                    {
                        string ip = reader.GetString(0);

                        if (model != null && model.IpAddress != ip)
                        {
                            model.Disks = disks;
                            yield return model;
                            model = null;
                        }

                        if (model == null)
                        {
                            model = new ClientInfoModel();
                            model.IpAddress = ip;
                            model.MemoryUsage = reader.GetFloat(1);
                            model.MemoryTotal = reader.GetFloat(2);
                            model.CPUUsagePercentage = reader.GetFloat(3);
                            disks = new List<StorageDeviceModel>();
                        }
                        disks.Add(new StorageDeviceModel()
                        {
                            IpAddress = ip,
                            Literal = reader.GetString(4).First(),
                            DiskSpaceAvailable = reader.GetFloat(5),
                            DiskSpaceTotal = reader.GetFloat(6)
                        });
                    }
                }
            }
        }
    }
}