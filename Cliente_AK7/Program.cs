using Cliente_AK7.Models;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading;
using System.Data.SqlClient;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;

class Program
{
    static HttpClient client = new HttpClient();

   
    static void Main()
    {
        client.BaseAddress = new Uri("http://apiprogra.somee.com/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (systemWimdos())
        {
            Console.WriteLine("Esta en Sistema Wimdows");
            MonitorRecursosSerividorWin();
            MonitorServicioWin();
        }  else
        {
            Console.WriteLine("Esta en Sistema Linux");

        }




        Console.ReadKey();
    }
    #region Windows
   
    static async void MonitorRecursosSerividorWin()
    {
        Process process = Process.GetCurrentProcess();

        try
        {
            Thread h1 = new Thread(async () =>
            {
                while (true)
                {
                    int espacioC = 0;

                    float monitorCPU = process.TotalProcessorTime.Ticks / (float)Stopwatch.Frequency / Environment.ProcessorCount;
                    Console.WriteLine("CPU en uso: " + monitorCPU * 100);

                    long monitorRAM = process.WorkingSet64;
                    Console.WriteLine("RAM en uso: {0:N0} MB", monitorRAM / 1024);

                    DriveInfo[] discos = DriveInfo.GetDrives();
                    foreach (DriveInfo disco in discos)
                    {
                        if (disco.IsReady)
                        {
                            long esapcioTotal = disco.TotalSize;
                            long espacioLibre = disco.TotalFreeSpace;
                            long espacioUsado = esapcioTotal - espacioLibre;
                            Console.WriteLine("{0} usado: {1:N0} bytes ({2:F2}%)", disco.Name, espacioUsado, (espacioUsado / (float)esapcioTotal) * 100);
                            espacioC = Convert.ToInt32(espacioUsado / 1024 / 1024 / 1024);

                        }
                    }
                    var currentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); ;

                    MonitoreoServidor MS = new MonitoreoServidor()
                    {
                        IdMonitoreo = 0,
                        IdServer = "S_W_1",
                        UsoCpu = Convert.ToInt32(monitorCPU * 100),
                        UsoMemoria = (int)monitorRAM / 1024,
                        UsoEspacio = (int)espacioC,
                        EstadoServer = 1,
                        FechaMonitoreo = DateTime.Parse(currentTime),
                        TimeOut = 3,
                        estadoParam = "normal"
                    };
                    MS = await crearRegistroServidor(MS);
                    Console.WriteLine($"Registrado");

                    //hilo duerme 3 min
                    Thread.Sleep(180000);
                }
            });

            h1.Start();
        }
        catch (Exception ex)
        {

           Console.WriteLine("Error monitoreo Recursos W");
        }
    }
    static async void MonitorServicioWin()
    {
        Process process = Process.GetCurrentProcess();

        try
        {
            Thread h2 = new Thread(async () =>
            {
                while (true)
                {

                    MonitoreoServicio ms = new MonitoreoServicio()
                    {
                        IdMonitoreo = 0,
                        IdServicio = "SVC1",
                        EstadoServicio = 0,
                        FechaMoniServicio = DateTime.Now,
                        TimeOutServicio = 3,
                        estadoParam = "alert"

                    };

                    if (conexionBDNortwhind())
                    {
                        ms.EstadoServicio = 1;
                        ms.estadoParam = "normal";
                    };

                    ms = await crearRegistroServicio(ms);
                    Console.WriteLine($"Servicio Registrado");

                    //hilo duerme 3 min
                    Thread.Sleep(180000);
                }
            });

            h2.Start();
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error en Monitoreo Servicios....: ");
        }
    }

    static async Task<MonitoreoServidor> crearRegistroServidor(MonitoreoServidor c)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("Monitoreo", c);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<MonitoreoServidor>();
    }
    static async Task<MonitoreoServicio> crearRegistroServicio(MonitoreoServicio c) 
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("monitoreoServicio", c);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<MonitoreoServicio>();
    }

   static private bool conexionBDNortwhind()
    {
        SqlConnection conexion = new SqlConnection("server=MVARGASPC\\INSTA12019 ; database=Northwind ; integrated security = true");
        bool conecta = false;

        try
        {
            conexion.Open();
            if (conexion.State > 0 )
            {
                conecta = true;
            }
            conexion.Close();

            return conecta;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error conexion con Northwind ");
            return false;
        }finally { conexion.Close(); }
    }

    static private bool systemWimdos()
    {
         bool esWimdows = false;

        try
        {
            string sistemaOpera = "";

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                        sistemaOpera= "Windows";
                    break;
                case PlatformID.Unix:
                    sistemaOpera= "Linux";
                    break;
                default:
                    sistemaOpera = "Unknown";
                    break;
            }

            if (sistemaOpera.Equals("Windows"))
            {
                esWimdows = true;
            }
            return esWimdows;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al comprobar sistema....");
           return esWimdows;
        }
    }

    #endregion




}//fn class