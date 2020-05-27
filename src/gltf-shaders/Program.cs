using CommandLine;
using Dapper;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace gltf_shaders
{
    class Program
    {
        static void Main(string[] args)
        {
            var diffuseColor = "#E6008000";
            var specularGlossiness = "4D0000ff";

            var version = Assembly.GetEntryAssembly().GetName().Version;
            Console.WriteLine($"Tool: Add shader info {version}");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Parser.Default.ParseArguments<gltf_shaders.Options>(args).WithParsed(o =>
            {
                o.User = string.IsNullOrEmpty(o.User) ? Environment.UserName : o.User;
                o.Database = string.IsNullOrEmpty(o.Database) ? Environment.UserName : o.Database;

                var connectionString = $"Host={o.Host};Username={o.User};Database={o.Database};Port={o.Port}";
                var istrusted = TrustedConnectionChecker.HasTrustedConnection(connectionString);

                if (!istrusted)
                {
                    Console.Write($"Password for user {o.User}: ");
                    var password = PasswordAsker.GetPassword();
                    connectionString += $";password={password}";
                    Console.WriteLine();
                }

                var conn = new NpgsqlConnection(connectionString);
                var sql = $"select ST_NumGeometries({o.InputGeometryColumn}) as NumberOfGeometries, {o.IdColumn} from {o.Table}";
                var buildings = conn.Query<Building>(sql).ToList(); ;

                Console.WriteLine("Hello World!" + buildings.Count);
                Console.WriteLine($"Id: {buildings[0].Id}, Number of Geometries: {buildings[0].NumberOfGeometries}");

                var shaders = new Shaders();
                var sg = new PbrSpecularGlossiness();
                shaders.PbrSpecularGlossiness = sg;
                var i = 1;

                foreach (var building in buildings)
                {
                    var diffuseColors = Enumerable.Repeat<String>(diffuseColor, building.NumberOfGeometries);
                    var spColors = Enumerable.Repeat<String>(specularGlossiness, building.NumberOfGeometries);
                    sg.DiffuseColors = diffuseColors.ToList();
                    sg.SpecularGlossiness = spColors.ToList();

                    // convert to json...
                    var json = JsonConvert.SerializeObject(shaders, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    var updateSql = $"update {o.Table} set {o.ShadersColumn} = '{json}' where {o.IdColumn}={building.Id}";
                    conn.Execute(updateSql);
                    var perc = Math.Round((double)i / buildings.AsList().Count * 100, 2);
                    Console.Write($"\rProgress: {perc:F}%");
                    i++;

                }

                conn.Close();
            });
        }
    }
}
