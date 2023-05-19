using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HomeworkServer
{
    public class Server
    {
        private TcpListener listener;

        public void Start()
        {
            string serverIpAddress = "127.0.0.1";
            int serverPort = 51111;
            listener = new TcpListener(IPAddress.Parse(serverIpAddress), serverPort);
            listener.Start();
            Console.WriteLine("Server started.");

            Task.Run(() => ListenForTeachers());
            Task.Run(() => ListenForStudents());
        }

        private async Task ListenForTeachers()
        {
            try
            {
                while (true)
                {
                    TcpClient teacherClient = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("Teacher connected.");
                    await HandleTeacher(teacherClient);
                    teacherClient.Close();
                    Console.WriteLine("Teacher disconnected.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred in teacher connection: {ex.Message}");
            }
        }
        string dataReceived = "";
        private async Task HandleTeacher(TcpClient teacherClient)
        {
            NetworkStream stream = teacherClient.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
           
            string response = dataReceived;
            byte[] responseData = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(responseData, 0, responseData.Length);
            Console.WriteLine("Response sent to teacher.");
        }

        private async Task ListenForStudents()
        {
            try
            {
                while (true)
                {
                    TcpClient studentClient = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("Student connected.");
                    await HandleStudent(studentClient);
                    studentClient.Close();
                    Console.WriteLine("Student disconnected.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred in student connection: {ex.Message}");
            }
        }

        private async Task HandleStudent(TcpClient studentClient)
        {
            NetworkStream stream = studentClient.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Student sent: {dataReceived}");
            string response = dataReceived;
            byte[] responseData = Encoding.ASCII.GetBytes(dataReceived);
            await stream.WriteAsync(responseData, 0, responseData.Length);
            Console.WriteLine("Response sent to student.");
        }

        public void Stop()
        {
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }

    public class Program
    {
        public static void Main()
        {
            Server server = new Server();
            server.Start();
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
            server.Stop();
        }
    }
}