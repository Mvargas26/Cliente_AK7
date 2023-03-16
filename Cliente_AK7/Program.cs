using Cliente_AK7.Models;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading;
using System.Data.SqlClient;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Net.Sockets;
using System.Net;

class Program
{
    //VARIABLES GLOBALES
    static HttpClient client = new HttpClient();
    private static NetworkStream soc_stream;
    private static BinaryWriter bw_Escritor;
    private static BinaryReader br_Lector;

    static void Main()
    {
        client.BaseAddress = new Uri("http://apiprogra.somee.com/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (systemWimdos())
        {
            Thread h1 = new Thread(async () =>
            {
                Console.WriteLine("Esta en Sistema Windows");

                while (true)
                {
                    MonitorRecursosSerividorWin();
                    MonitorServicioWin_BD1();
                    MonitorServicioWin_BD2();
                    Thread.Sleep(10000);
                }
            });
            h1.Start();

            Console.ReadKey();
        } 
        else
        {
            Thread h1 = new Thread(async () =>
            {
                Console.WriteLine("Esta en Sistema Linux");

                while (true)
                {
                    MonitorRecursosSerividor_Linux();
                    MonitorServicioLin_Socket();

                    Thread.Sleep(180000);
                }
            });
            h1.Start();
        }
    }//FN MAIN



    #region Windows
   
    static async void MonitorRecursosSerividorWin()
    {
        Process process = Process.GetCurrentProcess();

        try
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
            Console.WriteLine($"Registrado Server");
        }
        catch (Exception ex )
        {
            registroBitacora(ex.Message);
           Console.WriteLine("Error monitoreo Recursos W");
        }
    }
    static async void MonitorServicioWin_BD1()
    {
        try
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
            Console.WriteLine($"Servicio 1 Registrado");
        }
        catch (Exception ex  )
        {
            registroBitacora(ex.Message);
            Console.WriteLine("Error en Monitoreo Servicios....: Base de datos 1");
        }
    }
    static async void MonitorServicioWin_BD2()
    {
        try
        {
            MonitoreoServicio ms = new MonitoreoServicio()
            {
                IdMonitoreo = 0,
                IdServicio = "SVC2",
                EstadoServicio = 0,
                FechaMoniServicio = DateTime.Now,
                TimeOutServicio = 3,
                estadoParam = "alert"

            };

            if (conexionBDTarea4AK7())
            {
                ms.EstadoServicio = 1;
                ms.estadoParam = "normal";
            };

            ms = await crearRegistroServicio(ms);
            Console.WriteLine($"Servicio 2 Registrado");
        }
        catch (Exception ex)
        {
            registroBitacora(ex.Message);
            Console.WriteLine("Error en Monitoreo Servicios....: Base datos 2 ");
        }
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
            registroBitacora(ex.Message);
            Console.WriteLine("Error conexion con Northwind ");
            return false;
        }finally { conexion.Close(); }
    }
    static private bool conexionBDTarea4AK7()
    {
        SqlConnection conexion = new SqlConnection("server=MVARGASPC\\INSTA12019 ; database=Tarea4_AK7 ; integrated security = true");
        bool conecta = false;

        try
        {
            conexion.Open();
            if (conexion.State > 0)
            {
                conecta = true;
            }
            conexion.Close();

            return conecta;
        }
        catch (Exception ex)
        {
            registroBitacora(ex.Message);
            Console.WriteLine("Error conexion con Tarea4_AK7 ");
            return false;
        }
        finally { conexion.Close(); }
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
            registroBitacora(ex.Message);
            Console.WriteLine("Error al comprobar sistema....");
           return esWimdows;
        }
    }

    #endregion

    #region Linux

    static async void MonitorRecursosSerividor_Linux()
    {

        try
        {
            float CPUusado = usoCPULinux();
            Console.WriteLine("uso CPU : " + CPUusado + "%");

            float RAMusada = usoRAMlinux();
            Console.WriteLine("uso RAM : " + RAMusada + "%");

            float DISCOusado = usoDISCOlinux();
            Console.WriteLine("uso Disk : " + DISCOusado + "%");

            MonitoreoServidor MS = new MonitoreoServidor()
            {
                IdMonitoreo = 0,
                IdServer = "S_L_1",
                UsoCpu = Convert.ToInt32(CPUusado),
                UsoMemoria = (int)RAMusada,
                UsoEspacio = (int)DISCOusado,
                EstadoServer = 1,
                FechaMonitoreo = DateTime.Now,
                TimeOut = 3,
                estadoParam = "normal"
            };

            await crearRegistroServidor(MS);
            Console.WriteLine($"Registrado Server");
        }
        catch (Exception ex)
        {
            registroBitacora(ex.Message);
            Console.WriteLine("Error monitoreo Recursos L");
        }

    }

    static float usoCPULinux()
    {

        ProcessStartInfo psi = new ProcessStartInfo("bash", "-c \"top -b -n1 | grep 'Cpu(s)'\"");
        psi.RedirectStandardOutput = true;
        Process proc = Process.Start(psi);
        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        float usoCPU = float.Parse(output.Split(new char[] { ' ', '%' }, StringSplitOptions.RemoveEmptyEntries)[1]);

        return usoCPU;
    }

    static float usoRAMlinux()
    {
        ProcessStartInfo psi = new ProcessStartInfo("bash", "-c \"free | grep Mem\"");
        psi.RedirectStandardOutput = true;
        Process proc = Process.Start(psi);
        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        string[] values = output.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        float total = float.Parse(values[1]);
        float used = float.Parse(values[2]);
        float usoRAM = used / total * 100;

        return usoRAM;
    }

    static float usoDISCOlinux()
    {
        ProcessStartInfo psi = new ProcessStartInfo("bash", "-c \"df -h /\"");
        psi.RedirectStandardOutput = true;
        Process proc = Process.Start(psi);
        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        // parse the disk usage from the output
        string[] values = output.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        float total = float.Parse(values[8].Replace("G", ""));
        float used = float.Parse(values[9].Replace("G", ""));
        float usoDISco = used / total * 100;

        return usoDISco;
    }

    public static bool EjecutarClientesocket()
    {
        bool socketUP = false;

        TcpClient tcp_Cliente;

        try
        {
            tcp_Cliente = new TcpClient();
            tcp_Cliente.Connect("127.0.0.1", 12345);

            soc_stream = tcp_Cliente.GetStream();
            bw_Escritor = new BinaryWriter(soc_stream);
            br_Lector = new BinaryReader(soc_stream);

            string mensaje_Servidor = null;
            do
            {
                try
                {

                    mensaje_Servidor = br_Lector.ReadString();
                    Console.WriteLine(mensaje_Servidor + "\n");

                    if (mensaje_Servidor.Equals("Server_UP"))
                    {
                        socketUP = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    registroBitacora(ex.Message);
                    Console.WriteLine("Fin conexion Socket");
                }

            } while (mensaje_Servidor != "Server >>> Salir");
            Console.WriteLine("El Servidor Termino la conexion");
            bw_Escritor.Close();
            br_Lector.Close();
            tcp_Cliente.Close();
            soc_stream.Close();

            return socketUP;
        }
        catch (Exception ex)
        {
            registroBitacora(ex.Message);
            Console.WriteLine("Fin conexion Socket...");
            return socketUP;
        }
    }//fin metodo ejecutar cliente
    static async void MonitorServicioLin_Socket()
    {
        try
        {
            MonitoreoServicio ms = new MonitoreoServicio()
            {
                IdMonitoreo = 0,
                IdServicio = "SVCL1",
                EstadoServicio = 0,
                FechaMoniServicio = DateTime.Now,
                TimeOutServicio = 3,
                estadoParam = "alert"

            };

            if (EjecutarClientesocket())
            {
                ms.EstadoServicio = 1;
                ms.estadoParam = "normal";
            };

            ms = await crearRegistroServicio(ms);
            Console.WriteLine($"Servicio 1 Linux Registrado");
        }
        catch (Exception ex)
        {
            registroBitacora(ex.Message);
            Console.WriteLine("Error en Monitoreo Servicios....: Socket ");
        }
    }
    #endregion


    #region Compartidos
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
    static void registroBitacora(string ErrorGuardar)
    {
        try
        {
            string urlEscritorio = "";

            if (systemWimdos())
            {
                 urlEscritorio = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else
            {
               urlEscritorio = Directory.GetCurrentDirectory();
            }

            string archivo = Path.Combine(urlEscritorio, "BitacoraInterna_Cliente_PrograV.txt");

            if (File.Exists(archivo))
            {
                File.AppendAllText(archivo, $"{DateTime.Now}--Error: " + ErrorGuardar + "\n");
            }
            else
            {
                File.WriteAllText(archivo, $"{DateTime.Now}--Error: " + ErrorGuardar + "\n");
            }
        }
        catch (Exception)
        {

           Console.WriteLine("Error en la bitacora");
        }
       
    }
    #endregion
}//fn class